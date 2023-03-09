// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.Runspace.Types
{
   public interface IScriptExecutionRequest
   {
      string Name { get; }

      string Script { get; }

      OutputObjectsFormat OutputObjectsFormat { get; }

      IScriptParameter[] Parameters { get; }

      bool IsSystem { get; }
   }
}
