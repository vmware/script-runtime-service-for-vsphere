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
   /// Represents ScriptExecution Record which can be deleted.
   /// Holds LastUpdateTime and provides action for removal
   /// </summary>
   public interface IScriptExecutionRemovable {

      /// <summary>
      /// Deletes ScriptExecution Record
      /// </summary>
      void Remove();
   }
}
