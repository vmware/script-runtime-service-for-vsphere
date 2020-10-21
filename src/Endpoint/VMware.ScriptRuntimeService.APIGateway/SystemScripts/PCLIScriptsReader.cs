// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;

namespace VMware.ScriptRuntimeService.APIGateway.SystemScripts {
   public static class PCLIScriptsReader {
      private static string PCLIScriptsBasePath = Path.Combine("SystemScripts", "PCLIScripts");
      private static string ArgumentScriptsDirName = "ArgumentTransformationScripts";

      private static string GetFullPath(string itemName) {
         var itemPath = Path.Combine(PCLIScriptsBasePath, itemName);
         var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
         return Path.Combine(assemblyPath, itemPath);
      }

      #region Public Interface
      public static string ConnectByStringSamlToken => File.ReadAllText(GetFullPath("ConnectByStringSamlToken.ps1"));

      public static IEnumerable<ArgumentScriptTemplate> ListArgumentTransformationScriptNames() {
         var scriptsDir = GetFullPath(ArgumentScriptsDirName);
         foreach (var filePath in Directory.GetFiles(scriptsDir)) {
            var script_id = Path.GetFileNameWithoutExtension(filePath);
            yield return GetArgumentTransformationScript(script_id);
         }
      }

      public static bool ArgumentTransformationExists(string name) {
         var filePath = GetFullPath(Path.Combine(ArgumentScriptsDirName, $"{name}.json"));

         return File.Exists(filePath);
      }

      public static ArgumentScriptTemplate GetArgumentTransformationScript(string name) {
         ArgumentScriptTemplate result = null;
         var filePath = GetFullPath(Path.Combine(ArgumentScriptsDirName, $"{name}.json"));

         if (File.Exists(filePath)) {
            try {
               var content = File.ReadAllText(filePath);
               result = JsonConvert.DeserializeObject<ArgumentScriptTemplate>(content);
            } catch (Exception) { }
            
         }

         return result;
      }
      #endregion
   }
}
