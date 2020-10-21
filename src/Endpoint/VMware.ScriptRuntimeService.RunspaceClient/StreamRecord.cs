// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.RunspaceClient {
   public class StreamRecord : IStreamRecord {
      public DateTime Time { get; set; }
      public string Message { get; set; }
   }
}
