// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes {
   public interface IScriptExecutionStoreProvider : IScriptExecutionWriter, IScriptExecutionReader {
   }
}
