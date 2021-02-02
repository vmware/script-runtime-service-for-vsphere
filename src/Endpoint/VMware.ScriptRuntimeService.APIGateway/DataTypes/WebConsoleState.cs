// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// WebConsoleState defines possible web console states.
   /// </summary>
   [JsonConverter(typeof(StringEnumConverter))]
   [DataContract(Name = "webconsole_state")]
   [ReadOnly(true)]
   public enum WebConsoleState {
      /// <summary>
      /// WebConsole is ready available to be opened.
      /// </summary>
      [EnumMember(Value = "available")]
      Available,

      /// <summary>
      /// An error occurred during the web console creation.
      /// </summary>
      [EnumMember(Value = "error")]
      Error
   }
}
