// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model;

namespace VMware.ScriptRuntimeService.RunspaceClient
{
   public class ScriptResult : IScriptExecutionResult {
      public ScriptResult(ScriptExecutionResponse scriptExecutionResponse) {
         Id = scriptExecutionResponse.Id;
         Name = scriptExecutionResponse.Name;
         State = Enum.Parse<ScriptState>(scriptExecutionResponse.State, true);
         Reason = scriptExecutionResponse.Reason;
         StarTime = scriptExecutionResponse.StartTime;
         EndTime = scriptExecutionResponse.EndTime;
         OutputObjectsFormat = Enum.Parse<OutputObjectsFormat>(scriptExecutionResponse.OutputObjectsFormat.ToString());
         OutputObjectCollection = new OutputObjectCollection {
            FormattedTextPresentation = scriptExecutionResponse.OutputObjectCollection?.FormattedTextPresentation,
            SerializedObjects = scriptExecutionResponse.OutputObjectCollection?.SerializedObjects?.ToArray()
         };
         Streams = new DataStreams {
            Debug = DataStreams.FromModelRecords(scriptExecutionResponse.DataStreams?.Debug?.ToArray()),
            Error = DataStreams.FromModelRecords(scriptExecutionResponse.DataStreams?.Error?.ToArray()),
            Information = DataStreams.FromModelRecords(scriptExecutionResponse.DataStreams?.Information?.ToArray()),
            Verbose = DataStreams.FromModelRecords(scriptExecutionResponse.DataStreams?.Verbose?.ToArray()),
            Warning = DataStreams.FromModelRecords(scriptExecutionResponse.DataStreams?.Warning?.ToArray())
         };
      }
      public string Id { get; set;  }
      public ScriptState State { get; set; }
      public string Reason{ get; set; }
      public IOutputObjectCollection OutputObjectCollection { get; set; }
      public OutputObjectsFormat OutputObjectsFormat { get; set; }
      public DateTime? StarTime { get; }
      public DateTime? EndTime { get; }
      public IDataStreams Streams { get; set; }
      public string Name { get; }
   }
}
