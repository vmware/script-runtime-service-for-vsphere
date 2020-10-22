// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes {
   public class ScriptExecutionOutput : IScriptExecutionOutputObjects {
      public ScriptExecutionOutput() { }

      public ScriptExecutionOutput(IScriptExecutionResult scriptExecutionResult) {
         switch (scriptExecutionResult?.OutputObjectsFormat) {
            case OutputObjectsFormat.Json:
               OutputObjects = scriptExecutionResult.OutputObjectCollection.SerializedObjects;
               break;
            case OutputObjectsFormat.Text:
               OutputObjects = new string[]
                  {scriptExecutionResult.OutputObjectCollection.FormattedTextPresentation};
               break;
            default:
               break;
         }
         
      }
      public string[] OutputObjects { get; set;  }
   }
}
