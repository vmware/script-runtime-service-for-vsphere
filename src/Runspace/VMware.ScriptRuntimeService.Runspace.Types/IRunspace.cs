// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.Runspace.Types
{
   public interface IRunspace {
      Task<IScriptExecutionResult> StartScript(string content, OutputObjectsFormat outputObjectsFormat, IScriptParameter[] parameters = null, string name = null);
      IScriptExecutionResult GetScript(string id);
      IScriptExecutionResult GetLastScript();
      void CancelScript(string id);
   }
}
