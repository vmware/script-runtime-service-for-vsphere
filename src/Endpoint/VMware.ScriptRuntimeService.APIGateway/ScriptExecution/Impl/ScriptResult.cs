// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl
{
   public class ScriptResult : INamedScriptExecution {
      public ScriptResult(){}

      public ScriptResult(string scriptName, IScriptExecutionResult scriptResult) {
         Name = string.IsNullOrEmpty(scriptName) ? scriptResult.Name : scriptName;
         Id = scriptResult.Id;
         Reason = scriptResult.Reason;
         State = scriptResult.State;
         OutputObjectsFormat = scriptResult.OutputObjectsFormat;
         StarTime = scriptResult.StarTime;
         EndTime = scriptResult.EndTime;
      }

      public string Name { get; set; }

      #region IScriptExecutionResult
      public string Id { get; set; }
      public ScriptState State { get; set; }
      public string Reason { get; set; }
      public OutputObjectsFormat OutputObjectsFormat { get; set; }
      public DateTime? StarTime { get; set; }
      public DateTime? EndTime { get; set; }

      #endregion
   }
}
