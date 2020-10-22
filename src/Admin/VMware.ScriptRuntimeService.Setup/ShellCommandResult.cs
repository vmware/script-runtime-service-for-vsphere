// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.Setup {
   public class ShellCommandResult {
      public int ExitCode { get; set; }
      public string OutputStream { get; set; }
      public string ErrorStream { get; set; }
   }
}
