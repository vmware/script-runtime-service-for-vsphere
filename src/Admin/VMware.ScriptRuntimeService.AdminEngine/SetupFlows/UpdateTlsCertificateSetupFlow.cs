// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   /// <summary>
   /// Setup flow that updates Tls certificates
   /// </summary>
   public class UpdateTlsCertificateSetupFlow : BaseSetupFlow, ISetupFlow {
      protected override SetupFlowType Type => SetupFlowType.UpdateTlsCertificate;
      public UpdateTlsCertificateSetupFlow(ILoggerFactory loggerFactory) : base(loggerFactory) {
         _logger = loggerFactory.CreateLogger(typeof(UpdateTlsCertificateSetupFlow));
      }

      protected override void RunInternal(VCRegistration.VCRegistrator vcRegistrator, UserInput userInput) {
         vcRegistrator.UpdateTlsCertificate(
            userInput.Psc,
            userInput.User,
            userInput.Password,
            userInput.VcThumbprint,
            userInput.ForceSpecified,
            userInput.StsSettingsPath,
            userInput.TlsCertificatePath);
      }
   }
}
