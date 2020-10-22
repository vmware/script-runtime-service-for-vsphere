// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage {
   /// <summary>
   /// Polls script execution progress in worker thread and persists 
   /// script execution data.
   /// </summary>
   public interface IPollingScriptExecutionPersister {
      /// <summary>
      /// Starts script state polling and persists script execution data through <see cref="IScriptExecutionWriter"/>
      /// </summary>
      /// <param name="runspaceClient">Client for communication with the runspace where script runs</param>
      /// <param name="scriptId">Id of the script to poll for</param>
      /// <param name="scriptName">Name of the script to poll for</param>
      /// <param name="scriptExecutionWriter">Instance of <see cref="IScriptExecutionWriter" /> used to store Script Execution Data</param>
      void Start(IRunspace runspaceClient, string scriptId, string scriptName, IScriptExecutionStoreProvider scriptExecutionWriter);

      /// <summary>
      /// Raised once script has completed and it's result is persisted
      /// </summary>
      event EventHandler<ScriptResultStoredEventArgs> ScriptResultPersisted;
   }
}
