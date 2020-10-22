// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes {
   [DataContract]
   public class ScriptParameter {
      [Required]
      [DataMember(Name = "name", IsRequired = true)]
      public string Name { get; set; }

      [DataMember(Name = "value", IsRequired = false)]
      public object Value { get; set; }

      [DataMember(Name = "script", IsRequired = false)]
      public string Script { get; set; }
   }
}
