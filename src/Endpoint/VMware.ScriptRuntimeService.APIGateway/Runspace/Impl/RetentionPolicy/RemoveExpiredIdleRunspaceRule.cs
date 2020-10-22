// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy {
   public class RemoveExpiredIdleRunspaceRule : IRunspaceRetentionRule {
      private int _maxIdleTimeoutMinutes;
      public RemoveExpiredIdleRunspaceRule(int maxIdleTimeoutMinutes) {
         _maxIdleTimeoutMinutes = maxIdleTimeoutMinutes;
      }
      public bool ShouldRemove(IRunspaceStats runspaceStats) {
         return 
            !runspaceStats.IsActive && 
            (runspaceStats.IdleTimeSeconds > (_maxIdleTimeoutMinutes * 60));
      }
   }
}
