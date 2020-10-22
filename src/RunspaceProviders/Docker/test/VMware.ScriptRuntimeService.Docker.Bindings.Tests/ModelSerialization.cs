// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Newtonsoft.Json;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Docker.Bindings.Model;

namespace VMware.ScriptRuntimeService.Docker.Bindings.Tests
{
   public class ModelSerialization
   {
      [Test]
      public void TestContainerConfigToJson()
      {
         // Arrange
         var containerConfig = new ContainerConfig(image: "bash");

         // Act
         var actualJson = containerConfig.ToJson();

         // Assert
         Assert.IsTrue(actualJson.Contains("\"Image\": \"bash\""));
      }

      [Test]
      public void TestContainerCreateBodyWithHostConfig() {
         // Arrange
         var hostConfig = new HostConfig(networkMode: "docker_gwbridge");
         var body = new {
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
         var actualJson = JsonConvert.SerializeObject(body, Formatting.Indented);

         // Assert
         Assert.NotNull(actualJson);
      }

      [Test]
      public void TestContainerCreateBodyWithHostConfigFromInstanceOfContainerConfig()
      {
         // Arrange
         var containerConfig = new ContainerConfig(image: "bash", 
            hostConfig: new HostConfig(networkMode: "docker_gwbridge"));

         var hostConfig = new HostConfig(networkMode: "docker_gwbridge");
         var body = new
         {
            AttachStdin = false,
            AttachStdout = true,
            AttachStderr = true,
            Tty = false,
            OpenStdin = false,
            StdinOnce = false,
            Image = "bash",
            StopSignal = "SIGTERM",
            HostConfig = hostConfig
         };
         var expectedJson = JsonConvert.SerializeObject(body, Formatting.Indented);


         // Act
         var actualJson = JsonConvert.SerializeObject(containerConfig, Formatting.Indented);

         // Assert
         Assert.AreEqual(expectedJson, actualJson);
      }

   }
}
