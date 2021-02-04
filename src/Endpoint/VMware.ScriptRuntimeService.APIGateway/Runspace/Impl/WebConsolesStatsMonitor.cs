// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl
{
   public class WebConsolesStatsMonitor {
      private int _numberRunspaces;
      private int _maxNumberOfRunspaces;
      private int _maxNumberOfWebConsolesPerUser;

      public WebConsolesStatsMonitor(
         int maxNumberOfRunspaces,
         int maxNumberOfWebConsolesPerUser)  {
         _maxNumberOfRunspaces = maxNumberOfRunspaces;
         _maxNumberOfWebConsolesPerUser = maxNumberOfWebConsolesPerUser;
      }

      public bool IsCreateNewWebConsoleAllowed() {
         bool result = false;

         lock (this) {
            result = _numberRunspaces < _maxNumberOfRunspaces;
         }

         return result;
      }

      public void Register(IWebConsoleInfo webConsoleInfo, string sessionId) {
         _numberRunspaces++;
         // TODO: Implement
      }

      public void Unregister(string runspaceId) {
         _numberRunspaces--;
         // TODO: Implement
      }

      public string[] GetRegisteredWebConsoles() {
         // TODO: Implement
         return new string[] { };
      }

      public string[] EvaluateWebConsolesToRemove() {
         // TODO: Implement
         return new string[] { };
      }

      public void UpdateConfiguration(
         int maxNumberOfRunspaces,
         int maxNumberOfWebConsolesPerUser) {

         _maxNumberOfRunspaces = maxNumberOfRunspaces;
         _maxNumberOfWebConsolesPerUser = maxNumberOfWebConsolesPerUser;         
      }
   }
}
