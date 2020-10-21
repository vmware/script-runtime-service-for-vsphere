// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Selectors;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VMware.ScriptRuntimeService.Ls;
using VMware.ScriptRuntimeService.Setup.ConfigFileWriters;
using VMware.ScriptRuntimeService.Setup.K8sClient;
using VMware.ScriptRuntimeService.Setup.SelfSignedCertificates;
using VMware.ScriptRuntimeService.Setup.TlsTrustValidators;
using VMware.ScriptRuntimeService.SsoAdmin;

namespace VMware.ScriptRuntimeService.Setup.SetupFlows
{
   public class RegisterWithVCSetupFlow : ISetupFlow
   {
      private ILoggerFactory _loggerFactory;
      private ILogger _logger;
      public RegisterWithVCSetupFlow(ILoggerFactory loggerFactory) {
         if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

         _loggerFactory = loggerFactory;
         _logger = loggerFactory.CreateLogger(typeof(RegisterWithVCSetupFlow));
      }

      public int Run(UserInput userInput) {
         try {
            var certificatesCommonName = userInput.ServiceHostname;

            _logger.LogDebug($"User Input VC: {userInput.Psc}");
            _logger.LogDebug($"User Input VC User: {userInput.User}");
            _logger.LogDebug($"User Input VC Thumbprint: {userInput.VcThumbprint}");
            _logger.LogDebug($"User Input Force Specified: {userInput.ForceSpecified}");

            userInput.EnsureIsValid(SetupFlowType.RegisterWithVC);

            // --- InMemory Certificates ---
            X509Certificate2 signCertificate = null;
            X509Certificate2 tlsCertificate = null;
            // --- InMemory Certificates ---

            
            K8sSettings k8sSettings = null;
            if (userInput.K8sSettings != null && File.Exists(userInput.K8sSettings)) {
               k8sSettings = JsonConvert.DeserializeObject<K8sSettings>(File.ReadAllText(userInput.K8sSettings));
            }

            var configWriter = new K8sConfigWriter(_loggerFactory, k8sSettings);

            // --- Signing Certificate ---
            if (!string.IsNullOrEmpty(userInput.SigningCertificatePath) &&
               File.Exists(userInput.SigningCertificatePath)) {
               _logger.LogInformation($"Load signing certificate from path {userInput.SigningCertificatePath}");
               signCertificate = new X509Certificate2(userInput.SigningCertificatePath);
            } else {
               _logger.LogInformation("Generate signing self-signed certificate");
               var signingCertGen = new SigningCertificateGenerator(
                  _loggerFactory,
                  certificatesCommonName,
                  configWriter);

               signCertificate = signingCertGen.Generate(Constants.SignCertificateSecretName);
               if (signCertificate == null) {
                  _logger.LogError("Generate signing self-signed certificate failed.");
                  return 3;
               }
            }
            // --- Signing Certificate ---

            // --- TLS Certificate ---
            if (!string.IsNullOrEmpty(userInput.TlsCertificatePath) &&
               File.Exists(userInput.TlsCertificatePath)) {
               _logger.LogInformation($"Load tls certificate from path {userInput.TlsCertificatePath}");
               tlsCertificate = new X509Certificate2(userInput.TlsCertificatePath);
            } else {
               _logger.LogInformation("Generate tls self-signed certificate");
               var tlsCertGen = new TlsCertificateGenerator(
                  _loggerFactory,
                  certificatesCommonName,
                  configWriter);

               tlsCertificate = tlsCertGen.Generate(Constants.TlsCertificateSecretName);
               if (tlsCertificate == null) {
                  _logger.LogError("Generate tls self-signed certificate failed.");
                  return 4;
               }
            }
            // --- TLS Certificate ---

            // === VC Registration Actions ===
            X509CertificateValidator certificateValidator = null;
            if (userInput.ForceSpecified) {
               certificateValidator = new AcceptAllX509CertificateValidator();
            } else if (!string.IsNullOrEmpty(userInput.VcThumbprint)) {
               certificateValidator = new SpecifiedCertificateThumbprintValidator(userInput.VcThumbprint);
            }

            var lookupServiceClient = new LookupServiceClient(
               userInput.Psc,
               certificateValidator);

            var serviceSettings = SetupServiceSettings.NewService(
                  tlsCertificate,
                  signCertificate,
                  userInput.ServiceHostname,
                  443);

            _logger.LogDebug($"Service NodeId: {serviceSettings.NodeId}");
            _logger.LogDebug($"Service OwnerId: {serviceSettings.OwnerId}");
            _logger.LogDebug($"Service ServiceId: {serviceSettings.ServiceId}");
            _logger.LogDebug($"Service Endpoint Url: {serviceSettings.EndpointUrl}");

            var ssoSdkUri = lookupServiceClient.GetSsoAdminEndpointUri();
            var stsUri = lookupServiceClient.GetStsEndpointUri();
            _logger.LogDebug($"Resolved SSO SDK Endpoint: {ssoSdkUri}");
            _logger.LogDebug($"Resolved Sts Endpoint: {stsUri}");

            var ssoAdminClient = new SsoAdminClient(ssoSdkUri, stsUri, certificateValidator);


            // --- SSO Solution User Registration ---
            var ssoSolutionRegitration = new SsoSolutionUserRegistration(
               _loggerFactory,
               serviceSettings,
               ssoAdminClient);

            ssoSolutionRegitration.CreateSolutionUser(userInput.User, userInput.Password);
            // --- SSO Solution User Registration ---

            // --- Lookup Service Registration ---
            var lsRegistration = new LookupServiceRegistration(
               _loggerFactory,
               serviceSettings,
               lookupServiceClient);
            lsRegistration.Register(userInput.User, userInput.Password);
            // --- Lookup Service Registration ---

            // --- Store VC CA certificates ---
            var trustedCertificatesStore = new TrustedCertificatesStore(
               _loggerFactory,               
               ssoAdminClient,
               configWriter);
            trustedCertificatesStore.SaveVcenterCACertficates();
            // --- Store VC CA certificates ---

            // === VC Registration Actions ===

            // --- Save SRS API Gateway service settings ---
            var stsSettings = new StsSettings();
            stsSettings.Realm = ssoSolutionRegitration.GetTrustedCertificate(
               userInput.User,
               userInput.Password)?.Thumbprint;
            stsSettings.SolutionUserSigningCertificatePath = Constants.StsSettings_SolutionUserSigningCertificatePath;
            stsSettings.StsServiceEndpoint = stsUri.ToString();
            stsSettings.SolutionOwnerId = serviceSettings.OwnerId;
            stsSettings.SolutionServiceId = serviceSettings.ServiceId;

            configWriter.WriteServiceStsSettings(stsSettings);
            // --- Save SRS API Gateway service settings ---           
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
