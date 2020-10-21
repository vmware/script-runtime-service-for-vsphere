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
using VMware.ScriptRuntimeService.Setup.K8sClient;
using VMware.ScriptRuntimeService.Setup.TlsTrustValidators;
using System.Security.Cryptography.X509Certificates;

namespace VMware.ScriptRuntimeService.Setup.SetupFlows
{
   public class UpdateTlsCertificateSetupFlow : ISetupFlow
   {
      private ILoggerFactory _loggerFactory;
      private ILogger _logger;
      public UpdateTlsCertificateSetupFlow(ILoggerFactory loggerFactory) {
         if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

         _loggerFactory = loggerFactory;
         _logger = loggerFactory.CreateLogger(typeof(UpdateTlsCertificateSetupFlow));
      }

      public int Run(UserInput userInput) {
         try {
            var certificatesCommonName = userInput.ServiceHostname;

            _logger.LogDebug($"User Input VC: {userInput.Psc}");
            _logger.LogDebug($"User Input VC User: {userInput.User}");
            _logger.LogDebug($"User Input VC Thumbprint: {userInput.VcThumbprint}");
            _logger.LogDebug($"User Input Force Specified: {userInput.ForceSpecified}");
            _logger.LogDebug($"User Input Sts Settings Path Specified: {userInput.StsSettingsPath}");
            _logger.LogDebug($"User Input Tls Certificate Path Specified: {userInput.TlsCertificatePath}");

            userInput.EnsureIsValid(SetupFlowType.UpdateTlsCertificate);

            var stsSettings = new SettingsEditor(File.ReadAllText(userInput.StsSettingsPath)).
               GetStsSettings();
            _logger.LogDebug($"Sts Settings SolutionServiceId: {stsSettings.SolutionServiceId}");
            _logger.LogDebug($"Sts Settings SolutionOwnerId: {stsSettings.SolutionOwnerId}");

            var setupServiceSettings = SetupServiceSettings.FromStsSettings(stsSettings);

            setupServiceSettings.TlsCertificatePath = userInput.TlsCertificatePath;
            setupServiceSettings.TlsCertificate = new X509Certificate2(setupServiceSettings.TlsCertificatePath);
            _logger.LogDebug($"SetupServiceSettings ServiceId: {setupServiceSettings.ServiceId}");
            _logger.LogDebug($"SetupServiceSettings OwnerId: {setupServiceSettings.OwnerId}");
            _logger.LogDebug($"SetupServiceSettings TlsCertificatePath: {setupServiceSettings.TlsCertificatePath}");
            _logger.LogDebug($"SetupServiceSettings TlsCertificate Thumbprint: {setupServiceSettings.TlsCertificate?.Thumbprint}");

            K8sSettings k8sSettings = null;
            if (userInput.K8sSettings != null && File.Exists(userInput.K8sSettings)) {
               k8sSettings = JsonConvert.DeserializeObject<K8sSettings>(File.ReadAllText(userInput.K8sSettings));
            }

            // === VC Update Service Registration Actions ===
            X509CertificateValidator certificateValidator = null;
            if (userInput.ForceSpecified) {
               certificateValidator = new AcceptAllX509CertificateValidator();
            } else if (!string.IsNullOrEmpty(userInput.VcThumbprint)) {
               certificateValidator = new SpecifiedCertificateThumbprintValidator(userInput.VcThumbprint);
            }
                    
            var lookupServiceClient = new LookupServiceClient(
               userInput.Psc,
               certificateValidator);

            // --- Lookup Service Registration ---
            var lsRegistration = new LookupServiceRegistration(
               _loggerFactory,
               setupServiceSettings,
               lookupServiceClient);
            lsRegistration.UpdateServiceRegistrationTlsCertificate(userInput.User, userInput.Password);
            // --- Lookup Service Registration ---

            // === VC Update Service Registration Actions ===
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
