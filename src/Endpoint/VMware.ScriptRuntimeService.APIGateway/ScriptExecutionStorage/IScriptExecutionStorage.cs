// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage {
   interface IScriptExecutionStorage {
      void StartStoringScriptExecution(
         string userId,
         IRunspace runspaceClient,
         string scriptId,
         string scriptName);

      INamedScriptExecution GetScriptExecution(string userId, string scriptId);

      INamedScriptExecution[] ListScriptExecutions(string userId);

      IScriptExecutionOutputObjects GetScriptExecutionOutput(string userId, string scriptId);

      ScriptExecutionDataStreams GetScriptExecutionDataStreams(string userId, string scriptId);

      /// <summary>
      /// Updates storage settings
      /// </summary>
      void UpdateConfiguration(ScriptExecutionStorageSettings settings);
      
      /// <summary>
      /// Raised once script has completed and it's result is preserved
      /// </summary>
      event EventHandler<ScriptResultStoredEventArgs> ScriptResultStored;
   }
}
