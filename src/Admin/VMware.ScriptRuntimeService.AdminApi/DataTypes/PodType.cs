// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes {
   [DataContract(Name = "pod_type")]
   [Flags]
   public enum PodType {
      Setup = 0,
      ApiGateway = 1,
      AdminApi = 2
   }
}
