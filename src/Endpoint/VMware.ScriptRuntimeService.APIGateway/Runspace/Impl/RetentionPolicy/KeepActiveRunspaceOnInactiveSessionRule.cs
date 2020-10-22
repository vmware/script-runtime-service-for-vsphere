// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy {
   public class KeepActiveRunspaceOnInactiveSessionRule : IRunspaceRetentionRule {
      public bool ShouldRemove(IRunspaceStats runspaceStats) {
         return !runspaceStats.HasActiveSession && !runspaceStats.IsActive;
      }
   }
}
