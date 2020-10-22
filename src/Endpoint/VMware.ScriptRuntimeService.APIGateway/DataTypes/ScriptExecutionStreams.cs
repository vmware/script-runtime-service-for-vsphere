// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   [DataContract(Name = "script_execution_streams")]
   public class ScriptExecutionStreams {
      [DataMember(Name = "information")]
      public StreamRecord[] Information { get; set; }
      [DataMember(Name = "error")]
      public StreamRecord[] Error { get; set; }
      [DataMember(Name = "warning")]
      public StreamRecord[] Warning { get; set; }
      [DataMember(Name = "debug")]
      public StreamRecord[] Debug { get; set; }
      [DataMember(Name = "verbose")]
      public StreamRecord[] Verbose { get; set; }
   }
}
