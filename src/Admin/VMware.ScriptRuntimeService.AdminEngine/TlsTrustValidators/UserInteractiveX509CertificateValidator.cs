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
   public class UserInteractiveX509CertificateValidator : X509CertificateValidator {
      private IUserInteraction _userInteraction;
      private List<string> _acceptedCertificateThumbprints = new List<string>();
      public UserInteractiveX509CertificateValidator(IUserInteraction userInteraction) {
         _userInteraction = userInteraction ?? throw new ArgumentNullException(nameof(userInteraction));
      }
      public override void Validate(X509Certificate2 certificate) {
         // Check that there is a certificate.
         if (certificate == null) {
            throw new ArgumentNullException(nameof(certificate));
         }

         if (!_acceptedCertificateThumbprints.Contains(certificate.Thumbprint)) {
            var answer = _userInteraction.AskQuestion(
               FormatConfirmationMessage(certificate),
               new[] { "Y", "N", "y", "n" },
               "N");

            if (string.Compare(answer, "N", StringComparison.OrdinalIgnoreCase) == 0) {
               throw new SecurityTokenValidationException("Certificate was not accepted by the user");
            }
            _acceptedCertificateThumbprints.Add(certificate.Thumbprint);
         }
      }

      private string FormatConfirmationMessage(X509Certificate2 certificate) {
         return string.Format(Resources.ServerCertificatePrompt, certificate);
      }
   }
}
