// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   /// <summary>
   /// Setup flow that updates trusted CA certificates
   /// </summary>
   public class UpdateTrustedCACertificatesSetupFlow : BaseSetupFlow, ISetupFlow {
      protected override SetupFlowType Type => SetupFlowType.UpdateTrustedCACertificates;

      public UpdateTrustedCACertificatesSetupFlow(ILoggerFactory loggerFactory) : base(loggerFactory) {
         _logger = loggerFactory.CreateLogger(typeof(UpdateTrustedCACertificatesSetupFlow));
      }
      protected override void RunInternal(VCRegistrator vcRegistrator, UserInput userInput) {
         vcRegistrator.UpdateTrustedCACertificates(
            userInput.Psc,
            userInput.User,
            userInput.Password,
            userInput.VcThumbprint,
            userInput.ForceSpecified);
      }
   }
}
