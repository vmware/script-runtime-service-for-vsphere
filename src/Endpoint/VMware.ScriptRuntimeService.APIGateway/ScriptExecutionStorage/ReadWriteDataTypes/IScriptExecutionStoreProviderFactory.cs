// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes {
   public interface IScriptExecutionStoreProviderFactory {
      IScriptExecutionStoreProvider Create(
         ILogger logger,
         string rootFolder,
         string userId,
         string scriptId);

      IScriptExecutionStoreProvider Create(
         ILogger logger,
         string rootFolder,
         string userId,
         string scriptId,
         IFileSystem fileSystem);
   }
}
