// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics {
   public class RunspaceStats : IRunspaceStats {
      private IRunspaceSessionInfoProvider _sessionsInfoProvider;
      private IActiveIdleInfoProvider _activeIdleInfoProvider;

      public RunspaceStats(string runspaceId,
         bool isWebConsole,
         IRunspaceSessionInfoProvider sessionInfoProvider,
         IActiveIdleInfoProvider activeIdleInfoProvider) {
         RunspaceId = runspaceId;
         IsWebConsole = isWebConsole;
         _sessionsInfoProvider = sessionInfoProvider;
         _activeIdleInfoProvider = activeIdleInfoProvider;
      }

      public void Refresh() {
         _sessionsInfoProvider.Refresh();
         _activeIdleInfoProvider.Refresh();

         HasActiveSession = _sessionsInfoProvider.IsActive;

         IsActive = _activeIdleInfoProvider.IsActive;

         ActiveTimeSeconds = _activeIdleInfoProvider.ActiveTimeSeconds;

         IdleTimeSeconds = _activeIdleInfoProvider.IdleTimeSeconds;
      }

      public string RunspaceId { get; }
      public bool IsWebConsole { get; }
      public bool HasActiveSession { get; private set; }
      public bool IsActive { get; private set; }
      public int ActiveTimeSeconds { get; private set; }
      public int IdleTimeSeconds { get; private set; }

      public override bool Equals(object obj) {
         if (obj is RunspaceStats src) {
            return src.RunspaceId == RunspaceId;
         }

         return false;
      }

      public override int GetHashCode() {
         return RunspaceId.GetHashCode();

      }
   }
}
