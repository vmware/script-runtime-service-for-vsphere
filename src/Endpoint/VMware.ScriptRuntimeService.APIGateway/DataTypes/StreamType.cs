// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// StreamType defines script execution data stream types. </summary>
   [JsonConverter(typeof(StringEnumConverter))]
   [DataContract(Name = "stream_type")]
   public enum StreamType {
      [DataMember(Name = "information")]
      information,

      [DataMember(Name = "error")]
      error,

      [DataMember(Name = "warning")]
      warning,

      [DataMember(Name = "debug")]
      debug,

      [DataMember(Name = "verbose")]
      verbose
   }
}
