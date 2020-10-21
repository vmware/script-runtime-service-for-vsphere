// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Net;

namespace VMware.ScriptRuntimeService.Runspace.Types {
   public interface IRunspaceClientFactory {
      IRunspace Create(IPEndPoint endpoint);
   }
}
