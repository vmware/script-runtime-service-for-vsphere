// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.IO;
using System.Reflection;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.SystemScripts {
   public static class SystemScripts {
      private static string ScriptsBasePath = "SystemScripts";
      public static string ConvertToJsonWithTypeInfo {
         get {
            var scriptPath = Path.Combine(ScriptsBasePath, "ConvertTo-JsonWithTypeInfo.ps1");
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullScriptPath = Path.Combine(assemblyPath, scriptPath);
            return File.ReadAllText(fullScriptPath);
         }
      }
   }
}
