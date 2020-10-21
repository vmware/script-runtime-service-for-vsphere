// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes {
   [DataContract]
   public class OutputObjectCollection {
      [DataMember]
      public string FormattedTextPresentation { get; set; }
      [DataMember]
      public string[] SerializedObjects { get; set; }
   }
}
