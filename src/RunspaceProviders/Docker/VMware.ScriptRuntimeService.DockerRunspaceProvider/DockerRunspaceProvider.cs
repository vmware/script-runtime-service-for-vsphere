// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using VMware.ScriptRuntimeService.Docker.Bindings.Api;
using VMware.ScriptRuntimeService.Docker.Bindings.Client;
using VMware.ScriptRuntimeService.Docker.Bindings.Model;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using Container = System.ComponentModel.Container;

[assembly: InternalsVisibleTo("VMware.ScriptRuntimeService.DockerRunspaceProvider.Tests")]
namespace VMware.ScriptRuntimeService.DockerRunspaceProvider
{
   /// <summary>
   /// Manages Docker Runspace Container lifecycle.
   /// Public interface is <see cref="IRunspaceProvider"/>.
   /// Runspace Docker Image Name and Configuration are provided on instantiation.
   /// </summary>
   public class DockerRunspaceProvider : IRunspaceProvider
   {
      #region Private fields
      private readonly RunspaceContainerCreateSpec _runspaceContainerCreateSpec;
      private readonly IContainerApi _containerApi;
      private readonly bool _testConnectionToContainerOnCreate;
      #endregion

      #region Contsructors
      public DockerRunspaceProvider(
         RunspaceContainerCreateSpec runspaceContainerCreateSpec,
         string dockerEngineEndpointBasePath) :
         this(runspaceContainerCreateSpec, dockerEngineEndpointBasePath, null, true)
      { }

      internal DockerRunspaceProvider(
         RunspaceContainerCreateSpec runspaceContainerCreateSpec,
         string dockerEngineEndpointBasePath,
         IContainerApi containerApi,
         bool testConnectionToContainerOnCreate) {

         _containerApi = containerApi ?? CreateDefaultContainerApi(dockerEngineEndpointBasePath);
         _runspaceContainerCreateSpec = runspaceContainerCreateSpec;
         _testConnectionToContainerOnCreate = testConnectionToContainerOnCreate;
      }

      private IContainerApi CreateDefaultContainerApi(string dockerEngineEndpointBasePath) {

         if (string.IsNullOrEmpty(dockerEngineEndpointBasePath) ||
             !Uri.IsWellFormedUriString(dockerEngineEndpointBasePath, UriKind.Absolute)) {

            throw new ArgumentException(
               string.Format(
                  Resources.Resources.DockerRunspaceProvider_DockerRunspaceProvider_InvalidDockerEndpointUrl, dockerEngineEndpointBasePath),
               nameof(dockerEngineEndpointBasePath));
         }

         return new ContainerApi(dockerEngineEndpointBasePath);
      }
      #endregion

      #region Private helpers
      private static void EnsureRunspaceEndpointIsAccessible(IRunspaceInfo runspaceInfo) {
         if (runspaceInfo?.CreationState != RunspaceCreationState.Error) {
            bool ready = false;
            var retryNum = 0;
            var maxRetryCount = 20;
            var retryDelayMs = 500;
            Exception pingExc = null;
            while (retryNum < maxRetryCount) {
               using (TcpClient tcpClient = new TcpClient()) {
                  try {
                     tcpClient.Connect(runspaceInfo.Endpoint.Address, runspaceInfo.Endpoint.Port);
                     ready = true;
                     break;
                  } catch (Exception exc) {
                     pingExc = exc;
                  }
               }

               Thread.Sleep(retryDelayMs);
               retryNum++;
            }

            if (!ready) {
               throw new RunspaceProviderException(Resources.Resources.DockerRunspaceProvider_Create_NewContainerIsNotAccessible, pingExc);
            }
         }         
      }
      #endregion

