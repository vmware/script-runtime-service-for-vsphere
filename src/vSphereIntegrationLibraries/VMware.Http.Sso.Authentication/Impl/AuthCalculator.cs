// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Http;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication.Impl {
   public class AuthCalculator : IAuthCalculator {
      private int _maxTokenChunkLength;
      public AuthCalculator(int maxTokenChunkLength) {
         if (maxTokenChunkLength < AuthCalculatorFactory.MIN_TOKEN_CHUNK_LENGTH) {
            throw new AuthException(Resources.Max_ChunkSize_Lower_Than_Minimum_Supported);
         }

         _maxTokenChunkLength = maxTokenChunkLength;
      }

      public string[] ComputeToken(
         IRequest request, 
         X509Certificate2 signingCertificate, 
         SigningAlgorithm signingAlgorithm, 
         string samlTokenXml) {

         if (request == null) throw new ArgumentNullException(nameof(request));
         if (signingCertificate == null) throw new ArgumentNullException(nameof(signingCertificate));
         if (samlTokenXml == null) throw new ArgumentNullException(nameof(samlTokenXml));

         using (var rsaPrivateKey = signingCertificate.GetRSAPrivateKey()) {
            return new TokenFormatter().Format(
               new Token(request, rsaPrivateKey, signingAlgorithm, samlTokenXml),
               _maxTokenChunkLength
            );
         }
      }
   }
}
