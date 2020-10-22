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

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy {
   public class FolderPersistedScriptExecutionRecord : IPersistedScriptExecutionRecord {
      private ILogger _logger;
      private IFileSystem _fileSystem;
      private string _path;
      public FolderPersistedScriptExecutionRecord(ILogger logger, IFileSystem fileSystem, string path) {
         _logger = logger;
         _fileSystem = fileSystem;
         _path = path;
      }

      public void Remove() {
         if (_fileSystem.Directory.Exists(_path)) {
            try {
               // The below removal of script execution files and folders is risky!
               //
               // If other process or thread has handle to some of the file
               // partial deletion would happen. In this case there is a risk
               // readers of the ScriptExecution data to get partial information

               // Option to fix the above is to leach item on the file system that
               // will raise flag script record is marked for deletion. Script Execution Storage
               // should read this flag and ignore this script data
               var files = _fileSystem.Directory.EnumerateFiles(_path);
               foreach (var file in files) {
                  _fileSystem.File.Delete(file);
               }

               _fileSystem.Directory.Delete(_path);
            } catch (Exception exc) {
               _logger?.Log(LogLevel.Error, exc.ToString());
            }
         }
      }

      public DateTime LastUpdateDate => _fileSystem.DirectoryInfo.FromDirectoryName(_path).LastWriteTime;
   }
}
