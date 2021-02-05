// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Net;

namespace VMware.ScriptRuntimeService.RunspaceProviders.Types
{
   public interface IWebConsoleInfo {
      /// <summary>
      /// Unique WebConsole Id
      /// </summary>
      string Id { get; }

      /// <summary>
      /// Tracks state of the WebConsole Runsapce creation
      /// </summary>
      RunspaceCreationState CreationState { get; }

      /// <summary>
      /// When <see cref="CreationState"/> is an Error, the CreationError holds the occurred exception 
      /// </summary>
      RunspaceProviderException CreationError { get; }
   }
}
