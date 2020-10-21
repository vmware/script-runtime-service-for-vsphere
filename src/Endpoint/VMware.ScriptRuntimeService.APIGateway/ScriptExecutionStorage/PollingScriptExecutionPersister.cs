// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage {
   internal class PollingScriptExecutionPersister : IPollingScriptExecutionPersister {
      private ILogger _logger;
      public PollingScriptExecutionPersister(ILogger logger) {
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      }

      public event EventHandler<ScriptResultStoredEventArgs> ScriptResultPersisted;

      public void Start(IRunspace runspaceClient, string scriptId, string scriptName, IScriptExecutionStoreProvider scriptExecutionWriter) {
         int maxGetLastScriptFailures = 3;
         int lastScriptFailures = 0;
         Task.Run(() => {
            IScriptExecutionResult scriptExecutionResult = null;
            do {
               try {
                  scriptExecutionResult = runspaceClient.GetScript(scriptId);
               } catch (Exception exc) {
                  lastScriptFailures++;
                  _logger.Log(LogLevel.Error, exc.ToString());
               }
               
               scriptExecutionWriter.WriteScriptExecution(new NamedScriptExecution(scriptName, scriptExecutionResult));
               scriptExecutionWriter.WriteScriptExecutionOutput(new ScriptExecutionOutput(scriptExecutionResult));
               scriptExecutionWriter.WriteScriptExecutionDataStreams(new ScriptExecutionDataStreams(scriptExecutionResult?.Streams));
               
               Thread.Sleep(500);
            } while (lastScriptFailures < maxGetLastScriptFailures && scriptExecutionResult.State == ScriptState.Running);

            if (lastScriptFailures >= maxGetLastScriptFailures) {
               // Retrieval of last script failed which mean script has been lost because 
               // bad communication with the runspace
               // 1. Read the last persisted result
               // 2. Update Script ExecutionState to Error
               // 3. Write script state
               var lastPersistedScript = scriptExecutionWriter.ReadScriptExecution();
               var updatedScriptExecution = new NamedScriptExecution {
                  Name = lastPersistedScript?.Name,
                  Id = lastPersistedScript?.Id,
                  StarTime = lastPersistedScript?.StarTime,
                  EndTime = DateTime.Now,
                  OutputObjectsFormat = lastPersistedScript?.OutputObjectsFormat ?? OutputObjectsFormat.Text,
                  State = ScriptState.Error,
                  Reason = APIGatewayResources.PollingScriptExecutionPersister_ScriptFailed_RunspaceDisappeared
               };
               scriptExecutionWriter.WriteScriptExecution(updatedScriptExecution);
            }

            scriptExecutionWriter.Flush();

            ScriptResultPersisted?.Invoke(this, new ScriptResultStoredEventArgs { ScriptId = scriptId });
         });
      }
   }
}
