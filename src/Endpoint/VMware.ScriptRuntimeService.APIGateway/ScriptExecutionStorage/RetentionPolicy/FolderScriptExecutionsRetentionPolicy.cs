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
   /// <summary>
   /// Operates on persisted script executions. Script execution for which policy rules are met
   /// are being deleted.
   /// </summary>
   public class FolderScriptExecutionsRetentionPolicy {
      private ILogger _logger;
      private IFileSystem _fileSystem;
      private string _rootPath;
      private IScriptsRetentionRule[] _retentionRules;

      public FolderScriptExecutionsRetentionPolicy(
         ILoggerFactory loggerFactory,
         IFileSystem fileSystem,
         string rootPath,
         IScriptsRetentionRule[] retentionRules) {

         _logger = loggerFactory.CreateLogger(typeof(FolderScriptExecutionsRetentionPolicy));
         _fileSystem = fileSystem;
         _rootPath = rootPath;
         _retentionRules = retentionRules;
      }

      public void Apply() {
         if (!_fileSystem.Directory.Exists(_rootPath)) {
            return;
         }
         var userFolders = _fileSystem.DirectoryInfo.FromDirectoryName(_rootPath).GetDirectories();
         foreach (var userFolder in userFolders) {
            // Get user's scripts
            var scripts = _fileSystem.DirectoryInfo.FromDirectoryName(userFolder.FullName).GetDirectories();
            var userScriptRecords = new List<IPersistedScriptExecutionRecord>();
            foreach (var scriptFolder in scripts) {
               userScriptRecords.Add(new FolderPersistedScriptExecutionRecord(_logger, _fileSystem, scriptFolder.FullName));
            }

            // Apply rules for user's scripts
            foreach (var retentionRule in _retentionRules) {
               var scriptsToDelete = retentionRule.Evaluate(userScriptRecords.ToArray());
               if (scriptsToDelete != null && scriptsToDelete.Length > 0) {
                  foreach (var removableScriptRecord in scriptsToDelete) {
                     removableScriptRecord.Remove();
                  }
               }
            }
         }
      }
   }
}
