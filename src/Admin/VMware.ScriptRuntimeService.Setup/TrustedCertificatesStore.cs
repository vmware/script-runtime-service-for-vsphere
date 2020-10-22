// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VMware.ScriptRuntimeService.Setup.ConfigFileWriters;
using VMware.ScriptRuntimeService.SsoAdmin;

namespace VMware.ScriptRuntimeService.Setup {
   public class TrustedCertificatesStore {
      private readonly ILogger _logger;
      private readonly IConfigWriter _configWriter;
      private readonly SsoAdminClient _ssoAdminClient;

      public TrustedCertificatesStore(
         ILoggerFactory loggerFactory,         
         SsoAdminClient ssoAdminClient,
         IConfigWriter configWriter) {

         if (loggerFactory == null) { throw new ArgumentNullException(nameof(loggerFactory)); }
         _logger = loggerFactory.CreateLogger(typeof(TrustedCertificatesStore).FullName);         
         _ssoAdminClient = ssoAdminClient ?? throw new ArgumentNullException(nameof(ssoAdminClient));
         _configWriter = configWriter ?? throw new ArgumentNullException(nameof(configWriter));
      }


      public void SaveVcenterCACertficates() {
         try {
            _logger.LogInformation(
                  string.Format(Resources.PerofomingOperation, Resources.StoringCACertificates));
            var encodedCerts = _ssoAdminClient.GetEncodedCACertificateFromVecs();
            if (encodedCerts != null) {
               _configWriter.WriteTrustedCACertificates(encodedCerts);
            }            
         } catch (Exception ex) {
            _logger.LogError(ex.ToString());
            throw;
         }   
      }
   }
}
