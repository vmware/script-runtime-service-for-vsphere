// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes.ScriptExecutions {
   [DataContract(Name = "script_executions_storage_settings")]
   public class RetentionPolicy {
      [DataMember(Name = "max_number_of_scripts_per_user")]
      public uint? MaxNumberOfScriptsPerUser { get; set; }
      [DataMember(Name = "no_older_than_days")]
      public uint? NoOlderThanDays { get; set; }
   }
}
