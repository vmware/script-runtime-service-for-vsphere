// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.ScriptRuntimeService.Runspace.Types {
   public interface IScriptExecution {
      string Id { get; }

      ScriptState State { get; }

      string Reason { get; }

      OutputObjectsFormat OutputObjectsFormat { get; }

      DateTime? StarTime { get; }

      DateTime? EndTime { get; }
   }
}
