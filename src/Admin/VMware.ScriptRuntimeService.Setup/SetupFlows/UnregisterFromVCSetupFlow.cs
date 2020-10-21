// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using k8s.KubeConfigModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IO;
using System.Text;
using VMware.ScriptRuntimeService.Ls;
using VMware.ScriptRuntimeService.Setup.K8sClient;
using VMware.ScriptRuntimeService.Setup.TlsTrustValidators;
using VMware.ScriptRuntimeService.SsoAdmin;

namespace VMware.ScriptRuntimeService.Setup.SetupFlows
{
   public class UnregisterFromVCSetupFlow : ISetupFlow
   {
      private ILoggerFactory _loggerFactory;
      private ILogger _logger;

      public UnregisterFromVCSetupFlow(ILoggerFactory loggerFactory) {
         if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

         _loggerFactory = loggerFactory;
         _logger = loggerFactory.CreateLogger(typeof(UnregisterFromVCSetupFlow));
      }

      public int Run(UserInput userInput) {
         try {
            var certificatesCommonName = userInput.ServiceHostname;

            _logger.LogDebug($"User Input VC: {userInput.Psc}");
            _logger.LogDebug($"User Input VC User: {userInput.User}");
            _logger.LogDebug($"User Input VC Thumbprint: {userInput.VcThumbprint}");
            _logger.LogDebug($"User Input Force Specified: {userInput.ForceSpecified}");
            _logger.LogDebug($"User Input Sts Settings Path Specified: {userInput.StsSettingsPath}");

            userInput.EnsureIsValid(SetupFlowType.UnregisterFromVC);
                        
            var stsSettings = new SettingsEditor(File.ReadAllText(userInput.StsSettingsPath)).
               GetStsSettings();
            _logger.LogDebug($"Sts Settings SolutionServiceId: {stsSettings.SolutionServiceId}");
            _logger.LogDebug($"Sts Settings SolutionOwnerId: {stsSettings.SolutionOwnerId}");            

            K8sSettings k8sSettings = null;
            if (userInput.K8sSettings != null && File.Exists(userInput.K8sSettings)) {
               k8sSettings = JsonConvert.DeserializeObject<K8sSettings>(File.ReadAllText(userInput.K8sSettings));
            }

            // === VC Unregister Actions ===
            X509CertificateValidator certificateValidator = null;
            if (userInput.ForceSpecified) {
               certificateValidator = new AcceptAllX509CertificateValidator();
            } else if (!string.IsNullOrEmpty(userInput.VcThumbprint)) {
               certificateValidator = new SpecifiedCertificateThumbprintValidator(userInput.VcThumbprint);
            }

            _logger.LogDebug($"Service Id: {stsSettings.SolutionServiceId}");
            _logger.LogDebug($"Service OwnerId: {stsSettings.SolutionOwnerId}");

            var setupServiceSettings = SetupServiceSettings.FromStsSettings(stsSettings);

            _logger.LogDebug($"SetupServiceSettings ServiceId: {setupServiceSettings.ServiceId}");
            _logger.LogDebug($"SetupServiceSettings OwnerId: {setupServiceSettings.OwnerId}");

            var lookupServiceClient = new LookupServiceClient(
               userInput.Psc,
               certificateValidator);

            var ssoSdkUri = lookupServiceClient.GetSsoAdminEndpointUri();
            var stsUri = lookupServiceClient.GetStsEndpointUri();
            _logger.LogDebug($"Resolved SSO SDK Endpoint: {ssoSdkUri}");
            _logger.LogDebug($"Resolved Sts Endpoint: {stsUri}");

            var ssoAdminClient = new SsoAdminClient(ssoSdkUri, stsUri, certificateValidator);

            // --- SSO Solution User Registration ---
            var ssoSolutionRegitration = new SsoSolutionUserRegistration(
               _loggerFactory,
               setupServiceSettings,
               ssoAdminClient);

            ssoSolutionRegitration.DeleteSolutionUser(userInput.User, userInput.Password);
            // --- SSO Solution User Registration ---

            // --- Lookup Service Registration ---
            var lsRegistration = new LookupServiceRegistration(
               _loggerFactory,
               setupServiceSettings,
               lookupServiceClient);
            lsRegistration.Deregister(userInput.User, userInput.Password);
            // --- Lookup Service Registration ---
            // === VC Unregister Actions ===
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
