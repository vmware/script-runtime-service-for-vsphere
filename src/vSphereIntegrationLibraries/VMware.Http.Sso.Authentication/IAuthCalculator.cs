// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Security.Cryptography.X509Certificates;

namespace VMware.Http.Sso.Authentication {
   public interface IAuthCalculator {
      string[] ComputeToken(IRequest request,
         X509Certificate2 signingCertificate,
         SigningAlgorithm signingAlgorithm,
         string samlTokenXml);
   }
}
