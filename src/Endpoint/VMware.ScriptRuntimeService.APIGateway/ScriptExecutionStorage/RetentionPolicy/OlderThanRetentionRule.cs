// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy {
   public class OlderThanRetentionRule : IScriptsRetentionRule {
      private readonly TimeSpan _olderThan;
      public OlderThanRetentionRule(TimeSpan olderThan) {
         _olderThan = olderThan;
      }

      public IPersistedScriptExecutionRecord[] Evaluate(IPersistedScriptExecutionRecord[] scripts) {
         List<IPersistedScriptExecutionRecord> result = new List<IPersistedScriptExecutionRecord>();

         var now = DateTime.Now;

         foreach (var script in scripts ?? Enumerable.Empty<IPersistedScriptExecutionRecord>()) {
            if (now - script.LastUpdateDate > _olderThan) {
               result.Add(script);
            }
         }

         return result.Count > 0 ? result.ToArray() : null;
      }
   }
}
