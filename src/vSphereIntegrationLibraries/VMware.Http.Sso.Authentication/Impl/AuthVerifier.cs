// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.Sts.SamlToken;

namespace VMware.Http.Sso.Authentication.Impl {
   public class AuthVerifier : IAuthVerifier {
      private RequestsVerifier _requestsVerifier;

      public AuthVerifier(int clockToleranceSec, int maxRequestAgeSec) {
         _requestsVerifier = new RequestsVerifier(clockToleranceSec, maxRequestAgeSec);
      }

      public ISamlToken VerifyToken(IRequest request, string[] tokenString) {
         var token = (new TokenFormatter()).Parse(tokenString);
         
         _requestsVerifier.VerifyAgeAndRepeatOnNewRequest(token.Nonce);

         ISamlToken result = null;

         try {
            result = new SamlToken(token.SamlToken);
         } catch (Exception exc) {
            throw new AuthException(exc.Message, exc);
         }

         token.VerifyBodyHash(request);
         token.VerifySignature(request, result.ConfirmationCertificate);

         return result;
      }
   }
}
