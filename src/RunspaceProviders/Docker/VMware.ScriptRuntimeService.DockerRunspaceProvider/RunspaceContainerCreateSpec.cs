// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

namespace VMware.ScriptRuntimeService.DockerRunspaceProvider {
   /// <summary>
   /// Holds information for the docker runspace container
   /// </summary>
   public class RunspaceContainerCreateSpec {
      /// <summary>
      /// Name of the docker image that represents the runspace
      /// </summary>
      public string ImageName { get; set; }

      /// <summary>
      /// Docker network to which runspace container will be created
      /// </summary>
      public string NetworkName { get; set; }

      /// <summary>
      /// Runspace container momory hard limit in MegaBytes
      /// </summary>
      public int RunspaceContainerMemoryLimitMB { get; set; }
   }
}
