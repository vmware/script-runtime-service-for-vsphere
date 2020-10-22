// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication.Impl {
   public static class SigningAlgorithmConverter {
      private const string SHA256 = "RSA-SHA256";
      private const string SHA384 = "RSA-SHA384";
      private const string SHA512 = "RSA-SHA512";

      public static string EnumToString(SigningAlgorithm signingAlgorithm) {
         string result = null;
         switch (signingAlgorithm) {
            case SigningAlgorithm.RSA_SHA256:
               result = SHA256;
               break;
            case SigningAlgorithm.RSA_SHA384:
               result = SHA384;
               break;
            case SigningAlgorithm.RSA_SHA512:
               result = SHA512;
               break;
         }

         if (string.IsNullOrEmpty(result)) {
            throw new AuthenticationException(string.Format(Resources.Unsupported_Signing_Algorithm, signingAlgorithm));
         }

         return result;
      }


      public static SigningAlgorithm StringToEnum(string signingAlgorithm) {
         SigningAlgorithm? result = null;
         if (signingAlgorithm == SHA256) {
            result = SigningAlgorithm.RSA_SHA256;
         } else if (signingAlgorithm == SHA384) {
            result = SigningAlgorithm.RSA_SHA384;
         } else if (signingAlgorithm == SHA512) {
            result = SigningAlgorithm.RSA_SHA512;
         }

         if (result == null) {
            throw new AuthenticationException(string.Format(Resources.Unsupported_Signing_Algorithm, signingAlgorithm));
         }

         return result.Value;
      }
      public static SigningAlgorithm ToSigningAlgorithm(HashAlgorithmName hashAlgorithm) {
         var res = SigningAlgorithm.RSA_SHA256;

         if (hashAlgorithm == HashAlgorithmName.SHA256) {
            res = SigningAlgorithm.RSA_SHA256;
            if (hashAlgorithm == HashAlgorithmName.SHA384) {
               res = SigningAlgorithm.RSA_SHA384;
            } else if (hashAlgorithm == HashAlgorithmName.SHA512) {
               res = SigningAlgorithm.RSA_SHA512;
            } else {
               throw new AuthenticationException(
                  string.Format(
                     Resources.Hash_Algorithm_Not_Supported,
                     hashAlgorithm.Name));
            }
         }

         return res;
      }

      public static HashAlgorithmName ToHashAlgorithmName(SigningAlgorithm signingAlgorithm) {
         switch (signingAlgorithm) {
            case SigningAlgorithm.RSA_SHA256:
               return HashAlgorithmName.SHA256;
            case SigningAlgorithm.RSA_SHA384:
               return HashAlgorithmName.SHA384;
            case SigningAlgorithm.RSA_SHA512:
               return HashAlgorithmName.SHA512;
            default:
               return HashAlgorithmName.SHA256;
         }
      }
   }
}
