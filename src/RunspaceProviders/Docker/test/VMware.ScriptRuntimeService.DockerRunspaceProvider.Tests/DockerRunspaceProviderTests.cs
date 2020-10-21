// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Docker.Bindings.Api;
using VMware.ScriptRuntimeService.Docker.Bindings.Client;
using VMware.ScriptRuntimeService.Docker.Bindings.Model;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.DockerRunspaceProvider.Tests
{   
   public class DockerRunspaceProviderTests {
      private const string TEST_DOCKER_ADDRESS = @"http://test:5000";
      private const string TEST_RUNSPACE_ID = "test-runspace-id";
      private const string TEST_IMAGE_NAME = "test-runspace-name";
      private const string TEST_NETWORK_NAME = "test-network-name";
      private const string TEST_IP_ADDRESS = "10.11.12.13";
      private const int TEST_NETWORK_PORT = 5464;

      private Mock<IContainerApi> MockContainerApi() {
         var containerApiMock = new Mock<IContainerApi>();

         // Mock ContainerCreate API
         containerApiMock.Setup(m => m.ContainerCreate(It.IsAny<ContainerConfig>(), null)).Returns(
            new ContainerCreateResponse(TEST_RUNSPACE_ID, new List<string>())
         );

         // Mock ContainerStart API
         containerApiMock.Setup(m => m.ContainerStart(TEST_RUNSPACE_ID, null));

         // Mock ContainerInspect API
         containerApiMock.Setup(m => m.ContainerInspect(TEST_RUNSPACE_ID, null)).Returns(
            new ContainerInspectResponse(
               id : TEST_RUNSPACE_ID,
               networkSettings : new NetworkSettings(
                  networks : new Dictionary<string, EndpointSettings> {
                     {TEST_NETWORK_NAME, new EndpointSettings(iPAddress : TEST_IP_ADDRESS)}
                  },
                  ports : new PortMap {
                     {$"{TEST_NETWORK_PORT}/tcp", new List<string> {"", ""}}
                  }),
               state: new ContainerInspectResponseState(ContainerInspectResponseState.StatusEnum.Running)));

         return containerApiMock;
      }


      [Test]
      public void TestCreate() {
         // Arrange
         var containerApi = MockContainerApi().Object;

         var dockerRunspaceProvider = new DockerRunspaceProvider(
            new RunspaceContainerCreateSpec() {
               ImageName = TEST_IMAGE_NAME,
               NetworkName = TEST_NETWORK_NAME
            },
            TEST_DOCKER_ADDRESS,
            containerApi,
            false);

         // Act
         var actual = dockerRunspaceProvider.StartCreate();
         actual = dockerRunspaceProvider.WaitCreateCompletion(actual);

         // Assert
         Assert.AreEqual(TEST_RUNSPACE_ID, actual.Id);
         Assert.AreEqual(TEST_IP_ADDRESS, actual.Endpoint.Address.ToString());
         Assert.AreEqual(TEST_NETWORK_PORT, actual.Endpoint.Port);
      }

      [Test]
      public void TestGet() {
         // Arrange
         var containerApi = MockContainerApi().Object;

         var dockerRunspaceProvider = new DockerRunspaceProvider(
            new RunspaceContainerCreateSpec() {
               ImageName = TEST_IMAGE_NAME,
               NetworkName = TEST_NETWORK_NAME
            },
            TEST_DOCKER_ADDRESS,
            containerApi,
            false);

         // Act
         var actual = dockerRunspaceProvider.Get(TEST_RUNSPACE_ID);

         // Assert
         Assert.AreEqual(TEST_RUNSPACE_ID, actual.Id);
         Assert.AreEqual(TEST_IP_ADDRESS, actual.Endpoint.Address.ToString());
         Assert.AreEqual(TEST_NETWORK_PORT, actual.Endpoint.Port);
      }

      [Test]
      public void TestKill() {
         // Arrange
         var containerApiMock = new Mock<IContainerApi>();
         containerApiMock.Setup(m => m.ContainerKill(TEST_RUNSPACE_ID, null));

         var dockerRunspaceProvider = new DockerRunspaceProvider(
            new RunspaceContainerCreateSpec() {
               ImageName = TEST_IMAGE_NAME,
               NetworkName = TEST_NETWORK_NAME
            },
            TEST_DOCKER_ADDRESS,
            containerApiMock.Object,
            false);

         // Act & Assert
         Assert.DoesNotThrow(() => dockerRunspaceProvider.Kill(TEST_RUNSPACE_ID));
      }

      [Test]
      public void TestDockerExceptionHandledOnCreate() {
         // Arrange
         var containerApiMock = new Mock<IContainerApi>();

         // Mock ContainerCreate API
         containerApiMock.Setup(m => m.ContainerCreate(It.IsAny<ContainerConfig>(), null))
            .Throws(new ApiException());

         var containerApi = containerApiMock.Object;

         var dockerRunspaceProvider = new DockerRunspaceProvider(
            new RunspaceContainerCreateSpec() {
               ImageName = TEST_IMAGE_NAME,
               NetworkName = TEST_NETWORK_NAME
            },
            TEST_DOCKER_ADDRESS,
            containerApi,
            false);

         // Act
         var actual = dockerRunspaceProvider.StartCreate();
         actual = dockerRunspaceProvider.WaitCreateCompletion(actual);

         // Assert
         Assert.AreEqual(RunspaceCreationState.Error, actual.CreationState);
         Assert.NotNull(actual.CreationError);
         Assert.AreEqual("Docker Engine ContainerCreate API failed", actual.CreationError.Message);         
      }

      [Test]
      public void TestDockerExceptionHandledOnGet()
      {
         // Arrange
         var containerApiMock = new Mock<IContainerApi>();

         // Mock ContainerInspect API
         containerApiMock.Setup(m => m.ContainerInspect(TEST_RUNSPACE_ID, null))
            .Throws(new ApiException());

         var containerApi = containerApiMock.Object;

         var dockerRunspaceProvider = new DockerRunspaceProvider(
            new RunspaceContainerCreateSpec()
            {
               ImageName = TEST_IMAGE_NAME,
               NetworkName = TEST_NETWORK_NAME
            },
            TEST_DOCKER_ADDRESS,
            containerApi,
            false);

         // Act && Assert
         Assert.Throws<RunspaceProviderException>(
            () => dockerRunspaceProvider.Get(TEST_RUNSPACE_ID),
            "Docker Engine ContainerInspect API failed");
      }

      [Test]
      public void TestDockerExceptionHandledOnKill()
      {
         // Arrange
         var containerApiMock = new Mock<IContainerApi>();

         // Mock ContainerInspect API
         containerApiMock.Setup(m => m.ContainerKill(TEST_RUNSPACE_ID, null))
            .Throws(new ApiException());

         var containerApi = containerApiMock.Object;

         var dockerRunspaceProvider = new DockerRunspaceProvider(
            new RunspaceContainerCreateSpec()
            {
               ImageName = TEST_IMAGE_NAME,
               NetworkName = TEST_NETWORK_NAME
            },
            TEST_DOCKER_ADDRESS,
            containerApi,
            false);

         // Act && Assert
         Assert.Throws<RunspaceProviderException>(
            () => dockerRunspaceProvider.Kill(TEST_RUNSPACE_ID),
            "Docker Engine ContainerKill API failed");
      }
   }
}
