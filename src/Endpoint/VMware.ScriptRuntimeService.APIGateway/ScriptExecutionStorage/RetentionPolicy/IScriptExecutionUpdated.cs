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
   /// Represents last update of script execution record
   /// </summary>
   public interface IScriptExecutionUpdated {
      /// <summary>
      /// Last Update Time of the ScriptExecution record
      /// </summary>
      DateTime LastUpdateDate { get; }
   }
}
