// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes {
   public interface IScriptExecutionReader {
      INamedScriptExecution ReadScriptExecution();
      IScriptExecutionOutputObjects ReadScriptExecutionOutput();
      ScriptExecutionDataStreams ReadScriptExecutionDataStreams();
   }
}
