// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.Runspace.Types {
   public interface INamedScriptExecution : IScriptExecution {
      string Name { get; }
   }
}
