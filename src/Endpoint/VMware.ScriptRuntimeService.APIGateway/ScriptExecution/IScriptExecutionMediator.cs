﻿// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecution
{
   public interface IScriptExecutionMediator
   {
      Task<INamedScriptExecution> StartScriptExecution(string userId, IRunspaceInfo runspace, IScriptExecutionRequest scriptExecutionRequest);
      
      INamedScriptExecution GetScriptExecution(string userId, string scriptId);

      INamedScriptExecution[] ListScriptExecutions(string userId);

      IScriptExecutionOutputObjects GetScriptExecutionOutput(string userId, string scriptId);

      IScriptExecutionDataStreams GetScriptExecutionDataStreams(string userId, string scriptId);

      void CancelScriptExecution(string userId, string scriptId);
      void UpdateConfiguration(ScriptExecutionStorageSettings settings);
   }
}
