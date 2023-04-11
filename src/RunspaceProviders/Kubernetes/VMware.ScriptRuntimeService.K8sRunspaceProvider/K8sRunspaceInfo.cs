// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Net;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.K8sRunspaceProvider {
   public class K8sRunspaceInfo : IRunspaceInfo, IWebConsoleInfo {

      #region IRunspaceInfo
      public string Id { get; set; }

      public IPEndPoint Endpoint { get; set; }

      public RunspaceCreationState CreationState { get; set; }

      public RunspaceProviderException CreationError { get; set; }
      #endregion
   }
}
