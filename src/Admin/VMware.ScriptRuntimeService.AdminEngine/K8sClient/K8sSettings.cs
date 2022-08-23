// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminEngine.K8sClient {
   public class K8sSettings {
      public string ClusterEndpoint { get; set; }
      public string AccessToken { get; set; }
      public string Namespace { get; set; }
   }
}
