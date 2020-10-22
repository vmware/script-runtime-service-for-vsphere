// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics {
   public interface IActiveIdleInfoProvider : IRunspaceStatDataProvider {
      string RunspaceId { get; }
      bool IsActive { get; }
      int ActiveTimeSeconds { get; }
      int IdleTimeSeconds { get; }
   }

   public class ActiveIdleInfoProvider : IActiveIdleInfoProvider {
      private IRunspaceInfo _runspaceInfo;
      private DateTime _creationTime;
      private IRunspaceClientFactory _runspaceClientFactory;
      public ActiveIdleInfoProvider(IRunspaceInfo runspaceInfo, IRunspaceClientFactory runspaceClientFactory = null) {
         _runspaceInfo = runspaceInfo;
         _creationTime = DateTime.Now;
         _runspaceClientFactory = runspaceClientFactory;
         if (_runspaceClientFactory == null) {
            _runspaceClientFactory = new RunspaceClientFactory();
         }
      }

      public string RunspaceId => _runspaceInfo.Id;
      public bool IsActive { get; private set; }
      public int ActiveTimeSeconds { get; private set; }
      public int IdleTimeSeconds { get; private set; }

      // NB: Potential issue with the below implementation
      // would be when Runspace and This provider runs on
      // different machines and clocks of the machines are different
      public void Refresh() {
         if (_runspaceInfo.Endpoint != null) {
            var runspaceClient = _runspaceClientFactory.Create(_runspaceInfo.Endpoint);
            try {
               var lastScript = runspaceClient.GetLastScript();
               if (lastScript == null) {
                  IsActive = false;
                  ActiveTimeSeconds = 0;
                  IdleTimeSeconds = Convert.ToInt32((DateTime.Now - _creationTime).TotalSeconds);
               } else {
                  if (lastScript.State != ScriptState.Running) {
                     IsActive = false;
                     IdleTimeSeconds = Convert.ToInt32((DateTime.Now - lastScript.EndTime.Value).TotalSeconds);
                     ActiveTimeSeconds = 0;
                  }

                  if (lastScript.State == ScriptState.Running) {
                     IsActive = true;
                     ActiveTimeSeconds = Convert.ToInt32((DateTime.Now - lastScript.StarTime.Value).TotalSeconds);
                     IdleTimeSeconds = 0;
                  }
               }
            } catch (RunspaceEndpointException) {
               IsActive = false;
               ActiveTimeSeconds = 0;
               IdleTimeSeconds = Convert.ToInt32((DateTime.Now - _creationTime).TotalSeconds);
            }
         } else {
            // Considerrunspace is with creation error and stays idle
            IsActive = false;
            ActiveTimeSeconds = 0;
            IdleTimeSeconds = Convert.ToInt32((DateTime.Now - _creationTime).TotalSeconds);
         }
        
      }
   }
}
