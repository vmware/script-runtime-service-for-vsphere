// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy {
   public class RemoveExpiredActiveRunspaceRule : IRunspaceRetentionRule {
      private int _maxActiveTimeoutMinutes;
      public RemoveExpiredActiveRunspaceRule(int maxActiveTimeoutMinutes) {
         _maxActiveTimeoutMinutes = maxActiveTimeoutMinutes;
      }
      public bool ShouldRemove(IRunspaceStats runspaceStats) {
         return 
            runspaceStats.IsActive && 
            (runspaceStats.ActiveTimeSeconds > (_maxActiveTimeoutMinutes * 60));
      }
   }
}
