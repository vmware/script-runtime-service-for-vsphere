// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine {
   public class ScriptExecutionResult : IScriptExecutionResult {
      public string Id { get; set; }
      public string Name { get; set; }

      public IOutputObjectCollection OutputObjectCollection { get; set; }

      public ScriptState State { get; set; }

      public OutputObjectsFormat OutputObjectsFormat { get; set; }

      public string Reason { get; set; }

      public IDataStreams Streams { get; set; }

      public DateTime? StarTime { get; set; }

      public DateTime? EndTime { get; set; }
   }
}
