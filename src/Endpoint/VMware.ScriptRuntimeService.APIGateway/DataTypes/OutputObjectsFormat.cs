// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {

   [JsonConverter(typeof(StringEnumConverter))]
   [DataContract(Name = "output_objects_format")]
   public enum OutputObjectsFormat {
      [EnumMember(Value = "text")]
      Text,
      [EnumMember(Value = "json")]
      Json
   }
}
