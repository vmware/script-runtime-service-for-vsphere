// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace {
   public interface IWebConsoleData : IWebConsoleInfo {
      /// <summary>
      /// State of the web console resource.
      /// </summary>
      public WebConsoleState State { get; set; }

      /// <summary>
      /// Details about the error that has occured when the runspaces state is 'error'.
      /// </summary>
      public ErrorDetails ErrorDetails { get; set; }

      /// <summary>
      /// Time at which the object was created. String representing time in format ISO 8601.
      /// </summary>
      public DateTime CreationTime { get; set; }
   }
}
