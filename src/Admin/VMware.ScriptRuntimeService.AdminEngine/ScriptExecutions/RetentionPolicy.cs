// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes {
   public class RetentionPolicy {
      public uint MaxNumberOfScriptsPerUser { get; set; }
      public uint NoOlderThanDays { get; set; }
   }
}
