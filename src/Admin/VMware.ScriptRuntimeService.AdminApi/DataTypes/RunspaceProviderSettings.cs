// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes {
   [DataContract(Name = "runspace_provider_settings")]
   public class RunspaceProviderSettings {
      [DataMember(Name = "max_number_of_runspaces")]
      public uint? MaxNumberOfRunspaces { get; set; }
      [DataMember(Name = "max_runspace_idle_time_minutes")]
      public uint? MaxRunspaceIdleTimeMinutes { get; set; }
      [DataMember(Name = "max_runspace_active_time_minutes")]
      public uint? MaxRunspaceActiveTimeMinutes { get; set; }
   }
}
