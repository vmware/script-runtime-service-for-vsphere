// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollector.InProcDataCollector;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Docker.Bindings.Api;

namespace VMware.ScriptRuntimeService.DockerRunspaceProvider.Tests
{
   /// <summary>
   /// Contains integrations tests interacting with docker API
   /// To run those configure docker API available on DOCKER_ENGINE_ENDPOINT_BASE_PATH constant
   /// and uncomment [Test] attributes
   /// </summary>
   public class Integration {
      private const string DOCKER_ENGINE_ENDPOINT_BASE_PATH = @"http://10.23.82.131:5555/v1.39";
      private const string IMAGE_NAME = "pclirunspace";
      private const string NETWORK_NAME = "docker_gwbridge";
      private DockerRunspaceProvider _runspaceProvider;
      private IContainerApi _containerApi;
      private List<string> _startedContainers = new List<string>();

      [SetUp]
      public void Setup()
      {
         _containerApi = new ContainerApi(DOCKER_ENGINE_ENDPOINT_BASE_PATH);
         _runspaceProvider = new DockerRunspaceProvider(
            new RunspaceContainerCreateSpec {
               ImageName = IMAGE_NAME,
               NetworkName = NETWORK_NAME
            }, 
            DOCKER_ENGINE_ENDPOINT_BASE_PATH,
            _containerApi,
            false);
      }

      [TearDown]
      public void TearDown()
      {
         // Stop Container Started by the tests
         foreach (var startedContainerId in _startedContainers) {
            _containerApi.ContainerStop(startedContainerId);
         }

         // Deletes all non-running containers
         _containerApi.ContainerPrune();
      }

      //[Test]
      public void TestCreateSingleRunspace() {
         // Act
         var runspaceInfo = _runspaceProvider.StartCreate();
         runspaceInfo = _runspaceProvider.WaitCreateCompletion(runspaceInfo);

         // Assert
         Assert.NotNull(runspaceInfo);
         Assert.NotNull(runspaceInfo.Id);
         Assert.NotNull(runspaceInfo.Endpoint);
         Assert.NotNull(runspaceInfo.Endpoint.Address);
         Assert.IsTrue(runspaceInfo.Endpoint.Port > 0);

         // Prepare for TearDown
         _startedContainers.Add(runspaceInfo.Id);
      }

      //[Test]
      public void TestCreateAndCallPowerCLIRunspace() {
         // Act
         var runspaceInfo = _runspaceProvider.StartCreate();
         runspaceInfo = _runspaceProvider.WaitCreateCompletion(runspaceInfo);

         // Assert
         Assert.NotNull(runspaceInfo);
         Assert.NotNull(runspaceInfo.Id);
         Assert.NotNull(runspaceInfo.Endpoint);
         Assert.NotNull(runspaceInfo.Endpoint.Address);
         Assert.IsTrue(runspaceInfo.Endpoint.Port > 0);

         // Prepare for TearDown
         _startedContainers.Add(runspaceInfo.Id);
      }

      //[Test]
      public void TestKillRunsapceContainer() {
         // Arrange
         var runspaceInfo = _runspaceProvider.StartCreate();
         runspaceInfo = _runspaceProvider.WaitCreateCompletion(runspaceInfo);

         // Act
         _runspaceProvider.Kill(runspaceInfo.Id);

         // Assert
         var runningContainers = _containerApi.ContainerList();
         Assert.IsEmpty(runningContainers);
      }

      //[Test]
      public void TestListPowerCLIRunspace() {
         // Arrange
         var runspaceInfo1 = _runspaceProvider.StartCreate();
         runspaceInfo1 = _runspaceProvider.WaitCreateCompletion(runspaceInfo1);
         _startedContainers.Add(runspaceInfo1.Id);
         var runspaceInfo2 = _runspaceProvider.StartCreate();
         runspaceInfo2 = _runspaceProvider.WaitCreateCompletion(runspaceInfo2);

         // Act
         var actual = _runspaceProvider.List();

         // Assert
         Assert.AreEqual(2, actual.Length);
         Assert.IsNotNull(actual.FirstOrDefault(a => a.Id == runspaceInfo1.Id));
         Assert.IsNotNull(actual.FirstOrDefault(a => a.Id == runspaceInfo2.Id));
      }
   }
}
