// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IdentityModel.Selectors;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;
using VMware.ScriptRuntimeService.AdminEngine.SelfSignedCertificates;
using VMware.ScriptRuntimeService.AdminEngine.TlsTrustValidators;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration.Exceptions;
using VMware.ScriptRuntimeService.Ls;
using VMware.ScriptRuntimeService.SsoAdmin;

namespace VMware.ScriptRuntimeService.AdminEngine.VCRegistration {
   /// <summary>
   /// Service like class that runs specific setup flows
   /// </summary>
   public class VCRegistrator {
      private readonly ILoggerFactory _loggerFactory;
      private readonly IConfigWriter _configWriter;
      private readonly IConfigReader _configReader;
      private readonly ILogger _logger;

      public VCRegistrator(ILoggerFactory loggerFactory, IConfigWriter configWriter, IConfigReader configReader) {
         _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
         _logger = loggerFactory.CreateLogger(typeof(VCRegistrator));
         _configWriter = configWriter ?? throw new ArgumentNullException(nameof(configWriter));
         _configReader = configReader ?? throw new ArgumentNullException(nameof(configReader));
      }

      public string GetRegisteredVC() {
         var stsSettings = _configReader.ReadServiceStsSettings();

         if (null == stsSettings || string.IsNullOrEmpty(stsSettings.VCenterAddress)) {
            throw new SrsNotRegisteredException();
         }

         return stsSettings.VCenterAddress;
      }

      public void UpdateTlsCertificate(
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force,
         string stsSettingsPath,
         string tlsCertificatePath) {

         if (!string.IsNullOrEmpty(thumbprint)) {
            thumbprint = FormatThumbprint(thumbprint);
         }

         X509CertificateValidator certificateValidator = GetCertificateValidator(thumbprint, force);

         var lookupServiceClient = GetLookupServiceClient(
            psc,
            certificateValidator);

         var stsSettings = new SettingsEditor(File.ReadAllText(stsSettingsPath)).
               GetStsSettings();
         _logger.LogDebug($"Sts Settings SolutionServiceId: {stsSettings.SolutionServiceId}");
         _logger.LogDebug($"Sts Settings SolutionOwnerId: {stsSettings.SolutionOwnerId}");

         var setupServiceSettings = SetupServiceSettings.FromStsSettings(stsSettings);
         setupServiceSettings.TlsCertificatePath = tlsCertificatePath;
         setupServiceSettings.TlsCertificate = new X509Certificate2(setupServiceSettings.TlsCertificatePath);
         _logger.LogDebug($"SetupServiceSettings ServiceId: {setupServiceSettings.ServiceId}");
         _logger.LogDebug($"SetupServiceSettings OwnerId: {setupServiceSettings.OwnerId}");
         _logger.LogDebug($"SetupServiceSettings TlsCertificatePath: {setupServiceSettings.TlsCertificatePath}");
         _logger.LogDebug($"SetupServiceSettings TlsCertificate Thumbprint: {setupServiceSettings.TlsCertificate?.Thumbprint}");

         var lsRegistration = new LookupServiceRegistration(
               _loggerFactory,
               setupServiceSettings,
               lookupServiceClient);
         lsRegistration.UpdateServiceRegistrationTlsCertificate(username, password);
      }

      public void UpdateTrustedCACertificates(
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force) {

         if (!string.IsNullOrEmpty(thumbprint)) {
            thumbprint = FormatThumbprint(thumbprint);
         }

         var trustedCertificatesCollector = GetTrustedCertificatesCollector(psc, username, password, thumbprint, force);

         StoreVCCACertificates(trustedCertificatesCollector);
      }

