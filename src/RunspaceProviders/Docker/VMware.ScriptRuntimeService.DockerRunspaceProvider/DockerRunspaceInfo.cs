// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Linq;
using System.Net;
using VMware.ScriptRuntimeService.Docker.Bindings.Api;
using VMware.ScriptRuntimeService.Docker.Bindings.Client;
using VMware.ScriptRuntimeService.Docker.Bindings.Model;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.DockerRunspaceProvider
{
   /// <summary>
   /// Represents Running Runspace Docker Container Instance with Id and Endpoint IP Address
   /// </summary>
   public class DockerRunspaceInfo : IRunspaceInfo {

      private DockerRunspaceInfo() { }

      public static DockerRunspaceInfo FromRunspaceProviderError(RunspaceProviderException exception) {
         return new DockerRunspaceInfo {
            Id = null,
            Endpoint = null,
            CreationState = RunspaceCreationState.Error,
            CreationError = exception
         };
      }

      public static DockerRunspaceInfo FromContainerInspectResponse(
         ContainerInspectResponse containerInspectResponse, 
         string dockerNetworkName) {

         if (containerInspectResponse == null) {
            throw new RunspaceProviderException(Resources.Resources.DockerRunspaceInfo_FromContainerInspectResponse_ContainerInspectResponse_IsNull);
         }

         if (containerInspectResponse.NetworkSettings?.Networks == null ||
             string.IsNullOrEmpty(dockerNetworkName) ||
             !containerInspectResponse.NetworkSettings.Networks.ContainsKey(dockerNetworkName) ||
             !IPAddress.TryParse(
                containerInspectResponse.NetworkSettings.Networks[dockerNetworkName].IPAddress,
                out var ipAddress)) {

            throw new RunspaceProviderException(
               string.Format(
                  Resources.Resources.DockerRunspaceInfo_DockerRunspaceInfo_InvalidIp,
                  dockerNetworkName));
         }

         if (!TryGetContainerTcpPort(containerInspectResponse.NetworkSettings.Ports, out var port)) {
            throw new RunspaceProviderException(Resources.Resources.DockerRunspaceInfo_DockerRunspaceInfo_NoTcpPort);
         }

         return new DockerRunspaceInfo {
            Id = containerInspectResponse.Id,
            Endpoint = new IPEndPoint(
               ipAddress,
               port)
         };
      }

      private static bool TryGetContainerTcpPort(PortMap portMap, out int port) {
         string portString = string.Empty;

         foreach (var portMapKey in portMap.Keys) {
            if (portMapKey.Contains("/tcp")) {
               portString = portMapKey.Substring(0, portMapKey.IndexOf("/"));
            }
         }

         return int.TryParse(portString, out port);
      }
      
      #region IRunspaceInfo
      public string Id { get; private set; }
      public IPEndPoint Endpoint { get; private set; }

      public RunspaceCreationState CreationState { get; set; }

      public RunspaceProviderException CreationError { get; set; }
      #endregion
   }
}
