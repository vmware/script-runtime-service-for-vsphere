// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes {

   [DataContract]
   public class ScriptExecutionResponse {

      [DataMember]
      public string Id { get; set; }

      [DataMember]
      public string Name { get; set; }

      [DataMember]
      public string State { get; set; }

      [DataMember]
      public string Reason { get; set; }

      [DataMember]
      public OutputObjectCollection OutputObjectCollection { get; set; }

      [DataMember]
      public ScriptExecutionStreams DataStreams { get; set; }

      [DataMember(Name = "output_objects_format", IsRequired = true)]
      public OutputObjectsFormat OutputObjectsFormat { get; set; }

      [DataMember(Name = "start_time", IsRequired = false)]
      public DateTime? StarTime { get; set; }

      [DataMember(Name = "end_time", IsRequired = false)]
      public DateTime? EndTime { get; set; }
   }
}
