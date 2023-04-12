// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.K8sRunspaceProvider
{
   public class K8sRunspaceProviderSettings : IRunspaceProviderSettings {
      public string RunspaceImageName { get; set; }
      public int RunspacePort { get; set; }
      public int WebConsolePort { get; set; }
      public int WebConsoleCreationTimeoutMs { get; set; }
      public string ImagePullSecret { get; set; }
      public bool VerifyRunspaceApiIsAccessibleOnCreate { get; set; }
   }
}
