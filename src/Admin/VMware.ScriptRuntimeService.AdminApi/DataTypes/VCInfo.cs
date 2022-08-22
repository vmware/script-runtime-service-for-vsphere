// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes {
   [DataContract(Name = "vc_info")]
   public class VCInfo {
      [DataMember(Name = "address")]
      public string Address { get; set; }
      [DataMember(Name = "thumbprint")]
      public string Thumbprint { get; set; }
      [DataMember(Name = "username")]
      public string UserName { get; set; }
      [DataMember(Name = "password")]
      public string Password { get; set; }
   }
}
