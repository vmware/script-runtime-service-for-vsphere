// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters {
   public interface IConfigReader {
      T ReadSettings<T>(string settingsName);
      StsSettings ReadServiceStsSettings();
   }
}
