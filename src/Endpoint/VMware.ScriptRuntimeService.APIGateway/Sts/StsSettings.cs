// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace VMware.ScriptRuntimeService.APIGateway.Sts {
   public class StsSettings : AuthenticationSchemeOptions {
      public StsSettings() { }

      public string Realm { get; set; }

      public string StsServiceEndpoint { get; set; }

      public string SolutionServiceId { get; set; }

      public string SolutionOwnerId { get; set; }

      public string SolutionUserSigningCertificatePath { get; set; }
   }
}
