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
   [DataContract(Name = "webconsole_state")]
   [ReadOnly(true)]
   public enum PodType {
      [EnumMember(Value = "setup")]
      Setup = 0,

      [EnumMember(Value = "api-gateway")]
      ApiGateway = 1,

      [EnumMember(Value = "admin-api")]
      AdminApi = 2
   }
}
