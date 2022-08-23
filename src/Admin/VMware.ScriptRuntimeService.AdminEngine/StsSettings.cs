// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.AdminEngine {

   public class StsSettings {
      public string Realm { get; set; }

      public string StsServiceEndpoint { get; set; }

      public string SolutionServiceId { get; set; }

      public string SolutionOwnerId { get; set; }

      public string SolutionUserSigningCertificatePath { get; set; }
   }
}
