// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication {
   public enum SigningAlgorithm {
      RSA_SHA256,
      RSA_SHA384,
      RSA_SHA512,
   }
}
