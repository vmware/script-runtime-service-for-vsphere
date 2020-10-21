// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes {
   [JsonConverter(typeof(StringEnumConverter))]
   [DataContract(Name = "output_objects_format")]
   public enum OutputObjectsFormat {
      [EnumMember(Value = "text")] Text,

      [EnumMember(Value = "json")] Json
   }

   static class OutputObjectsFormatEnumConverter {
      public static Runspace.Types.OutputObjectsFormat ToRunspaceTypes(OutputObjectsFormat value) {
         return Enum.Parse<Runspace.Types.OutputObjectsFormat>(value.ToString());
      }
   }

}
