// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecution {
   public interface IScriptExecutionOutputObjects {
      string[] OutputObjects { get; }
   }
}
