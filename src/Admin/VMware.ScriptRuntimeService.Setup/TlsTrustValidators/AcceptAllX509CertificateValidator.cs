﻿// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;

namespace VMware.ScriptRuntimeService.Setup.TlsTrustValidators
{
   public class AcceptAllX509CertificateValidator : X509CertificateValidator {
      public override void Validate(X509Certificate2 certificate) {
         // Check that there is a certificate.
         if (certificate == null) {
            throw new ArgumentNullException(nameof(certificate));
         }
      }
   }
}
