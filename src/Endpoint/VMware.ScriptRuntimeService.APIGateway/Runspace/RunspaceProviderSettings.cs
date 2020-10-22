// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace
{
   public class RunspaceProviderSettings {
      // Default values
      private const int RunspaceContainerMemoryLimitMBDefault = 512;
      private const int MaxNumberOfRunspacesDefault = 4;
      private const int MaxRunspaceIdleTimeMinutesDefault = 10;
      private const int MaxRunspaceActiveTimeMinutesDefault = 60;

      private int _runspaceContainerMemoryLimitMB = RunspaceContainerMemoryLimitMBDefault;
      private int _maxNumberOfRunspaces = MaxNumberOfRunspacesDefault;
      private int _maxRunspaceIdleTimeMinutes = MaxRunspaceIdleTimeMinutesDefault;
      private int _maxRunspaceActiveTimeMinutes = MaxRunspaceActiveTimeMinutesDefault;

      public bool IsDefault() {
         return            
            RunspaceContainerMemoryLimitMB == RunspaceContainerMemoryLimitMBDefault &&
            MaxNumberOfRunspaces == MaxNumberOfRunspacesDefault &&
            MaxRunspaceIdleTimeMinutes == MaxRunspaceIdleTimeMinutesDefault &&
            MaxRunspaceActiveTimeMinutes == MaxRunspaceActiveTimeMinutesDefault &&
            DockerApiEndpoint == null &&
            K8sClusterEndpoint == null &&
            K8sClusterAccessToken == null &&
            K8sRunspaceImageName == null &&
            K8sNamespace == null &&
            K8sImagePullSecret == null &&
            RunspaceContainerName == null &&
            RunspaceContainerNetworkName == null &&
            RunspaceTrustedCertsConfigMapName == null;
      }

      public bool LocalhostDebug { get; set; }
      public string DockerApiEndpoint { get; set; }
      public string K8sClusterEndpoint { get; set; }
      public string K8sClusterAccessToken { get; set; }
      public string K8sRunspaceImageName { get; set; }
      public int K8sRunspacePort { get; set; }
      public string K8sNamespace { get; set; }
      public string K8sImagePullSecret { get; set; }
      public bool K8sVerifyRunspaceApiIsAccessibleOnCreate{ get; set; }
      public string RunspaceContainerName { get; set; }
      public string RunspaceContainerNetworkName { get; set; }
      public string RunspaceTrustedCertsConfigMapName { get; set; }
      public int RunspaceContainerMemoryLimitMB {
         get { return _runspaceContainerMemoryLimitMB; }
         set { _runspaceContainerMemoryLimitMB = value <= 0 ? RunspaceContainerMemoryLimitMBDefault : value; }
      }
      public int MaxNumberOfRunspaces {
         get { return _maxNumberOfRunspaces; }
         set { _maxNumberOfRunspaces = value <= 0 ? MaxNumberOfRunspacesDefault : value; }
      }

      public int MaxRunspaceIdleTimeMinutes {
         get { return _maxRunspaceIdleTimeMinutes; }
         set { _maxRunspaceIdleTimeMinutes = value <= 0 ? MaxRunspaceIdleTimeMinutesDefault : value; }
      }

      public int MaxRunspaceActiveTimeMinutes {
         get { return _maxRunspaceActiveTimeMinutes; }
         set { _maxRunspaceActiveTimeMinutes = value <= 0 ? MaxRunspaceActiveTimeMinutesDefault : value; }
      }
   }
}