      public void Clean(
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force) {
         _logger.LogInformation("Cleaning VC registration");

         _logger.LogDebug($"VC: {psc}");
         _logger.LogDebug($"VC User: {username}");
         _logger.LogDebug($"VC Thumbprint: {thumbprint}");
         _logger.LogDebug($"Force Specified: {force}");

         if (!string.IsNullOrEmpty(thumbprint)) {
            thumbprint = FormatThumbprint(thumbprint);
            _logger.LogDebug($"VC Thumbprint (reformatted): {thumbprint}");
         }

         // === VC Unregister Actions ===
         X509CertificateValidator certificateValidator = null;
         if (force) {
            certificateValidator = new AcceptAllX509CertificateValidator();
         } else if (!string.IsNullOrEmpty(thumbprint)) {
            certificateValidator = new SpecifiedCertificateThumbprintValidator(thumbprint);
         }

         var lookupServiceClient = new LookupServiceClient(
            psc,
            certificateValidator);

         var registeredServices = lookupServiceClient.ListRegisteredServices();
         string srsServiceId = null;
         string srsOwnerId = null;
         foreach (var service in registeredServices) {
            if (service.ownerId?.StartsWith("srs-SolutionOwner-") ?? false) {
               // SRS Service registration found
               srsServiceId = service.serviceId;
               srsOwnerId = service.ownerId;
               break;
            }
         }

         if (!string.IsNullOrEmpty(srsServiceId) && !string.IsNullOrEmpty(srsOwnerId)) {
            _logger.LogInformation($"SRS Service registration found on VC {psc}, service Id: {srsServiceId}, service owner Id: {srsOwnerId}");
            _logger.LogInformation("Performing SRS Service regitration cleanup");

            UnregisterInternal(
               psc,
               username,
               password,
               thumbprint,
               force,
               srsServiceId,
               srsOwnerId);
         } else {
            _logger.LogInformation($"SRS Service registration not found on VC {psc}");
         }
      }

      public void Unregister(
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force) {

         var stsSettings = _configReader.ReadServiceStsSettings();

         UnregisterInternal(
            psc,
            username,
            password,
            thumbprint,
            force,
            stsSettings.SolutionServiceId,
            stsSettings.SolutionOwnerId);

      }

      public void Unregister(
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force,
         string stsSettingsPath) {
         _logger.LogInformation("Unregister from VC");

         _logger.LogDebug($"VC: {psc}");
         _logger.LogDebug($"VC User: {username}");
         _logger.LogDebug($"VC Thumbprint: {thumbprint}");
         _logger.LogDebug($"Force Specified: {force}");
         _logger.LogDebug($"Sts Settings Path Specified: {stsSettingsPath}");

         var stsSettings = new SettingsEditor(File.ReadAllText(stsSettingsPath)).
            GetStsSettings();

         UnregisterInternal(
            psc,
            username,
            password,
            thumbprint,
            force,
            stsSettings.SolutionServiceId,
            stsSettings.SolutionOwnerId);

      }

