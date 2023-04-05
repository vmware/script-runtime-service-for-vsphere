// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminEngine.RunspaceProviders {
   public class RunspaceProviderSettings {
      public uint MaxNumberOfRunspaces { get; set; }
      public uint MaxRunspaceIdleTimeMinutes { get; set; }
      public uint MaxRunspaceActiveTimeMinutes { get; set; }
   }
}
