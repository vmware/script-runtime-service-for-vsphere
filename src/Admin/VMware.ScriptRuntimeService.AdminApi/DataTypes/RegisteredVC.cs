// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes {
   [DataContract(Name = "registered_vc")]
   public class RegisteredVC {
      [DataMember(Name = "vc_address")]
      public string VCAddress { get; set; }
   }
}
