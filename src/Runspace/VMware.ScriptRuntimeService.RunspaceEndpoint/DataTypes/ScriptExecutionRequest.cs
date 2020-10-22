// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes
{
   [DataContract]
   public class ScriptExecutionRequest {
      [DataMember(Name = "script", IsRequired = true)]
      public string Script { get; set; }

      [DataMember(Name = "output_objects_format", IsRequired = true)]
      public OutputObjectsFormat OutputObjectsFormat { get; set; }

      [DataMember(Name = "name", IsRequired = false)]
      public string Name { get; set; }

      [DataMember(Name = "parameters", IsRequired = false)]
      public ScriptParameter[] Parameters { get; set; }
   }
}
