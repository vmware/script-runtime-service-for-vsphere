// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   public class CleanupVCRegistrationSetupFlow : BaseSetupFlow, ISetupFlow {
      protected override SetupFlowType Type => SetupFlowType.CleanupVCRegistration;
      public CleanupVCRegistrationSetupFlow(ILoggerFactory loggerFactory) : base(loggerFactory) {
         _logger = loggerFactory.CreateLogger(typeof(CleanupVCRegistrationSetupFlow));
      }

      protected override void RunInternal(VCRegistration.VCRegistrator vcRegistrator, UserInput userInput) {
         vcRegistrator.Clean(
            userInput.Psc,
            userInput.User,
            userInput.Password,
            userInput.VcThumbprint,
            userInput.ForceSpecified);
      }
   }
}
