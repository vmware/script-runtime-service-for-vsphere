// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Net;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.RunspaceClient {
   public class RunspaceClientFactory : IRunspaceClientFactory {
      public IRunspace Create(IPEndPoint endpoint) {
         return new RunspaceClient(endpoint);
      }
   }
}
