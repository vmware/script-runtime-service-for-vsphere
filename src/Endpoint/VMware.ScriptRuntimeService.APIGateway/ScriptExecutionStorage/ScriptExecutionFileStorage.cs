// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Abstractions;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage {
   public class ScriptExecutionFileStorage : IScriptExecutionStorage, IDisposable {
      private ILogger _logger;
      private ILoggerFactory _loggerFactory;
      private IFileSystem _fileSystem;
      private IScriptExecutionStoreProviderFactory _scriptExecutionWriterFactory;
      private IPollingScriptExecutionPersisterFactory _scriptExecutionPersisterFactory;
      private string _rootFolder;
      private ScriptExecutionStorageSettings _lastSttingsConfiguration;
      private object _retentionPolicyUpdateLock = new object();

      // Retention Policy fields
      private FolderScriptExecutionsRetentionPolicy _retentionPolicy;
      private const int DEFAULT_NUMBER_OF_SCRIPTS_PER_USER = 30;
      private const int DEFAULT_NO_OLDER_THAN_DAYS = 5;
      private Timer _retentionPolicyApplierTimer;

      private Dictionary<string, IScriptExecutionStoreProvider> _scriptIdToFileStoreProvider =
         new Dictionary<string, IScriptExecutionStoreProvider>();
      private Dictionary<string, IPollingScriptExecutionPersister> _scriptIdToPollingPersister =
         new Dictionary<string, IPollingScriptExecutionPersister>();

      public ScriptExecutionFileStorage(ILoggerFactory loggerFactory, ScriptExecutionStorageSettings settings)
         : this(loggerFactory, settings, new FileSystem(), new ScriptExecutionFileStoreProviderFactory(), new PollingScriptExecutionPersisterFactory()) { }

      public ScriptExecutionFileStorage(
         ILoggerFactory loggerFactory,
         ScriptExecutionStorageSettings settings, 
         IFileSystem fileSystem,
         IScriptExecutionStoreProviderFactory scriptExecutionWriterFactory,
         IPollingScriptExecutionPersisterFactory scriptExecutionPersisterFactory) {

         _lastSttingsConfiguration = settings;
         _loggerFactory = loggerFactory;
         _logger = loggerFactory.CreateLogger(typeof(ScriptExecutionFileStorage));
         _rootFolder = settings.ServiceScriptStorageDir;
         _fileSystem = fileSystem;
         _scriptExecutionWriterFactory = scriptExecutionWriterFactory;
         _scriptExecutionPersisterFactory = scriptExecutionPersisterFactory;

         _logger.LogInformation("Creating Script Execution Storage with Settings");
         _logger.LogInformation($"   StorageDir: {settings.ServiceScriptStorageDir}");
         _logger.LogInformation($"   NumberOfScriptsPerUser: {settings.NumberOfScriptsPerUser}");
         _logger.LogInformation($"   NoOlderThanDays: {settings.NoOlderThanDays}");

         // Create Retention Policy base on settings
         var lastNumberOf = settings.NumberOfScriptsPerUser > 0 ? settings.NumberOfScriptsPerUser : DEFAULT_NUMBER_OF_SCRIPTS_PER_USER;
         var noOlderThanDays = settings.NoOlderThanDays > 0 ? settings.NoOlderThanDays : DEFAULT_NO_OLDER_THAN_DAYS;
         _retentionPolicy = new FolderScriptExecutionsRetentionPolicy(
            loggerFactory,
            _fileSystem,
            _rootFolder,
            new IScriptsRetentionRule[] {
               new LastNumberOfRecordsRetentionRule(lastNumberOf),
               new OlderThanRetentionRule(new TimeSpan(noOlderThanDays, 0, 0 ,0))
            });
         StartRetentionPolicyApplier();
      }

      private void StartRetentionPolicyApplier() {
         if (_retentionPolicyApplierTimer == null) {
            _retentionPolicyApplierTimer = new Timer(ApplyRetentionPolicy, null, 0, 1000 * 60 * 5 /*5 minutes*/);
         }
      }

      private void StopRetentionPolicyApplier() {
         if (_retentionPolicyApplierTimer != null) {
            _retentionPolicyApplierTimer.Dispose();
            _retentionPolicyApplierTimer = null;
         }
      }

      private void ApplyRetentionPolicy(Object stateInfo) {
         try {
            lock (_retentionPolicyUpdateLock) {
               _retentionPolicy.Apply();
            }            
         } catch (Exception exc) {
            _logger.Log(LogLevel.Error, exc.ToString());
         }
      }

      private void AddScriptStorageControllers(
         string scriptId,
         IScriptExecutionStoreProvider fileStoreProvider,
         IPollingScriptExecutionPersister pollingScriptExecutionPersister) {

         lock (this) {
            _scriptIdToFileStoreProvider[scriptId] = fileStoreProvider;
            _scriptIdToPollingPersister[scriptId] = pollingScriptExecutionPersister;
         }
      }

      private void RemoveScriptStorageControllers(string scriptId) {
         lock (this) {
            _scriptIdToFileStoreProvider.Remove(scriptId, out _);
            _scriptIdToPollingPersister.Remove(scriptId, out _);
         }
      }

      private string[] ListScriptIds(string userId) {
         var result = new string[]{};

         try {
            var directory = _fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(_rootFolder, userId));
            result = directory.GetDirectories().Select(di => di.Name).ToArray();
         } catch (Exception exc) {
            _logger.Log(LogLevel.Error, exc.ToString());
         }

         return result;
      }

      private IScriptExecutionReader GetScriptReader(string userId, string scriptId) {
         IScriptExecutionReader result = null;

         lock (this) {
            if (_scriptIdToFileStoreProvider.TryGetValue(scriptId, out var fileStoreProvider)) {
               result = fileStoreProvider;
            }
         }

         return result ?? new ScriptExecutionFileReader(_logger, _rootFolder, userId, scriptId, _fileSystem);
      }

      public event EventHandler<ScriptResultStoredEventArgs> ScriptResultStored;
      
      public INamedScriptExecution GetScriptExecution(string userId, string scriptId) {
         var reader = GetScriptReader(userId, scriptId);
         return reader.ReadScriptExecution();
      }

      public ScriptExecutionDataStreams GetScriptExecutionDataStreams(string userId, string scriptId) {
         var reader = GetScriptReader(userId, scriptId);
         return reader.ReadScriptExecutionDataStreams();
      }

      public IScriptExecutionOutputObjects GetScriptExecutionOutput(string userId, string scriptId) {
         var reader = GetScriptReader(userId, scriptId);
         return reader.ReadScriptExecutionOutput();
      }

      public INamedScriptExecution[] ListScriptExecutions(string userId) {
         var userScripts = ListScriptIds(userId);

         return userScripts.Select(scriptId => GetScriptExecution(userId, scriptId)).
            Where(se => !string.IsNullOrEmpty(se?.Id)).
            ToArray();
      }
      
      public void StartStoringScriptExecution(
         string userId, 
         IRunspace runspaceClient, 
         string scriptId, 
         string scriptName) {

         var fileStoreProvider = _scriptExecutionWriterFactory.Create(_logger, _rootFolder, userId, scriptId, _fileSystem);
         var pollingScriptExecutionPersister = _scriptExecutionPersisterFactory.Create(_logger);

         pollingScriptExecutionPersister.ScriptResultPersisted += PollingScriptExecutionPersister_ScriptResultPersisted;

         AddScriptStorageControllers(scriptId, fileStoreProvider, pollingScriptExecutionPersister);

         pollingScriptExecutionPersister.Start(runspaceClient, scriptId, scriptName, fileStoreProvider);
      }

      private void PollingScriptExecutionPersister_ScriptResultPersisted(object sender, ScriptResultStoredEventArgs e) {
         RemoveScriptStorageControllers(e.ScriptId);
         ScriptResultStored?.Invoke(this, e);
      }

      public void Dispose() {
         StopRetentionPolicyApplier();
      }

      public void UpdateConfiguration(ScriptExecutionStorageSettings settings) {
         if (settings != null &&
            (_lastSttingsConfiguration.NoOlderThanDays != settings.NoOlderThanDays ||
            _lastSttingsConfiguration.NumberOfScriptsPerUser != settings.NumberOfScriptsPerUser ||
            _lastSttingsConfiguration.ServiceScriptStorageDir != settings.ServiceScriptStorageDir)) {

            _logger.LogInformation("Updating Script Execution Storage Settings");
            _logger.LogInformation($"   StorageDir: {settings.ServiceScriptStorageDir}");
            _logger.LogInformation($"   NumberOfScriptsPerUser: {settings.NumberOfScriptsPerUser}");
            _logger.LogInformation($"   NoOlderThanDays: {settings.NoOlderThanDays}");

            // Update Settings
            _rootFolder = settings.ServiceScriptStorageDir;
            var lastNumberOf = settings.NumberOfScriptsPerUser > 0 ? settings.NumberOfScriptsPerUser : DEFAULT_NUMBER_OF_SCRIPTS_PER_USER;
            var noOlderThanDays = settings.NoOlderThanDays > 0 ? settings.NoOlderThanDays : DEFAULT_NO_OLDER_THAN_DAYS;
            _lastSttingsConfiguration = settings;

            // Update Retention policy
            lock (_retentionPolicyUpdateLock) {
               _retentionPolicy = new FolderScriptExecutionsRetentionPolicy(
                 _loggerFactory,
                 _fileSystem,
                 _rootFolder,
                 new IScriptsRetentionRule[] {
                     new LastNumberOfRecordsRetentionRule(lastNumberOf),
                     new OlderThanRetentionRule(new TimeSpan(noOlderThanDays, 0, 0 ,0))
                 });
            }
         }
      }
   }
}
