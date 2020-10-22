// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// ScriptExecutionState defines possible script execution states.
   /// </summary>
   [JsonConverter(typeof(StringEnumConverter))]
   [DataContract(Name="script_execution_state")]
   [ReadOnly(true)]
   public enum ScriptExecutionState {
      /// <summary>
      /// Script execution successfully completed.
      /// </summary>
      [EnumMember(Value = "success")]
      Success,

      /// <summary>
      /// Script execution was terminated by error.
      /// </summary>
      [EnumMember(Value = "error")]
      Error,

      /// <summary>
      /// Script execution is in progress.
      /// </summary>
      [EnumMember(Value = "running")]
      Running,

      /// <summary>
      /// Script execution was canceled.
      /// </summary>
      [EnumMember(Value = "canceled")]
      Canceled
   }

   internal static class ScriptStateConvert {
      public static ScriptExecutionState From(VMware.ScriptRuntimeService.Runspace.Types.ScriptState state) {
         switch (state) {
            case VMware.ScriptRuntimeService.Runspace.Types.ScriptState.Canceled:
               return ScriptExecutionState.Canceled;
            case VMware.ScriptRuntimeService.Runspace.Types.ScriptState.Running:
               return ScriptExecutionState.Running;
            case VMware.ScriptRuntimeService.Runspace.Types.ScriptState.Success:
               return ScriptExecutionState.Success;
            case VMware.ScriptRuntimeService.Runspace.Types.ScriptState.Error:
               return ScriptExecutionState.Error;
            default:
               return ScriptExecutionState.Running;
         }
      }
   }
}
