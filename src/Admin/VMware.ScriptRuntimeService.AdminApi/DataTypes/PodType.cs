// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes {
   [JsonConverter(typeof(StringEnumConverter))]
   [DataContract(Name = "pod_type")]
   [ReadOnly(true)]
   [Flags]
   public enum PodType {
      [EnumMember(Value = "none")]
      None = 0,
      
      [EnumMember(Value = "setup")]
      Setup = 1,

      [EnumMember(Value = "api-gateway")]
      ApiGateway = 2,

      [EnumMember(Value = "admin-api")]
      AdminApi = 4,

      [EnumMember(Value = "all")]
      All = 255
   }
}
