// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine {
   public class ScriptParameter : IScriptParameter {
      public string Name { get; set; }
      public object Value { get; set; }
      public string Script { get; set; }
   }
}
