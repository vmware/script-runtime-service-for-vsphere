// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminApi {
   public class EnvironmentWrapper : IEnvironment {
      public string GetEnvironmentVariable(string variable) {
         return System.Environment.GetEnvironmentVariable(variable);
      }
   }
}
