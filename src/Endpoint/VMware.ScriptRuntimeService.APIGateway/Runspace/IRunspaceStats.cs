// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace {
   public interface IRunspaceStats {
      void Refresh();

      string RunspaceId { get; }
      bool HasActiveSession { get; }
      bool IsActive { get; }
      int ActiveTimeSeconds { get; }
      int IdleTimeSeconds { get; }
   }
}
