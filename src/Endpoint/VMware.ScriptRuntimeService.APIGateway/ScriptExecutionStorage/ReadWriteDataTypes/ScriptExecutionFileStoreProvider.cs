// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes {
   /// <summary>
   /// Keeps script execution results in memory until Flush is called.
   /// When Flush is called a folder userId/scriptId is created and the results
   /// are saved as files in this dedicated script folder.
   ///
   /// Allows multiple readers to access the in-memory preserved script execution data.
   /// </summary>
   public class ScriptExecutionFileStoreProvider : IScriptExecutionStoreProvider {
      private readonly string _scriptFolder;
      private readonly ILogger _logger;
      private readonly IFileSystem _fileSystem;

      private readonly ReaderWriterLockSlim _scriptExecutionDataLock = new ReaderWriterLockSlim();
      private INamedScriptExecution _scriptExecution;

      private readonly ReaderWriterLockSlim _scriptExecutionOutputDataLock = new ReaderWriterLockSlim();
      private IScriptExecutionOutputObjects _scriptExecutionOutput;

      private readonly ReaderWriterLockSlim _scriptExecutionStreamsDataLock = new ReaderWriterLockSlim();
      private ScriptExecutionDataStreams _scriptExecutionStreams;

      public ScriptExecutionFileStoreProvider(
         ILogger logger,
         string rootFolder,
         string userId,
         string scriptId) : 
         this(logger, rootFolder, userId, scriptId, new FileSystem()) {
      }
      public ScriptExecutionFileStoreProvider (
         ILogger logger,
         string rootFolder,
         string userId,
         string scriptId,
         IFileSystem fileSystem) {
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
         _scriptFolder = Path.Combine(rootFolder, userId, scriptId);
         _fileSystem = fileSystem;

         // Creates directory for the user script
         _fileSystem.Directory.CreateDirectory(_scriptFolder);
      }

      public INamedScriptExecution ReadScriptExecution() {
         _scriptExecutionDataLock.EnterReadLock();
         try {
            return _scriptExecution;
         } finally {
            _scriptExecutionDataLock.ExitReadLock();
         }
      }

      public IScriptExecutionOutputObjects ReadScriptExecutionOutput() {
         _scriptExecutionOutputDataLock.EnterReadLock();
         try {
            return _scriptExecutionOutput;
         } finally {
            _scriptExecutionOutputDataLock.ExitReadLock();
         }
      }

      public ScriptExecutionDataStreams ReadScriptExecutionDataStreams() {
         _scriptExecutionStreamsDataLock.EnterReadLock();
         try {
            return _scriptExecutionStreams;
         } finally {
            _scriptExecutionStreamsDataLock.ExitReadLock();
         }
      }

      public void WriteScriptExecution(INamedScriptExecution scriptExecution) {
         _scriptExecutionDataLock.EnterWriteLock();
         try {
            _scriptExecution = scriptExecution;
         } finally {
            _scriptExecutionDataLock.ExitWriteLock();
         }
      }

      public void WriteScriptExecutionOutput(IScriptExecutionOutputObjects scriptExecutionOutput) {
         _scriptExecutionOutputDataLock.EnterWriteLock();
         try {
            _scriptExecutionOutput = scriptExecutionOutput;
         } finally {
            _scriptExecutionOutputDataLock.ExitWriteLock();
         }
      }

      public void WriteScriptExecutionDataStreams(ScriptExecutionDataStreams scriptExecutionDataStreams) {
         _scriptExecutionStreamsDataLock.EnterWriteLock();
         try {
            _scriptExecutionStreams = scriptExecutionDataStreams;
         } finally {
            _scriptExecutionStreamsDataLock.ExitWriteLock();
         }
      }

      public void Flush() {
         try {
            // Creates directory for the user script unless it doesn't exist
            _fileSystem.Directory.CreateDirectory(_scriptFolder);

            // Get JSON content for script execution data
            var scriptExecutionJson = JsonConvert.SerializeObject((NamedScriptExecution)ReadScriptExecution());
            var scriptExecutionOutputJson = JsonConvert.SerializeObject((ScriptExecutionOutput)ReadScriptExecutionOutput());
            var scriptExecutionDataStreamsJson = JsonConvert.SerializeObject(ReadScriptExecutionDataStreams());

            // Save JSON content to files
            _fileSystem.File.WriteAllText(Path.Combine(_scriptFolder, ScriptExecutionFileNames.ScriptExecution), scriptExecutionJson);
            _fileSystem.File.WriteAllText(Path.Combine(_scriptFolder, ScriptExecutionFileNames.ScriptExecutionOutput), scriptExecutionOutputJson);
            _fileSystem.File.WriteAllText(Path.Combine(_scriptFolder, ScriptExecutionFileNames.ScriptExecutionStreams), scriptExecutionDataStreamsJson);
         } catch (Exception exc) {
            _logger.Log(LogLevel.Error, exc.ToString());
         }
      }
   }
}
