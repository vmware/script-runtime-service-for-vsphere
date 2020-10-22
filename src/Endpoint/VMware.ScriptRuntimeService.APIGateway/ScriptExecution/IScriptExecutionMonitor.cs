// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecution {
   /// <summary>
   /// Starts async script execution on a given runspace endpoint and
   /// monitors script execution. When script execution finish
   /// keeps the result of the execution.
   /// </summary>
   public interface IScriptExecutionMonitor {
      /// <summary>
      /// Starts script execution sending start request to the runspace endpoint
      /// </summary>
      /// <param name="runspaceInfo">Runspace endpoint</param>
      /// <param name="startScriptAction">Function that accepts IRunspace instance where the script will be started and returns script id</param>
      /// <returns></returns>
      Task StartScript(IRunspaceInfo runspaceInfo, Func<IRunspace, Task<IScriptExecutionResult>> startScriptAction);

      /// <summary>
      /// Synchronously cancels script
      /// </summary>
      void CancelScript();

      /// <summary>
      /// Gets the script status and result
      /// </summary>
      /// <returns><see cref="IScriptExecutionResult"/> instance</returns>
      INamedScriptExecutionResult GetScriptExecutionResult();

      /// <summary>
      /// Script Id
      /// </summary>
      string ScriptId { get; }
   }
}
