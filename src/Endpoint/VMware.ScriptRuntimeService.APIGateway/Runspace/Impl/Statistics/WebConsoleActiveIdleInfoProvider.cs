// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics
{
   public class WebConsoleActiveIdleInfoProvider : IActiveIdleInfoProvider  {
      public WebConsoleActiveIdleInfoProvider(IWebConsoleInfo webConsoleInfo) {
         RunspaceId = webConsoleInfo.Id;
      }
      public string RunspaceId { get; }

      public bool IsActive => true;

      public int ActiveTimeSeconds => 0;

      public int IdleTimeSeconds => 0;

      public void Refresh() {         
      }
   }
}
