// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml;
using VMware.ScriptRuntimeService.Sts;

namespace VMware.ScriptRuntimeService.APIGateway.Sts {
   interface ISolutionStsClient {
      XmlElement IssueSolutionTokenByUserCredential(string username, SecureString password);
      XmlElement IssueSolutionTokenByToken(XmlElement samlToken);
      XmlElement IssueBearerTokenBySolutionToken(XmlElement solutionToken);
   }
}
