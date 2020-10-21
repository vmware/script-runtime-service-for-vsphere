// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VMware.Http.Sso.Authentication.Impl;

namespace VMware.Http.Sso.Authentication {
   public interface IToken {
      string SamlToken { get; }
      Nonce Nonce { get; }
      byte[] BodyHash { get; }
      SigningAlgorithm SignatureAlgorithm { get; }
      byte[] Signature { get; }

      void VerifyBodyHash(IRequest request);

      void VerifySignature(IRequest request, X509Certificate2 signingCertificate);
   }
}
