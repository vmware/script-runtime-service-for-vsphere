// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VMware.ScriptRuntimeService.AdminEngine.TlsTrustValidators {
   public class SelectedCertificatesX509CertificateValidator : X509CertificateValidator {
      private List<string> _acceptableCertificateThumbprints = new List<string>();
      public SelectedCertificatesX509CertificateValidator(X509Certificate2[] selectedCertificates) {
         if (selectedCertificates != null && selectedCertificates.Length > 0) {
            foreach (var cert in selectedCertificates) {
               _acceptableCertificateThumbprints.Add(cert.Thumbprint);
            }
         }
      }
      public override void Validate(X509Certificate2 certificate) {
         // Check there is a certificate.
         if (certificate == null) {
            throw new ArgumentNullException(nameof(certificate));
         }

         if (!_acceptableCertificateThumbprints.Contains(certificate.Thumbprint)) {
            throw new SecurityTokenValidationException($"Certificate {certificate} doesn't present in selected trust certificates");
         }
      }
   }
}
