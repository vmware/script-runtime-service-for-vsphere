// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   /// <summary>
   /// Factory class to create specific setup flows
   /// </summary>
   public static class SetupFlowFactory {
      public static ISetupFlow Create(ILoggerFactory loggerFactory, SetupFlowType setupFlowType) {
         ISetupFlow result = null;
         switch (setupFlowType) {
            default:
            case SetupFlowType.InitialSetup:
               result = new InitialSetupFlow(loggerFactory);
               break;
            case SetupFlowType.RegisterWithVC:
               result = new RegisterWithVCSetupFlow(loggerFactory);
               break;
            case SetupFlowType.UnregisterFromVC:
               result = new UnregisterFromVCSetupFlow(loggerFactory);
               break;
            case SetupFlowType.UpdateTlsCertificate:
               result = new UpdateTlsCertificateSetupFlow(loggerFactory);
               break;
            case SetupFlowType.UpdateTrustedCACertificates:
               result = new UpdateTrustedCACertificatesSetupFlow(loggerFactory);
               break;
            case SetupFlowType.CleanupVCRegistration:
               result = new CleanupVCRegistrationSetupFlow(loggerFactory);
               break;
         }
         return result;
      }
   }
}
