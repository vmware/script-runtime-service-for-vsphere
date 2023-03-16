// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl {
   public class PersistentScriptExecutionMediator : IScriptExecutionMediator, IDisposable {
      private IScriptExecutionStorage _scriptExecutionStorage;
      private readonly IRunspaceClientFactory _runspaceClientFactory;

      private readonly ConcurrentDictionary<string, RunspaceClient.RunspaceClient> _scriptIdToRunspaceClient =
         new ConcurrentDictionary<string, RunspaceClient.RunspaceClient>();

      public PersistentScriptExecutionMediator(ILoggerFactory loggerFactory, ScriptExecutionStorageSettings storageSettings) {
         _scriptExecutionStorage = new ScriptExecutionFileStorage(loggerFactory, storageSettings);
         _scriptExecutionStorage.ScriptResultStored += (sender, args) => {
            _scriptIdToRunspaceClient.TryRemove(args.ScriptId, out _);
         };
         _runspaceClientFactory = new RunspaceClientFactory();
      }

      public Task<INamedScriptExecution> StartScriptExecution(
         string userId, 
         IRunspaceInfo runspace, 
         IScriptExecutionRequest scriptExecutionRequest) {

         return StartScriptExecution(userId, runspace, scriptExecutionRequest, false);
      }
      
      public async Task<INamedScriptExecution> StartScriptExecution(
         string userId, 
         IRunspaceInfo runspace, 
         IScriptExecutionRequest scriptExecutionRequest,
         bool isSystemExecution) {

         var runspaceClient = _runspaceClientFactory.Create(runspace.Endpoint) as RunspaceClient.RunspaceClient;
         var scriptExecResult = await runspaceClient.StartScript(
            scriptExecutionRequest.Script, 
            scriptExecutionRequest.OutputObjectsFormat,
            scriptExecutionRequest.Parameters);
         _scriptIdToRunspaceClient[scriptExecResult.Id] = runspaceClient;

         _scriptExecutionStorage.StartStoringScriptExecution(
            userId, 
            runspaceClient, 
            scriptExecResult.Id, 
            scriptExecutionRequest.Name,
            isSystemExecution);

         return new ScriptResult(scriptExecutionRequest.Name, scriptExecResult);
      }

      public INamedScriptExecution[] ListScriptExecutions(string userId, bool skipSystemExecutions) {
         return _scriptExecutionStorage.ListScriptExecutions(userId, skipSystemExecutions);
      }

      public INamedScriptExecution GetScriptExecution(string userId, string scriptId) {
         return _scriptExecutionStorage.GetScriptExecution(userId, scriptId);
      }

      public IScriptExecutionOutputObjects GetScriptExecutionOutput(string userId, string scriptId) {
         return _scriptExecutionStorage.GetScriptExecutionOutput(userId, scriptId);
      }

      public IScriptExecutionDataStreams GetScriptExecutionDataStreams(string userId, string scriptId) {
         return _scriptExecutionStorage.GetScriptExecutionDataStreams(userId, scriptId);
      }

      public void CancelScriptExecution(string userId, string scriptId) {
         if (_scriptIdToRunspaceClient.TryGetValue(scriptId, out var runspaceClient)) {
            runspaceClient.CancelScript(scriptId);
         }
      }

      public void UpdateConfiguration(ScriptExecutionStorageSettings settings) {
         _scriptExecutionStorage.UpdateConfiguration(settings);
      }

      public void Dispose() {
         var disposableStorage = _scriptExecutionStorage as IDisposable;
         if (disposableStorage != null) {
            disposableStorage.Dispose();
            _scriptExecutionStorage = null;
         }
      }      
   }
}
