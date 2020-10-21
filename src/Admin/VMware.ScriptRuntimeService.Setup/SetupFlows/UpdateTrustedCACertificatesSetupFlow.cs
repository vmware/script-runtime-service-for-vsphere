// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IO;
using System.Text;
using VMware.ScriptRuntimeService.Ls;
using VMware.ScriptRuntimeService.Setup.ConfigFileWriters;
using VMware.ScriptRuntimeService.Setup.K8sClient;
using VMware.ScriptRuntimeService.Setup.TlsTrustValidators;
using VMware.ScriptRuntimeService.SsoAdmin;

namespace VMware.ScriptRuntimeService.Setup.SetupFlows
{
   public class UpdateTrustedCACertificatesSetupFlow : ISetupFlow
   {
      private ILoggerFactory _loggerFactory;
      private ILogger _logger;
      public UpdateTrustedCACertificatesSetupFlow(ILoggerFactory loggerFactory) {
         if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

         _loggerFactory = loggerFactory;
         _logger = loggerFactory.CreateLogger(typeof(UpdateTrustedCACertificatesSetupFlow));
      }

      public int Run(UserInput userInput) {
         try {
            var certificatesCommonName = userInput.ServiceHostname;

            _logger.LogDebug($"User Input VC: {userInput.Psc}");
            _logger.LogDebug($"User Input VC User: {userInput.User}");
            _logger.LogDebug($"User Input VC Thumbprint: {userInput.VcThumbprint}");
            _logger.LogDebug($"User Input Force Specified: {userInput.ForceSpecified}");

            userInput.EnsureIsValid(SetupFlowType.UpdateTrustedCACertificates);

            // Create Config File Writer
            K8sSettings k8sSettings = null;
            if (userInput.K8sSettings != null && File.Exists(userInput.K8sSettings)) {
               k8sSettings = JsonConvert.DeserializeObject<K8sSettings>(File.ReadAllText(userInput.K8sSettings));
            }

            var configWriter = new K8sConfigWriter(_loggerFactory, k8sSettings);

            // Create Lookup Service Client
            X509CertificateValidator certificateValidator = null;
            if (userInput.ForceSpecified) {
               certificateValidator = new AcceptAllX509CertificateValidator();
            } else if (!string.IsNullOrEmpty(userInput.VcThumbprint)) {
               certificateValidator = new SpecifiedCertificateThumbprintValidator(userInput.VcThumbprint);
            }

            var lookupServiceClient = new LookupServiceClient(
               userInput.Psc,
               certificateValidator);

            // Get SSO Admin And STS URIs from Lookup Service
            var ssoSdkUri = lookupServiceClient.GetSsoAdminEndpointUri();
            var stsUri = lookupServiceClient.GetStsEndpointUri();
            _logger.LogDebug($"Resolved SSO SDK Endpoint: {ssoSdkUri}");
            _logger.LogDebug($"Resolved Sts Endpoint: {stsUri}");

            //
            var ssoAdminClient = new SsoAdminClient(ssoSdkUri, stsUri, certificateValidator);

            // --- Store VC CA certificates ---
            var trustedCertificatesStore = new TrustedCertificatesStore(
               _loggerFactory,               
               ssoAdminClient,
               configWriter);
            trustedCertificatesStore.SaveVcenterCACertficates();
            // --- Store VC CA certificates ---

         } catch (InvalidUserInputException exc) {
            _logger.LogError(exc, exc.Message);
            return 1;
         } catch (Exception exc) {
            _logger.LogError(exc, exc.Message);
            return 2;
         }

         _logger.LogInformation("Success");
         return 0;
      }
   }
}
