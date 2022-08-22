// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.Setup.SetupFlows
{
   public class RegisterWithVCSetupFlow : BaseSetupFlow, ISetupFlow {
      protected override SetupFlowType Type => SetupFlowType.RegisterWithVC;
      public RegisterWithVCSetupFlow(ILoggerFactory loggerFactory) : base(loggerFactory) {
         _logger = loggerFactory.CreateLogger(typeof(RegisterWithVCSetupFlow));
      }

      protected override void RunInternal(VCRegistration.VCRegistrator vcRegistrator, UserInput userInput) {
         vcRegistrator.Register(
            userInput.ServiceHostname,
            userInput.SigningCertificatePath,
            userInput.TlsCertificatePath,
            userInput.Psc,
            userInput.User,
            userInput.Password,
            userInput.VcThumbprint,
            userInput.ForceSpecified);
      }
   }
}
