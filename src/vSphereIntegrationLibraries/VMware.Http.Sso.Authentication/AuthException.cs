// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.Http.Sso.Authentication {
   public class AuthException : Exception {
      public AuthException(string message)
         : base(message) { }

      public AuthException(string message, Exception exception)
         : base(message, exception) { }
   }
}
