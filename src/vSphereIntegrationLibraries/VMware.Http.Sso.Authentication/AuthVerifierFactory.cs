// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.Http.Sso.Authentication.Impl;

namespace VMware.Http.Sso.Authentication {
   public static class AuthVerifierFactory {
      public static IAuthVerifier Create(int clockToleranceSec, int maxRequestAgeSec) {
         return new AuthVerifier(clockToleranceSec, maxRequestAgeSec);
      }
   }
}
