// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Collections.Generic;
using System.Resources;
using VMware.ScriptRuntimeService.RunspaceEndpoint.Resources;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes {
   public static class ApiErrorCodes {
      private static Dictionary<string, int> ErrorMessageToErrorCode = new Dictionary<string, int> {
         {nameof(RunspaceEndpointResources.AnotherScriptIsRunning), 310},
         {nameof(RunspaceEndpointResources.ScriptNotScheduled), 311},
      };

      public static int GetErrorCode(string errorRsourceKey) {
         if (!ErrorMessageToErrorCode.TryGetValue(errorRsourceKey, out var result)) {
            result = Unknown;
         }
         return result;
      }

      public static int Unknown => 500;
   }
}
