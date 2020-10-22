// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Net;

namespace VMware.ScriptRuntimeService.RunspaceProviders.Types
{
   public interface IRunspaceInfo {
      /// <summary>
      /// Unique Runspace Id
      /// </summary>
      string Id { get; }

      /// <summary>
      /// Runspace Service Endpoint
      /// </summary>
      IPEndPoint Endpoint { get; }

      /// <summary>
      /// Tracks state during asynchronous Runsapce creation
      /// </summary>
      RunspaceCreationState CreationState { get; }

      /// <summary>
      /// When <see cref="CreationState"/> is Error holds exception produced by creation process
      /// </summary>
      RunspaceProviderException CreationError { get; }
   }
}
