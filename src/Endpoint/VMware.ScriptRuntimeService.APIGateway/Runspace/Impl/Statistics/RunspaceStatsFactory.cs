// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics {
   public interface IRunspaceStatsFactory {
      IRunspaceStats Create(
         string runspaceId,
         bool isWebConsole,
         IRunspaceSessionInfoProvider sessionInfoProvider,
         IActiveIdleInfoProvider activeIdleInfoProvider);
   }

   public class RunspaceStatsFactory : IRunspaceStatsFactory {
      public IRunspaceStats Create(
         string runspaceId,
         bool isWebConsole,
         IRunspaceSessionInfoProvider sessionInfoProvider,
         IActiveIdleInfoProvider activeIdleInfoProvider) {

         return new RunspaceStats(
            runspaceId,
            isWebConsole,
            sessionInfoProvider,
            activeIdleInfoProvider);
      }
   }
}
