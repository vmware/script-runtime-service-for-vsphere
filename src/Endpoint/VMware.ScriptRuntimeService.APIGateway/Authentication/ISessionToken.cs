// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.Sts.SamlToken;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication {      
   internal interface ISessionToken {
      string UserName { get; set; }

      string SessionId { get; set; }
      ISamlToken HoKSamlToken { get; set; }
   }
}
