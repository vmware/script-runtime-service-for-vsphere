// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Docker.Bindings.Api;
using VMware.ScriptRuntimeService.Docker.Bindings.Model;

namespace VMware.ScriptRuntimeService.Docker.Bindings.Tests
{
   /// <summary>
   /// Contains integrations tests interacting with docker API
   /// To run those configure docker API available on DOCKER_API_BASE_PATH constant
   /// and uncomment [Test] attributes
   /// </summary>
   public class IntegrationTests {      
      private const string DOCKER_API_BASE_PATH = @"http://10.23.82.131:5555/v1.39";
      private IContainerApi _containerApi;
      private List<string> _startedContainers = new List<string>();

      [SetUp]
      public void Setup()
      {
         var basePath = DOCKER_API_BASE_PATH;
         _containerApi = new ContainerApi(basePath);
      }

      [TearDown]
      public void TearDown() {
         // Stop Container Started by the tests
         foreach (var startedContainerId in _startedContainers) {
            _containerApi.ContainerStop(startedContainerId);
         }

         // Deletes all non-running containers
         _containerApi.ContainerPrune();
      }

      //[Test]
      public void TestListContainers() {
         // Act
         var containerSummary = _containerApi.ContainerList();

         // Assert
         Assert.NotNull(containerSummary);
      }

      //[Test]
      public void TestCreateBashContainer() {
         // Arrange
         var containerConfig = new ContainerConfig(image:"bash");
         var name = "bash_instance";

         // Act
         var containerCreateResponse = _containerApi.ContainerCreate(containerConfig, name);

         // Assert
         Assert.NotNull(containerCreateResponse);
         Assert.NotNull(containerCreateResponse.Id);
      }

      //[Test]
      public void TestStartBashContainer()
      {
         // Arrange
         var containerConfig = new ContainerConfig(image: "bash");
         var name = "bash_instance";
         var containerCreateResponse = _containerApi.ContainerCreate(containerConfig, name);

         // Act
         TestDelegate testDelegate = delegate () {
            _containerApi.ContainerStart(containerCreateResponse.Id);
         };

         // Assert
         Assert.DoesNotThrow(testDelegate);

         // Prepare for TearDown
         _startedContainers.Add(containerCreateResponse.Id);
      }

      //[Test]
      public void TestStartMultipleBashContainers()
      {
         // Arrange
         var containerConfig = new ContainerConfig(image: "bash", 
                                                   cmd: new List<string>(){"sleep", "5s"});

         // Act
         for (int i = 0; i < 2; i++) {
            var containerCreateResponse = _containerApi.ContainerCreate(containerConfig);
            _containerApi.ContainerStart(containerCreateResponse.Id);
            _startedContainers.Add(containerCreateResponse.Id);
         }

         // Assert
         var containerSummary = _containerApi.ContainerList();
         Assert.AreEqual(2, containerSummary.Count);
      }

      //[Test]
      public void TestStartContainerConnectedToSpecificNetwork() {
         // Arrange
         var hostConfig = new HostConfig(networkMode: "docker_gwbridge");
         var body = new
         {
            Image = "bash",
            AttachStdin = false,
            AttachStdout = true,
            AttachStderr = true,
            Tty = false,
            OpenStdin = false,
            StdinOnce = false,
            StopSignal = "SIGTERM",
            HostConfig = hostConfig
         };

         // Act
         var containerCreateResponse = _containerApi.ContainerCreate(body);

         // Assert
         Assert.NotNull(containerCreateResponse);
         Assert.NotNull(containerCreateResponse.Id);
      }

      //[Test]
      public void TestStartMultipleBashContainersConnectedToSameNetwork()
      {
         // Arrange
         var containerConfig = new ContainerConfig(image: "bash",
            cmd: new List<string>() { "sleep", "5s" },
            hostConfig: new HostConfig(networkMode: "docker_gwbridge"));

         // Act
         for (int i = 0; i < 2; i++)
         {
            var containerCreateResponse = _containerApi.ContainerCreate(containerConfig);
            _containerApi.ContainerStart(containerCreateResponse.Id);
            _startedContainers.Add(containerCreateResponse.Id);
         }

         // Assert
         var containerSummary = _containerApi.ContainerList();
         Assert.AreEqual(2, containerSummary.Count);
      }
   }
}