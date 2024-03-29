// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Net;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.K8sRunspaceProvider {
   public class K8sWebConsoleInfo : IWebConsoleInfo
   {

      #region IWebConsoleInfo
      public string Id { get; set; }

      public RunspaceCreationState CreationState { get; set; }

      public RunspaceProviderException CreationError { get; set; }

      public IPEndPoint Endpoint { get; set; }
      #endregion
   }
}
