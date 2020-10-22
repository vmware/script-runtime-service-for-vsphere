// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.Sts.SamlToken;

namespace VMware.Http.Sso.Authentication {
   public interface IAuthVerifier {
      ISamlToken VerifyToken(IRequest request, string[] token);
   }
}
