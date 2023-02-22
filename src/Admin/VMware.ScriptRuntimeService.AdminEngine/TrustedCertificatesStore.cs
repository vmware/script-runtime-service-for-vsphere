// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using System;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;

namespace VMware.ScriptRuntimeService.AdminEngine {
   public class TrustedCertificatesStore {
      private readonly ILogger _logger;
      private readonly IConfigWriter _configWriter;
      private readonly VCTrustedCertificatesCollector _trustedCertificatesCollector;

      public TrustedCertificatesStore(
         ILoggerFactory loggerFactory,
         VCTrustedCertificatesCollector trustedCertificatesCollector,
         IConfigWriter configWriter) {

         if (loggerFactory == null) { throw new ArgumentNullException(nameof(loggerFactory)); }
         _logger = loggerFactory.CreateLogger(typeof(TrustedCertificatesStore).FullName);
         _trustedCertificatesCollector = trustedCertificatesCollector ?? throw new ArgumentNullException(nameof(trustedCertificatesCollector));
         _configWriter = configWriter ?? throw new ArgumentNullException(nameof(configWriter));
      }


      public void SaveVcenterCACertificates() {
         try {
            _logger.LogInformation(
                  string.Format(Resources.PerofomingOperation, Resources.StoringCACertificates));
            var encodedCerts = _trustedCertificatesCollector.GetEncodedTrustedCertificates();
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
