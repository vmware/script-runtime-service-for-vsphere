// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes {
   public class ScriptExecutionFileStoreProviderFactory : IScriptExecutionStoreProviderFactory {
      public IScriptExecutionStoreProvider Create(ILogger logger, string rootFolder, string userId, string scriptId) {
         return new ScriptExecutionFileStoreProvider(logger, rootFolder, userId, scriptId, new FileSystem());
      }

      public IScriptExecutionStoreProvider Create(
         ILogger logger,
         string rootFolder,
         string userId,
         string scriptId,
         IFileSystem fileSystem) {
         return new ScriptExecutionFileStoreProvider(logger, rootFolder, userId, scriptId, fileSystem);
      }
   }
}