      private void UnregisterInternal(
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force,
         string solutionServiceId,
         string solutionOwnerId) {

         if (!string.IsNullOrEmpty(thumbprint)) {
            thumbprint = FormatThumbprint(thumbprint);
            _logger.LogDebug($"VC Thumbprint (reformatted): {thumbprint}");
         }

         // === VC Unregister Actions ===
         X509CertificateValidator certificateValidator = null;
         if (force) {
            certificateValidator = new AcceptAllX509CertificateValidator();
         } else if (!string.IsNullOrEmpty(thumbprint)) {
            certificateValidator = new SpecifiedCertificateThumbprintValidator(thumbprint);
         }

         var setupServiceSettings = SetupServiceSettings.FromStsSettings(new AdminEngine.StsSettings {
            SolutionServiceId = solutionServiceId,
            SolutionOwnerId = solutionOwnerId
         });

         _logger.LogDebug($"SetupServiceSettings ServiceId: {setupServiceSettings.ServiceId}");
         _logger.LogDebug($"SetupServiceSettings OwnerId: {setupServiceSettings.OwnerId}");

         var lookupServiceClient = new LookupServiceClient(
            psc,
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

         ssoSolutionRegitration.DeleteSolutionUser(username, password);
         // --- SSO Solution User Registration ---

         // --- Lookup Service Registration ---
         var lsRegistration = new LookupServiceRegistration(
            _loggerFactory,
            setupServiceSettings,
            lookupServiceClient);
         lsRegistration.Deregister(username, password);

         // --- Write empty STS Settings ---
         _configWriter.WriteServiceStsSettings(new StsSettings());
         // --- Write empty STS  Settings ---
      }

      public void Register(
         string hostname,
         string signingCertificatePath,
         string tlsCertificatePath,
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force) {
         Register(
            hostname,
            signingCertificatePath,
            tlsCertificatePath,
            psc,
            username,
            password,
            thumbprint,
            force,
            false);
      }

      public void Register(
         string hostname,
         string signingCertificatePath,
         string tlsCertificatePath,
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool force,
         bool cleanPriorToRegistring) {

         _logger.LogInformation("Registring with VC");

         _logger.LogDebug($"VC: {psc}");
         _logger.LogDebug($"VC User: {username}");
         _logger.LogDebug($"VC Thumbprint: {thumbprint}");
         _logger.LogDebug($"Force Specified: {force}");
         _logger.LogDebug($"Clean before registring: {cleanPriorToRegistring}");

         if (!string.IsNullOrEmpty(thumbprint)) {
            thumbprint = FormatThumbprint(thumbprint);
            _logger.LogDebug($"VC Thumbprint (reformatted): {thumbprint}");
         }

         if (cleanPriorToRegistring) {
            Clean(psc, username, password, thumbprint, force);
         }

         var certificates = SetupCertificatesInternal(hostname, signingCertificatePath, tlsCertificatePath);

         var serviceSettings = SetupServiceSettings.NewService(
               certificates.TlsCertificate,
               certificates.SignCertificate,
               hostname,
               443);

         _logger.LogDebug($"Service NodeId: {serviceSettings.NodeId}");
         _logger.LogDebug($"Service OwnerId: {serviceSettings.OwnerId}");
         _logger.LogDebug($"Service ServiceId: {serviceSettings.ServiceId}");
         _logger.LogDebug($"Service Endpoint Url: {serviceSettings.EndpointUrl}");

         X509CertificateValidator certificateValidator = GetCertificateValidator(thumbprint, force);
         var lookupServiceClient = GetLookupServiceClient(psc, certificateValidator);
         var ssoAdminClient = GetSsoAdminClient(lookupServiceClient, certificateValidator);
         var trustedCertificatesCollector = GetTrustedCertificatesCollector(psc, username, password, thumbprint, force);

         // --- SSO Solution User Registration ---
         var ssoSolutionRegitration = new SsoSolutionUserRegistration(
            _loggerFactory,
            serviceSettings,
            ssoAdminClient);

         ssoSolutionRegitration.CreateSolutionUser(username, password);
         // --- SSO Solution User Registration ---

         // --- Lookup Service Registration ---
         var lsRegistration = new LookupServiceRegistration(
            _loggerFactory,
            serviceSettings,
            lookupServiceClient);
         lsRegistration.Register(username, password);
         // --- Lookup Service Registration ---

         StoreVCCACertificates(trustedCertificatesCollector);

         // === VC Registration Actions ===

         // --- Save SRS API Gateway service settings ---
         var stsSettings = new StsSettings {
            Realm = ssoSolutionRegitration.GetTrustedCertificate(
            username,
            password)?.Thumbprint,
            SolutionUserSigningCertificatePath = Constants.StsSettings_SolutionUserSigningCertificatePath,
            StsServiceEndpoint = lookupServiceClient.GetStsEndpointUri().ToString(),
            SolutionOwnerId = serviceSettings.OwnerId,
            SolutionServiceId = serviceSettings.ServiceId,
            VCenterAddress = psc
         };

         _configWriter.WriteServiceStsSettings(stsSettings);

         _logger.LogInformation("Registring with VC successful");
      }

      public void SetupCertificates(string hostname, string signingCertificatePath, string tlsCertificatePath) {
         SetupCertificatesInternal(hostname, signingCertificatePath, tlsCertificatePath);
      }

      private CertificatePair SetupCertificatesInternal(string hostname, string signingCertificatePath, string tlsCertificatePath) {
         _logger.LogInformation("Setting up certificates");

         _logger.LogDebug($"HostName: {hostname}");
         _logger.LogDebug($"Signing Certificate Path: {signingCertificatePath}");
         _logger.LogDebug($"Tls Certificate Path: {tlsCertificatePath}");

         X509Certificate2 signCertificate = GetOrGenerateSigningCertificate(
            signingCertificatePath,
            hostname);

         X509Certificate2 tlsCertificate = GetOrGenerateTlsCertificate(
            tlsCertificatePath,
            hostname);

         // --- Write empty STS Settings ---
         _configWriter.WriteServiceStsSettings(new StsSettings());
         // --- Write empty STS  Settings ---

         _logger.LogInformation("Setting up certificates successful");

         return new CertificatePair(signCertificate, tlsCertificate);
      }

      private class CertificatePair {
         public X509Certificate2 SignCertificate { get; }
         public X509Certificate2 TlsCertificate { get; }

         public CertificatePair(X509Certificate2 sign, X509Certificate2 tls) {
            SignCertificate = sign;
            TlsCertificate = tls;
         }
      }

      public X509Certificate2 GetOrGenerateSigningCertificate(string signingCertificatePath, string certificatesCommonName) {
         return GetOrGenerateCertificate<SigningCertificateGenerator>(
            signingCertificatePath,
            certificatesCommonName,
            Constants.SignCertificateSecretName);
      }

      public X509Certificate2 GetOrGenerateTlsCertificate(string tlsCertificatePath, string certificatesCommonName) {
         return GetOrGenerateCertificate<TlsCertificateGenerator>(
            tlsCertificatePath,
            certificatesCommonName,
            Constants.TlsCertificateSecretName);
      }

      private X509Certificate2 GetOrGenerateCertificate<T>(string certificatePath, string commonName, string certificateName) where T : ISelfSignedCertificateGenerator {
         _logger.LogDebug($"GetOrGenerateCertificate<{typeof(T)}>('{certificatePath}', '{commonName}', '{certificateName}')");
         X509Certificate2 certificate = null;
         if (!string.IsNullOrEmpty(certificatePath) &&
               File.Exists(certificatePath)) {
            _logger.LogInformation($"Load certificate from path {certificatePath}");
            certificate = new X509Certificate2(certificatePath);
         } else {
            _logger.LogInformation("Generate self-signed certificate");
            var certGen = (ISelfSignedCertificateGenerator) Activator.CreateInstance(
               typeof(T),
               _loggerFactory,
               commonName,
               _configWriter);

            certificate = certGen.Generate(certificateName);
            if (certificate == null) {
               _logger.LogError("Generate self-signed certificate failed.");
               throw new SelfSignedCertificateGenerationException("Generate self-signed certificate failed.");
            }
         }

         return certificate;
      }

      private static X509CertificateValidator GetCertificateValidator(string thumbprint, bool force) {
         X509CertificateValidator certificateValidator = null;
         if (force) {
            certificateValidator = new AcceptAllX509CertificateValidator();
         } else if (!string.IsNullOrEmpty(thumbprint)) {
            certificateValidator = new SpecifiedCertificateThumbprintValidator(thumbprint);
         }

         return certificateValidator;
      }

      private static LookupServiceClient GetLookupServiceClient(string psc, X509CertificateValidator certificateValidator) {
         return new LookupServiceClient(
            psc,
            certificateValidator);
      }

      private SsoAdminClient GetSsoAdminClient(LookupServiceClient lookupServiceClient, X509CertificateValidator certificateValidator) {

         // Get SSO Admin And STS URIs from Lookup Service
         var ssoSdkUri = lookupServiceClient.GetSsoAdminEndpointUri();
         var stsUri = lookupServiceClient.GetStsEndpointUri();
         _logger.LogDebug($"Resolved SSO SDK Endpoint: {ssoSdkUri}");
         _logger.LogDebug($"Resolved Sts Endpoint: {stsUri}");

         return new SsoAdminClient(ssoSdkUri, stsUri, certificateValidator);
      }

      private VCTrustedCertificatesCollector GetTrustedCertificatesCollector(string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool ignoreServerCertificateValidation) {

         return new VCTrustedCertificatesCollector(_loggerFactory, psc, username, password, thumbprint, ignoreServerCertificateValidation);
      }

      private void StoreVCCACertificates(VCTrustedCertificatesCollector trustedCertificatesCollector) {
         var trustedCertificatesStore = new TrustedCertificatesStore(
                     _loggerFactory,
                     trustedCertificatesCollector,
                     _configWriter);
         trustedCertificatesStore.SaveVcenterCACertificates();
      }

      internal static string FormatThumbprint(string thumbprint) {
         if (thumbprint is null) {
            throw new ArgumentNullException(nameof(thumbprint));
         }
         if (string.IsNullOrEmpty(thumbprint)) {
            throw new ArgumentException(nameof(thumbprint));
         }

         thumbprint = thumbprint.Trim().Replace(":", "").Replace(" ", "").ToUpperInvariant();

         // SHA1 - 40 symbols, SHA256 - 64
         if (thumbprint.Length != 40 && thumbprint.Length != 64) {
            throw new FormatException("Invalid length. The thumbprint should contain 40 not-separator symbols for SHA1 or 64 non-separator symbols for SHA256.");
         }

         var matches = Regex.Matches(thumbprint, "[^0-9A-F]", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);

         if (matches.Count > 0) {
            var invalidCharacters = matches.Select(m => m.Value).ToArray();
            throw new FormatException($"Thumbprint contains invalid characters '{string.Join("','", invalidCharacters)}'");
         }

         StringBuilder sb = new StringBuilder();

         for (int i = 0; i < thumbprint.Length; i++) {
            if (i % 2 == 0) {
               sb.Append(':');
            }
            sb.Append(thumbprint[i]);
         }


         return sb.ToString().Trim(':');
      }
   }
}
