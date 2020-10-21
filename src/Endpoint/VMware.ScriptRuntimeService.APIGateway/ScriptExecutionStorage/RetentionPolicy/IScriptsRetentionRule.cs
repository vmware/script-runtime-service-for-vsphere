// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy {
   /// <summary>
   /// Operates on set of script records represented by their LastUpdate Time.
   /// Evaluate returns result of script records for removal.
   /// </summary>
   public interface IScriptsRetentionRule {

      /// <summary>
      /// Zero or more <see cref="IPersistedScriptExecutionRecord"/> script records
      /// </summary>
      /// <param name="scripts">Set of script records represented by record's last update date</param>
      /// <returns></returns>
      IPersistedScriptExecutionRecord[] Evaluate(IPersistedScriptExecutionRecord[] scripts);
   }
}
