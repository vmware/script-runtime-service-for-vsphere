// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   /// <summary>
   /// Setup flow that de-registers SRS from a VC
   /// </summary>
   public class UnregisterFromVCSetupFlow : BaseSetupFlow, ISetupFlow {
      protected override SetupFlowType Type => SetupFlowType.UnregisterFromVC;

      public UnregisterFromVCSetupFlow(ILoggerFactory loggerFactory) : base(loggerFactory) {
         _logger = loggerFactory.CreateLogger(typeof(UnregisterFromVCSetupFlow));
      }

      protected override void RunInternal(VCRegistration.VCRegistrator vcRegistrator, UserInput userInput) {
         vcRegistrator.Unregister(
            userInput.Psc,
            userInput.User,
            userInput.Password,
            userInput.VcThumbprint,
            userInput.ForceSpecified,
            userInput.StsSettingsPath);
      }
   }
}
