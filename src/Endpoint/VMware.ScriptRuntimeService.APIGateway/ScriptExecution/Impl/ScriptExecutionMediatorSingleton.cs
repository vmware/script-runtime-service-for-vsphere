// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl
{
   public class ScriptExecutionMediatorSingleton {
      #region Singleton
      private static readonly ScriptExecutionMediatorSingleton _instance = new ScriptExecutionMediatorSingleton();
      private ScriptExecutionMediatorSingleton() { }

      public void CreateScriptExecutionStorage(ILoggerFactory loggerFactory, ScriptExecutionStorageSettings storageSettings) {
         ScriptExecutionMediator = new PersistentScriptExecutionMediator(loggerFactory, storageSettings);
      }

      public static ScriptExecutionMediatorSingleton Instance => _instance;
      #endregion

      public IScriptExecutionMediator ScriptExecutionMediator { get; private set; }
   }
}
