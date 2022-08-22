// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminApi {
   public class VCenterStsSettings {
      public string VCenterAddress { get; set; }
      public string Realm { get; set; }
      public string StsServiceEndpoint { get; set; }
      public string SolutionServiceId { get; set; }
      public string SolutionOwnerId { get; set; }
      public string SolutionUserSigningCertificatePath { get; set; }
   }
}
