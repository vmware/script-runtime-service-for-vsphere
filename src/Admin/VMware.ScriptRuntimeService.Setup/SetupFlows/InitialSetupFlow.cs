// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.Setup.SetupFlows {
   public class InitialSetupFlow : BaseSetupFlow, ISetupFlow {

      protected override SetupFlowType Type => SetupFlowType.InitialSetup;

      public InitialSetupFlow(ILoggerFactory loggerFactory) : base(loggerFactory) {
         _logger = loggerFactory.CreateLogger(typeof(InitialSetupFlow));
      }

      protected override void RunInternal(VCRegistration.VCRegistrator vcRegistrator, UserInput userInput) {
         vcRegistrator.SetupCertificates(
            userInput.ServiceHostname,
            userInput.SigningCertificatePath,
            userInput.TlsCertificatePath);
      }
   }
}
