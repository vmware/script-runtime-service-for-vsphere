// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using VMware.ScriptRuntimeService.SsoAdmin;

namespace VMware.ScriptRuntimeService.AdminEngine {
   public class SsoSolutionUserRegistration {
      private readonly SetupServiceSettings _serviceSettings;
      private readonly ILogger _logger;
      private readonly SsoAdminClient _ssoAdminClient;

      public SsoSolutionUserRegistration(
         ILoggerFactory loggerFactory,
         SetupServiceSettings serviceSettings,
         SsoAdminClient ssoAdminClient) {

         if (loggerFactory == null) { throw new ArgumentNullException(nameof(loggerFactory)); }
         _logger = loggerFactory.CreateLogger(typeof(SsoSolutionUserRegistration).FullName);
         _serviceSettings = serviceSettings ?? throw new ArgumentNullException(nameof(serviceSettings));
         _ssoAdminClient = ssoAdminClient ?? throw new ArgumentNullException(nameof(ssoAdminClient));

      }

      public void CreateSolutionUser(string user, SecureString password) {
         try {
            _logger.LogInformation(
               string.Format(Resources.PerofomingOperation, Resources.RegisteringSolutionUser));
            _ssoAdminClient.CreateLocalSolutionUser(
               user,
               password,
               _serviceSettings.OwnerId,
               _serviceSettings.SigningCertificate,
               string.Empty);
         } catch (Exception ex) {
            _logger.LogError(ex.ToString());
            throw;
         }
      }

      public void DeleteSolutionUser(string user, SecureString password) {
         try {
            _logger.LogInformation(
               string.Format(Resources.PerofomingOperation, Resources.DeleteSolutionUserOperation));
            _ssoAdminClient.DeleteLocalPrincipal(
               user,
               password,
               _serviceSettings.OwnerId);
         } catch (Exception ex) {
            _logger.LogError(ex.ToString());
            throw;
         }
      }

      public X509Certificate2 GetTrustedCertificate(string user, SecureString password) {
         try {
            return _ssoAdminClient.GetTrustedCertificatesAsync(
               user,
               password).FirstOrDefault();
         } catch (Exception ex) {
            _logger.LogError(ex.ToString());
            throw;
         }
      }
   }
}
