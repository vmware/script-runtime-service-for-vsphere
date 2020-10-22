// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes {
   public class ScriptExecutionFileReader : IScriptExecutionReader {
      private ILogger _logger;
      private string _scriptFolder;
      private IFileSystem _fileSystem;

      public ScriptExecutionFileReader(ILogger logger, string rootFolder, string userId, string scriptId):
      this(logger, rootFolder, userId, scriptId, new FileSystem()){
      }


      public ScriptExecutionFileReader(ILogger logger, string rootFolder, string userId, string scriptId, IFileSystem fileSystem) {
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
         _scriptFolder = Path.Combine(rootFolder, userId, scriptId);
         _fileSystem = fileSystem;
      }

      private bool FileExists(string fileName) {
         var filePath = Path.Combine(_scriptFolder, fileName);
         return _fileSystem.File.Exists(filePath);
      }

      private string GetFileContent(string fileName) {
         string result = null;
         try {
            var filePath = Path.Combine(_scriptFolder, fileName);
            if (_fileSystem.File.Exists(filePath)) {
               result = _fileSystem.File.ReadAllText(filePath);
            }
         } catch (Exception exc) {
            _logger.Log(LogLevel.Error, exc.ToString());
         }

         return result;
      }
      public INamedScriptExecution ReadScriptExecution() {
         INamedScriptExecution result = null;

         if (FileExists(ScriptExecutionFileNames.ScriptExecution)) {
            var jsonContent = GetFileContent(ScriptExecutionFileNames.ScriptExecution);

            try {
               result = JsonConvert.DeserializeObject<NamedScriptExecution>(jsonContent);
            } catch (Exception exc) {
               _logger.Log(LogLevel.Error, exc.ToString());
            }
         }

         return result;
      }

      public ScriptExecutionDataStreams ReadScriptExecutionDataStreams() {
         ScriptExecutionDataStreams result = null;

         if (FileExists(ScriptExecutionFileNames.ScriptExecution)) {
            var jsonContent = GetFileContent(ScriptExecutionFileNames.ScriptExecutionStreams);

            try {
               result = JsonConvert.DeserializeObject<ScriptExecutionDataStreams>(jsonContent);
            } catch (Exception exc) {
               _logger.Log(LogLevel.Error, exc.ToString());
            }
         }

         return result;
      }

      public IScriptExecutionOutputObjects ReadScriptExecutionOutput() {
         IScriptExecutionOutputObjects result = null;

         if (FileExists(ScriptExecutionFileNames.ScriptExecution)) {
            var jsonContent = GetFileContent(ScriptExecutionFileNames.ScriptExecutionOutput);

            try {
               result = JsonConvert.DeserializeObject<ScriptExecutionOutput>(jsonContent);
            } catch (Exception exc) {
               _logger.Log(LogLevel.Error, exc.ToString());
            }
         }

         return result;
      }
   }
}
