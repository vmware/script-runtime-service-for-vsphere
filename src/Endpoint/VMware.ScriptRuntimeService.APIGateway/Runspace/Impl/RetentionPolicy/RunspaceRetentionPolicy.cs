// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy {
   public class RunspaceRetentionPolicy {
      private IRunspaceRetentionRule[] _retentionRules;
      public RunspaceRetentionPolicy(IRunspaceRetentionRule[] retentionRules) {
         _retentionRules = retentionRules;
      }

      public bool ShouldRemove(IRunspaceStats runspaceStats) {
         bool result = false;

         foreach (var rule in _retentionRules ?? Enumerable.Empty<IRunspaceRetentionRule>()) {
            result = rule.ShouldRemove(runspaceStats);
            // Break on first positive ShouldRemove answer
            if (result) {
               break;
            }
         }

         return result;
      }
   }
}
