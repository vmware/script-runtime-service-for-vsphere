// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes
{
   /// <summary>
   /// Script Execution object allows you to run a script in a runspace.
   /// The API allows you to create, cancel, and retrieve script executions. 
   /// </summary>
   [DataContract(Name = "script_execution")]
   public class ScriptExecution : IScriptExecutionRequest {
      public ScriptExecution() { }

      public ScriptExecution(INamedScriptExecution scriptResult) {
         Id = scriptResult.Id;
         ExecutionState = ScriptStateConvert.From(scriptResult.State);
         Reason = scriptResult.Reason;
         Output_Ojects_Format = Enum.Parse<OutputObjectsFormat>(scriptResult.OutputObjectsFormat.ToString());
         Name = scriptResult.Name;
         StarTime = scriptResult.StarTime;
         EndTime = scriptResult.EndTime;
      }

      /// <summary>
      /// Unique identifier for the object.
      /// </summary>
      [DataMember(Name = "id")]
      public string Id { get; }

      /// <summary>
      /// Unique identifier of the runspace where script execution is performed.
      /// </summary>
      [Required]
      [DataMember(Name = "runspace_id", IsRequired = true)]
      public string RunspaceId { get; set; }

      /// <summary>
      /// Name of the script execution. It is optional to give a name of the script execution on create request. If name was not specified on script execution creation the field has null value.
      /// </summary>
      [DataMember(Name = "name")]
      public string Name { get; set; }

      /// <summary>
      /// Content of the script.
      /// </summary>
      [Required]
      [DataMember(Name = "script", IsRequired = true)]
      public string Script { get; set; }

      /// <summary>
      /// List of arguments that will be passed to the script.
      /// If script content defines parameters argument can be provided.
      /// The parameter names defined in the script content should match the names specified in this list.
      /// </summary>
      [DataMember(Name = "script_parameters")]
      public ScriptParameter[] ScriptParameters { get; set; }

      /// <summary>
      /// Output object format specifies the desired output format of he objects that will be produced by the script execution.
      /// If text output format is requested the output is list of strings representing the output formatted as text.
      /// If json output format is requested the output is list of json formatted objects with type information.
      /// </summary>
      [DataMember(Name = "output_objects_format")]
      public OutputObjectsFormat Output_Ojects_Format { get; set; }

      public ScriptRuntimeService.Runspace.Types.OutputObjectsFormat OutputObjectsFormat {
         get => Enum.Parse<VMware.ScriptRuntimeService.Runspace.Types.OutputObjectsFormat>(Output_Ojects_Format.ToString());
         set => Output_Ojects_Format = Enum.Parse<OutputObjectsFormat>(value.ToString());
      }

      IScriptParameter[] IScriptExecutionRequest.Parameters =>
         ScriptParameters != null ?
            Array.ConvertAll(ScriptParameters, item => item as IScriptParameter) :
            null;

      /// <summary>
      /// State of the script execution resource. Script execution is asynchronous. When created the script
      /// execution is started. The state fields represent the state of the script execution in the moment of retrieval.
      /// </summary>
      [JsonConverter(typeof(StringEnumConverter))]
      [DataMember(Name = "state")]
      public ScriptExecutionState ExecutionState { get; }

      /// <summary>
      /// Reason for the current script execution state. In most of the cases reason field will be empty. In case
      /// of an error or cancellation reason will contain information about the reason that caused script execution to
      /// become in this state.
      /// </summary>
      [DataMember(Name = "reason")]
      public string Reason { get; }

      /// <summary>
      /// Time at which the script execution was started. String representing time in format ISO 8601.
      /// </summary>
      [DataMember(Name = "start_time", IsRequired = false)]
      [JsonConverter(typeof(IsoDateTimeConverter))]
      public DateTime? StarTime { get; }

      /// <summary>
      /// Time at which the script execution was finished. String representing time in format ISO 8601.
      /// </summary>
      [DataMember(Name = "end_time", IsRequired = false)]
      [JsonConverter(typeof(IsoDateTimeConverter))]
      public DateTime? EndTime { get; }
   }
}
