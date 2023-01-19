// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.AdminApi {
   public interface IEnvironment {
      //
      // Summary:
      //     Retrieves the value of an environment variable from the current process.
      //
      // Parameters:
      //   variable:
      //     The name of the environment variable.
      //
      // Returns:
      //     The value of the environment variable specified by variable, or null if the environment
      //     variable is not found.
      //
      // Exceptions:
      //   T:System.ArgumentNullException:
      //     variable is null.
      //
      //   T:System.Security.SecurityException:
      //     The caller does not have the required permission to perform this operation.
      public string? GetEnvironmentVariable(string variable);
   }
}
