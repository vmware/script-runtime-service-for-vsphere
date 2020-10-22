// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes
{
   [DataContract]
   public class ScriptExecutionStreams {
      [DataMember]
      public StreamRecord[] Information { get; set; }
      [DataMember]
      public StreamRecord[] Error { get; set; }
      [DataMember]
      public StreamRecord[] Warning { get; set; }
      [DataMember]
      public StreamRecord[] Debug { get; set; }
      [DataMember]
      public StreamRecord[] Verbose { get; set; }
   }
}
