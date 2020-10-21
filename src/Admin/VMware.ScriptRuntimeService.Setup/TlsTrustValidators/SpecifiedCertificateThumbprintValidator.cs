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

namespace VMware.ScriptRuntimeService.Setup.TlsTrustValidators
{
   public class SpecifiedCertificateThumbprintValidator : X509CertificateValidator {      
      private List<string> _acceptableCertificateThumbprints = new List<string>();
      public SpecifiedCertificateThumbprintValidator(string thumbprint) {
         if (!string.IsNullOrEmpty(thumbprint)) {
            _acceptableCertificateThumbprints.Add(thumbprint.Replace(":", "").ToLower());
         }         
      }
      public override void Validate(X509Certificate2 certificate) {
         // Check there is a certificate.
         if (certificate == null) {
            throw new ArgumentNullException(nameof(certificate));
         }

         if (certificate.Thumbprint == null || !_acceptableCertificateThumbprints.Contains(certificate.Thumbprint.ToLower())) {
            throw new SecurityTokenValidationException($"Certificate {certificate} doesn't present in selected trust certificates");
         }
      }
   }
}
