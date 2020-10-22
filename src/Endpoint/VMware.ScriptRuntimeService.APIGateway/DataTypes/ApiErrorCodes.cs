// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Properties;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   public static class ApiErrorCodes {
      private const int SCRIPTS_ERROR_CODES = 1000;
      private static Dictionary<string, int> ErrorMessageToErrorCode = new Dictionary<string, int> {
         {nameof(APIGatewayResources.ScriptsController_RunspaceFailedToProcessScriptRequest), 1025},
         {nameof(APIGatewayResources.ScriptsController_RunspaceFailedToCancelScriptExecution), 1027},
         {nameof(APIGatewayResources.ScriptsController_ScriptNotFound), 1040},
         {nameof(APIGatewayResources.ScriptsController_ScriptStorageService_FailedToRetrieveScripts), 1045},
         {nameof(APIGatewayResources.ScriptOutputController_ScriptStorageService_FailedToRetrieveScriptOutput), 1146},
         {nameof(APIGatewayResources.ScriptStreamsController_ScriptStorageService_FailedToRetrieveScriptStreams), 1147},
         {nameof(APIGatewayResources.RunspaceController_Post_MaxnumberOfRunspacesReached), 2020},
         {nameof(APIGatewayResources.RunspaceNotFound), 2030},
         {nameof(APIGatewayResources.RunspaceNotReady), 2031},
         {nameof(APIGatewayResources.RunspaceController_List_RunspaceProviderListFailed), 2035},
         {nameof(APIGatewayResources.RunspaceController_Kill_RunspaceProviderKillFailed), 2040},
         {nameof(APIGatewayResources.ArgumentScriptsController_ArgumentTransformationScriptNotFound), 4010},
         {nameof(APIGatewayResources.SessionsController_SessionsService_FailedToDeleteSession), 5010},
         {nameof(APIGatewayResources.PowerCLIVCLoginController_Post_AcquireAuthorizationTokenFailed), 2050},
      };

      public static int GetErrorCode(string errorRsourceMessage) {
         if (!ErrorMessageToErrorCode.TryGetValue(errorRsourceMessage, out var result)) {
            result = Unknown;
         }
         return result;
      }

      public static int CalculateScriptsErrorCode(int internalErrorCode) {
         return SCRIPTS_ERROR_CODES + internalErrorCode;
      }

      public static int Unknown => 500;
   }
}
