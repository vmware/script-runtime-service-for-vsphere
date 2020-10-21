// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy {
   public class LastNumberOfRecordsRetentionRule : IScriptsRetentionRule {
      private readonly int _maxNumberOfRecords;
      public LastNumberOfRecordsRetentionRule(int maxNumberOfRecords) {
         _maxNumberOfRecords = maxNumberOfRecords;
      }

      public IPersistedScriptExecutionRecord[] Evaluate(IPersistedScriptExecutionRecord[] scripts) {
         IPersistedScriptExecutionRecord[] result = null;

         if (scripts?.Length > _maxNumberOfRecords) {
            List<IPersistedScriptExecutionRecord> records = new List<IPersistedScriptExecutionRecord>(scripts);
            records.Sort(
               Comparer<IPersistedScriptExecutionRecord>.Create(
               (r1, r2) => r1.LastUpdateDate.CompareTo(r2.LastUpdateDate)));

            result = records.GetRange(0, records.Count - _maxNumberOfRecords).ToArray();
         }

         return result;
      }
   }
}
