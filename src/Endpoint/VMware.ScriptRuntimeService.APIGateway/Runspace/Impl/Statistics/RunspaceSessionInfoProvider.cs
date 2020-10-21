// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Authentication;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics {
   public interface IRunspaceSessionInfoProvider : IRunspaceStatDataProvider {
      bool IsActive { get; }
      string SessionId { get; }
   }

   public class RunspaceSessionInfoProvider : IRunspaceSessionInfoProvider {

      public RunspaceSessionInfoProvider(string sessionId) {
         SessionId = sessionId;
         IsActive = Sessions.Instance.IsActive(sessionId);
      }

      public bool IsActive { get; private set; }
   
      public string SessionId { get; }

      public void Refresh() {
         IsActive = Sessions.Instance.IsActive(SessionId);
      }
   }
}
