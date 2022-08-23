// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminEngine.VCRegistration {
   public class CommonSettings {
      public string Hostname { get; set; }
      public string TlsCertificatePath { get; set; }
      public string SolutionUserSigningCertificatePath { get; set; }
      public string ConfigMap { get; set; }
   }
}
