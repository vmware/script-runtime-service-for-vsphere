// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   public interface ISetupFlow {
      /// <summary>
      /// Runs actions to complete specific setup flow implementation
      /// </summary>
      /// <param name="userInput">User input parameters</param>
      /// <returns>Flow exit code. Can be used as process exit code.</returns>
      int Run(UserInput userInput);
   }
}
