// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace {
   public interface IRunspacesStatsMonitor {
      /// <summary>
      /// Takes decision whether new runspace is good to be created
      /// </summary>
      /// <returns>True if new runspace is allowed, otherwise false</returns>
      bool IsCreateNewRunspaceAllowed();

      /// <summary>
      /// Adds runspace for monitoring
      /// </summary>
      /// <param name="runspaceInfo"></param>
      /// <param name="sessionId"></param>
      void Register(IRunspaceInfo runspaceInfo, string sessionId);

      /// <summary>
      /// Removes runspace from monitoring
      /// </summary>
      /// <param name="runspaceId"></param>
      void Unregister(string runspaceId);

      /// <summary>
      /// Takes decision which of the registered runspaces are good to be removed
      /// </summary>
      /// <returns>Array of runspace Ids</returns>
      string[] EvaluateRunspacesToRemove();

      /// <summary>
      /// Gets registered runspace Ids registered for monitoring
      /// </summary>
      /// <returns>Array of runspace Ids</returns>
      string[] GetRegisteredRunspaces();
   }
}
