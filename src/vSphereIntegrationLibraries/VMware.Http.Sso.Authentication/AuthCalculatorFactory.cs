// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.Http.Sso.Authentication.Impl;

namespace VMware.Http.Sso.Authentication {
   public static class AuthCalculatorFactory {
      public const int MIN_TOKEN_CHUNK_LENGTH = 128;
      public const int DEF_TOKEN_CHUNK_LENGTH = 4096;

      public static IAuthCalculator Create() {
         return Create(DEF_TOKEN_CHUNK_LENGTH);
      }

      public static IAuthCalculator Create(int maxTokenChunkLength) {
         return new AuthCalculator(maxTokenChunkLength);
      }
   }
}
