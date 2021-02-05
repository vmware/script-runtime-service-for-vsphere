// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.K8sRunspaceProvider {
   public class K8sWebConsoleInfo : IWebConsoleInfo
   {

      #region IWebConsoleInfo
      public string Id { get; set; }

      public RunspaceCreationState CreationState { get; set; }

      public RunspaceProviderException CreationError { get; set; }
      #endregion
   }
}
