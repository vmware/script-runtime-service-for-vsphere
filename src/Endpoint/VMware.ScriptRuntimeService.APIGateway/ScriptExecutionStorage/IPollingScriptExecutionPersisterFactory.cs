// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage {
   public interface IPollingScriptExecutionPersisterFactory {
      IPollingScriptExecutionPersister Create(ILogger logger);
   }
}