      #region IRunspaceProvider interface
      /// <summary>
      /// Instantiates, Starts, and Gets Container details through Docker API
      /// </summary>
      /// <returns>Instance of <see cref="IRunspaceInfo"/> for the started Runspace Container.</returns>
      public IRunspaceInfo StartCreate() {

         IRunspaceInfo result = null;
         const long CPUsCount = 1;
         const long BytesInMegaByte = 1048576;
         var containerConfig = new ContainerConfig(
               image: _runspaceContainerCreateSpec.ImageName,
               hostConfig: new HostConfig(
                  networkMode: _runspaceContainerCreateSpec.NetworkName,
                  cpuCount: CPUsCount,
                  restartPolicy: new RestartPolicy(RestartPolicy.NameEnum.Empty),
                  memory: BytesInMegaByte * _runspaceContainerCreateSpec.RunspaceContainerMemoryLimitMB));

         ContainerCreateResponse runspaceContainerInstance;
         try {
            // Create Runspace Container Instance
            runspaceContainerInstance = _containerApi.ContainerCreate(containerConfig);

            // Start Runspace Container
            _containerApi.ContainerStart(runspaceContainerInstance.Id);

            result = Get(runspaceContainerInstance.Id);
         } catch (ApiException dockerApiException) {
            result = DockerRunspaceInfo.FromRunspaceProviderError(
               new RunspaceProviderException(
                  Resources.Resources.DockerRunspaceProvider_Create_StartContainerDockerAPIFail, 
                  dockerApiException));
         }             

         try {
            if (_testConnectionToContainerOnCreate) {
               // Ensure Container is accessible over the network after creation
               EnsureRunspaceEndpointIsAccessible(result);
            }            
         } catch (RunspaceProviderException exception) {
            // Kill the container that is not accessible, otherwise it will leak
            try {
               if (result.Id != null) {
                  Kill(result.Id);
               }               
            } catch(RunspaceProviderException) {}

            result = DockerRunspaceInfo.FromRunspaceProviderError(exception);
         }         

         return result;
      }

      public IRunspaceInfo WaitCreateCompletion(IRunspaceInfo runspace) {
         return runspace;
      }

      public IRunspaceInfo Get(string id) {
         ContainerInspectResponse containerInspectResponse;

         try {
            // Get Container Details
            containerInspectResponse = _containerApi.ContainerInspect(id);
         } catch (ApiException dockerApiException) {
            throw new RunspaceProviderException(
               Resources.Resources.DockerRunspaceProvider_Get_ContainerDockerAPIFail,
               dockerApiException);
         }

         // Populate Result from ContainerInspectResponse
         return containerInspectResponse.State.Status == ContainerInspectResponseState.StatusEnum.Running ?
            DockerRunspaceInfo.FromContainerInspectResponse(
               containerInspectResponse,
               _runspaceContainerCreateSpec.NetworkName) :
            null;
      }

      public void Kill(string id)
      {
         try {
            _containerApi.ContainerKill(id);
            _containerApi.ContainerDelete(id, force:true);
         } catch (ApiException dockerApiException) {
            throw new RunspaceProviderException(
               Resources.Resources.DockerRunspaceProvider_Kill_KillContainerDockerAPIFail, 
               dockerApiException);
         }
      }

      public IRunspaceInfo[] List() {
         List<IRunspaceInfo> result = new List<IRunspaceInfo>();

         try {
            var runningRunspaceContainerList = _containerApi.ContainerList();
            foreach (var runspaceContainer in runningRunspaceContainerList) {
               if (runspaceContainer != null &&
                   runspaceContainer.Image == _runspaceContainerCreateSpec.ImageName &&
                   runspaceContainer.State.ToLower() == "running") {

                  var containerInspectResponse = _containerApi.ContainerInspect(runspaceContainer.Id);

                  result.Add(DockerRunspaceInfo.FromContainerInspectResponse(
                     containerInspectResponse,
                     _runspaceContainerCreateSpec.NetworkName));
               }
            }
         } catch (ApiException dockerApiException) {
            throw new RunspaceProviderException(
               Resources.Resources.DockerRunspaceProvider_Get_ContainerDockerAPIFail,
               dockerApiException);
         }

         // Populate Result from ContainerInspectResponse
         return result.ToArray();
      }

      public void UpdateConfiguration(IRunspaceProviderSettings runspaceProviderSettings) {
         // Not supported
      }
      #endregion
   }
}
