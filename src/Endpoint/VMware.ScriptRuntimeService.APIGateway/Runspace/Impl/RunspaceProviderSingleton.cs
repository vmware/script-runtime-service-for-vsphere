// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.DockerRunspaceProvider;
using VMware.ScriptRuntimeService.K8sRunspaceProvider;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl
{
   /// <summary>
   /// Provides single instance of <see cref="IRunspaceProvider"/> during the lifetime of the service
   /// It is instantiated once at service startup
   /// </summary>
   public class RunspaceProviderSingleton {
      private static readonly RunspaceProviderSingleton _instance = new RunspaceProviderSingleton();
      private RunspaceProviderSingleton() {}

      public static RunspaceProviderSingleton Instance => _instance;

      internal void CreateRunspaceProvider(ILoggerFactory loggerFactory, RunspaceProviderSettings runspaceProviderSettings) {
         var logger = loggerFactory.CreateLogger(typeof(RunspaceProviderSingleton));
         logger.LogDebug("RunspaceProvider Settings:");
         logger.LogDebug($"  LocalhostDebug: {runspaceProviderSettings.LocalhostDebug}");
         logger.LogDebug($"  DockerApiEndpoint: {runspaceProviderSettings.DockerApiEndpoint}");
         logger.LogDebug($"  K8sClusterEndpoint: {runspaceProviderSettings.K8sClusterEndpoint}");
         logger.LogDebug($"  K8sImagePullSecret: {runspaceProviderSettings.K8sImagePullSecret}");
         logger.LogDebug($"  K8sNamespace: {runspaceProviderSettings.K8sNamespace}");
         logger.LogDebug($"  K8sRunspaceImageName: {runspaceProviderSettings.K8sRunspaceImageName}");
         logger.LogDebug($"  K8sRunspacePort: {runspaceProviderSettings.K8sRunspacePort}");
         logger.LogDebug($"  K8sVerifyRunspaceApiIsAccessibleOnCreate: {runspaceProviderSettings.K8sVerifyRunspaceApiIsAccessibleOnCreate}");
         logger.LogDebug($"  MaxNumberOfRunspaces: {runspaceProviderSettings.MaxNumberOfRunspaces}");
         logger.LogDebug($"  MaxRunspaceActiveTimeMinutes: {runspaceProviderSettings.MaxRunspaceActiveTimeMinutes}");
         logger.LogDebug($"  MaxRunspaceIdleTimeMinutes: {runspaceProviderSettings.MaxRunspaceIdleTimeMinutes}");
         logger.LogDebug($"  RunspaceContainerMemoryLimitMB: {runspaceProviderSettings.RunspaceContainerMemoryLimitMB}");
         logger.LogDebug($"  RunspaceContainerName: {runspaceProviderSettings.RunspaceContainerName}");
         logger.LogDebug($"  RunspaceContainerNetworkName: {runspaceProviderSettings.RunspaceContainerNetworkName}");
         logger.LogDebug($"  RunspaceTrustedCertsConfigMapName: {runspaceProviderSettings.RunspaceTrustedCertsConfigMapName}");

         if (runspaceProviderSettings.LocalhostDebug) {
            logger.LogInformation($"Creating {typeof(DebugRunspaceProvider).Name} runspace provider");
            RunspaceProvider = new MultiTenantRunspaceProvider(loggerFactory, new DebugRunspaceProvider());
         } else if (!string.IsNullOrEmpty(runspaceProviderSettings.DockerApiEndpoint)) {
            logger.LogInformation($"Creating {typeof(DockerRunspaceProvider.DockerRunspaceProvider).Name} runspace provider");
            RunspaceProvider = new MultiTenantRunspaceProvider(
               loggerFactory,
               new DockerRunspaceProvider.DockerRunspaceProvider(
                  new RunspaceContainerCreateSpec() {
                     ImageName = runspaceProviderSettings.RunspaceContainerName,
                     NetworkName = runspaceProviderSettings.RunspaceContainerNetworkName,
                     RunspaceContainerMemoryLimitMB = runspaceProviderSettings.RunspaceContainerMemoryLimitMB
                  },
                  runspaceProviderSettings.DockerApiEndpoint),
               runspaceProviderSettings.MaxNumberOfRunspaces,
               runspaceProviderSettings.MaxRunspaceIdleTimeMinutes,
               runspaceProviderSettings.MaxRunspaceActiveTimeMinutes);
         } else if (!string.IsNullOrEmpty(runspaceProviderSettings.K8sRunspaceImageName)) {
            logger.LogInformation($"Creating {typeof(K8sRunspaceProvider.K8sRunspaceProvider).Name} runspace provider");
            RunspaceProvider = new MultiTenantRunspaceProvider(
               loggerFactory,
               new K8sRunspaceProvider.K8sRunspaceProvider(
                  loggerFactory,
                  runspaceProviderSettings.K8sClusterEndpoint,
                  runspaceProviderSettings.K8sClusterAccessToken,
                  runspaceProviderSettings.K8sNamespace,
                  runspaceProviderSettings.K8sRunspaceImageName,
                  runspaceProviderSettings.K8sRunspacePort,
                  runspaceProviderSettings.K8sImagePullSecret,
                  runspaceProviderSettings.K8sVerifyRunspaceApiIsAccessibleOnCreate,
                  runspaceProviderSettings.RunspaceTrustedCertsConfigMapName
                  ),
               runspaceProviderSettings.MaxNumberOfRunspaces,
               runspaceProviderSettings.MaxRunspaceIdleTimeMinutes,
               runspaceProviderSettings.MaxRunspaceActiveTimeMinutes);
         }
      }

      internal IMultiTenantRunspaceProvider RunspaceProvider { get; private set; }
   }
}
