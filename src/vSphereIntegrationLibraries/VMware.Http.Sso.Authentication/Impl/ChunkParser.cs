// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication.Impl {
   public class ChunkParser : IEnumerator<ParamValue> {
      private const char PARAM_DELIMITER = ',';
      private string remainingTokenString;


      public ChunkParser(string token, string prefix = null) {
         if (string.IsNullOrEmpty(token)) {
            throw new AuthException(Resources.ChunkParser_EmptyToken);
         }

         remainingTokenString = token;

         if (!string.IsNullOrEmpty(prefix) && token.StartsWith(prefix)) {
            remainingTokenString = remainingTokenString.Substring(prefix.Length, remainingTokenString.Length - prefix.Length).Trim();
         }
      }

      public ParamValue Current {
         get {
                ParamValue result = null;

            if (!string.IsNullOrEmpty(remainingTokenString)) {
               var firstDelimiter = remainingTokenString.IndexOf(PARAM_DELIMITER);
               var paramLength = firstDelimiter >= 0 ? firstDelimiter + 1 : remainingTokenString.Length;

               result = ParamFormatter.Parse(remainingTokenString.Substring(0, paramLength));
            }

            return result;
         }
      }

      object IEnumerator.Current => Current;

      public void Dispose() {
      }

      public bool MoveNext() {
         var res = false;

         if (!string.IsNullOrEmpty(remainingTokenString)) {
            var firstDelimiter = remainingTokenString.IndexOf(PARAM_DELIMITER);
            if (firstDelimiter >= 0 && remainingTokenString.Length > 0) {
               remainingTokenString = remainingTokenString.Substring(firstDelimiter + 1, remainingTokenString.Length - (firstDelimiter + 1));
               res = true;
            }
         }

         return res;
      }

      public void Reset() {
      }
   }
}
