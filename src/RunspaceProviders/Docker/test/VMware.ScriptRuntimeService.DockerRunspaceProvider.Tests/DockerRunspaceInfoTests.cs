// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using NUnit.Framework;
using System.Collections.Generic;
using VMware.ScriptRuntimeService.Docker.Bindings.Model;
using VMware.ScriptRuntimeService.DockerRunspaceProvider;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.DockerRunspaceProvider.Tests
{
   public class DockerRunspaceInfoTests {
      private const string DOCKER_NETWORK_NAME = "docker_gwbridge";

      [Test]
      public void TestEndpointConstruction()
      {
         // Arrange
         var runspaceId = "test-id";
         var ipAddress = "10.10.10.10";
         var expectedPort = 80;
         // Container's port-number and protocol in format: <int>[/<string>]
         var portKey = $"{expectedPort}/tcp";

         var input = new ContainerInspectResponse(
            id: runspaceId,
            networkSettings: new NetworkSettings(
               networks: new Dictionary<string, EndpointSettings> {
                  { DOCKER_NETWORK_NAME, new EndpointSettings(iPAddress: ipAddress) }
               },
               ports: new PortMap {
                  { portKey, new List<string>{"127.0.0.1", "4443" } }
               }));

         // Act
         var actual = DockerRunspaceInfo.FromContainerInspectResponse(input, DOCKER_NETWORK_NAME);

         // Assert
         Assert.NotNull(actual);
         Assert.AreEqual(runspaceId, actual.Id);
         Assert.AreEqual(ipAddress, actual.Endpoint.Address.ToString());
         Assert.AreEqual(expectedPort, actual.Endpoint.Port);
      }

      [Test]
      public void TestMultipleContainerPortsForDifferentProtocols()
      {
         // Arrange
         var runspaceId = "test-id";
         var ipAddress = "10.10.10.10";
         var expectedPort = 80;
         var portKey1 = "78";
         var portKey2 = $"{expectedPort}/tcp";
         var portKey3 = "43/udp";

         var input = new ContainerInspectResponse(
            id: runspaceId,
            networkSettings: new NetworkSettings(
               networks: new Dictionary<string, EndpointSettings> {
                  { DOCKER_NETWORK_NAME, new EndpointSettings(iPAddress: ipAddress) }
               },
               ports: new PortMap {
                  { portKey1, new List<string>{"127.0.0.1", "4443" } },
                  { portKey2, new List<string>{"127.0.0.1", "443" } },
                  { portKey3, new List<string>{"127.0.0.1", "3344" } }
               }));

         // Act
         var actual = DockerRunspaceInfo.FromContainerInspectResponse(input, DOCKER_NETWORK_NAME);

         // Assert
         Assert.AreEqual(expectedPort, actual.Endpoint.Port);
      }

      [Test]
      public void TestInvalidIp()
      {
         // Arrange
         var ipAddress = "InvalidIPAddress";

         var runspaceId = "test-id";
         var expectedPort = 80;

         // Container's port-number and protocol in format: <int>[/<string>]
         var portKey = $"{expectedPort}/tcp";

         var input = new ContainerInspectResponse(
            id: runspaceId,
            networkSettings: new NetworkSettings(
               networks: new Dictionary<string, EndpointSettings> {
                  { DOCKER_NETWORK_NAME, new EndpointSettings(iPAddress: ipAddress) }
               },
               ports: new PortMap {
                  { portKey, new List<string>{"127.0.0.1", "4443" } }
               }));

         // Act & Assert
         Assert.Throws<RunspaceProviderException>(
            () => { DockerRunspaceInfo.FromContainerInspectResponse(input, DOCKER_NETWORK_NAME); });
      }

      [Test]
      public void TestNullIp()
      {
         // Arrange
         string ipAddress = null;

         var runspaceId = "test-id";
         var expectedPort = 80;

         // Container's port-number and protocol in format: <int>[/<string>]
         var portKey = $"{expectedPort}/tcp";

         var input = new ContainerInspectResponse(
            id: runspaceId,
            networkSettings: new NetworkSettings(
               networks: new Dictionary<string, EndpointSettings> {
                  { DOCKER_NETWORK_NAME, new EndpointSettings(iPAddress: ipAddress) }
               },
               ports: new PortMap {
                  { portKey, new List<string>{"127.0.0.1", "4443" } }
               }));

         // Act & Assert
         Assert.Throws<RunspaceProviderException>(() => { DockerRunspaceInfo.FromContainerInspectResponse(input, DOCKER_NETWORK_NAME); });
      }

      [Test]
      public void TestInvalidPort()
      {
         // Arrange
         string ipAddress = "10.10.10.10";
         var runspaceId = "test-id";
         var portKey = "80";

         var input = new ContainerInspectResponse(
            id: runspaceId,
            networkSettings: new NetworkSettings(
               networks: new Dictionary<string, EndpointSettings> {
                  { DOCKER_NETWORK_NAME, new EndpointSettings(iPAddress: ipAddress) }
               },
               ports: new PortMap {
                  { portKey, new List<string>{"127.0.0.1", "4443" } }
               }));

         // Act & Assert
         Assert.Throws<RunspaceProviderException>(() => { DockerRunspaceInfo.FromContainerInspectResponse(input, DOCKER_NETWORK_NAME); });
      }
   }
}