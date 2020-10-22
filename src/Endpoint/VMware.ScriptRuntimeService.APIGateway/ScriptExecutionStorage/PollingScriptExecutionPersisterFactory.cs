// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage {
   public class PollingScriptExecutionPersisterFactory : IPollingScriptExecutionPersisterFactory {
      public IPollingScriptExecutionPersister Create(ILogger logger) {
         return new PollingScriptExecutionPersister(logger);
      }
   }
}
