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
   /// RunspaceState defines possible runspace states.
   /// </summary>
   [JsonConverter(typeof(StringEnumConverter))]
   [DataContract(Name = "runspace_state")]
   [ReadOnly(true)]
   public enum RunspaceState {
      /// <summary>
      /// Runspace is ready to run scripts.
      /// </summary>
      [EnumMember(Value = "ready")]
      Ready,

      /// <summary>
      /// Runspace is running a script.
      /// </summary>
      [EnumMember(Value = "active")]
      Active,

      /// <summary>
      /// Runspace is being created.
      /// </summary>
      [EnumMember(Value = "creating")]
      Creating,

      /// <summary>
      /// An error occurred during runspace creation.
      /// </summary>
      [EnumMember(Value = "error")]
      Error
   }
}
