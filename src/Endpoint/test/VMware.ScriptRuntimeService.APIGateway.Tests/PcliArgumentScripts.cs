// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.SystemScripts;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class PcliArgumentScripts {
      [Test]
      public void TransformationTemplateIsRetrievedByName() {
         // Arrange
         var scriptName = "vm-by-id-server-name";
         var expectedContent = "Get-VM -Id <id> -Server <server>";

         // Act
         var actual = PCLIScriptsReader.GetArgumentTransformationScript(scriptName);

         // Assert
         Assert.AreEqual(scriptName, actual.Id);
         Assert.AreEqual(expectedContent, actual.ScriptTemplate);
         Assert.AreEqual("VMware.VimAutomation.Types.VirtualMachine", actual.ResultType);
         Assert.AreEqual("PowerCLI", actual.ScriptRuntime);         
      }

      [Test]
      public void ExistsResturnsTrueForExistingScript() {
         // Arrange
         var scriptName = "vm-by-id-server-name";

         // Act
         var actual = PCLIScriptsReader.ArgumentTransformationExists(scriptName);

         // Assert
         Assert.IsTrue(actual);
      }

      [Test]
      public void ExistsResturnsFalseForNonExistingScript() {
         // Arrange
         var scriptName = "inexistent";

         // Act
         var actual = PCLIScriptsReader.ArgumentTransformationExists(scriptName);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void ListResturns() {
         // Arrange
         var expectedItem = "vm-by-id-server-name";

         // Act
         var actual = new List<ArgumentScriptTemplate>(PCLIScriptsReader.ListArgumentTransformationScriptNames());

         // Assert
         Assert.IsNotNull(actual);
         Assert.Contains(expectedItem, actual.Select( a => a.Id ).ToArray());
      }

      [Test]
      public void GenerateByOneIdAndOneServerName() {
         // Arrange
         var scriptName = "vm-by-id-server-name";
         var template = PCLIScriptsReader.GetArgumentTransformationScript(scriptName);
         var idParameter = new PlaceholderValue {
            PlaceholderName = "id",
            Value = new [] {"vm-2"}
         };
         var serverParameter = new PlaceholderValue {
            PlaceholderName = "server",
            Value = new[] { "10.23.45.67" }
         };
         var expected = "Get-VM -Id 'vm-2' -Server '10.23.45.67'";

         // Act
         var actual = new PCLIScriptsGenerator(
            template.ScriptTemplate,
            new []{
               new PlaceholderValueList {
                  Values = new [] { idParameter, serverParameter }
               }
            }).Generate();

         // Assert
         Assert.AreEqual(expected, actual.Script);
      }

      [Test]
      public void GenerateByTwoIdAndOneServerGuid() {
         // Arrange
         var scriptName = "vm-by-id-server-instance-uuid";
         var template = PCLIScriptsReader.GetArgumentTransformationScript(scriptName);
         var idParameter = new PlaceholderValue {
            PlaceholderName = "id",
            Value = new[] { "vm-2", "vm-3" }
         };
         var serverParameter = new PlaceholderValue {
            PlaceholderName = "server_instance_uuid",
            Value = new[] { "uuid_uuid" }
         };
         var expected = "Get-VM -Id 'vm-2','vm-3' -Server ($global:DefaultVIServers | Where-Object {$_.InstanceUuid -eq 'uuid_uuid'})";

         // Act
         var actual = new PCLIScriptsGenerator(
            template.ScriptTemplate,
            new[]{
                  new PlaceholderValueList {
               Values = new []{idParameter, serverParameter}
            }}).Generate();

         // Assert
         Assert.AreEqual(expected, actual.Script);
      }

      [Test]
      public void GenerateByOneIdAndTwoServerName() {
         // Arrange
         var scriptName = "vm-by-id-server-name";
         var template = PCLIScriptsReader.GetArgumentTransformationScript(scriptName);
         var idParameter = new PlaceholderValue {
            PlaceholderName = "id",
            Value = new[] { "vm-2" }
         };
         var serverParameter = new PlaceholderValue {
            PlaceholderName = "server",
            Value = new[] { "10.23.45.67"}
         };

         var server2Parameter = new PlaceholderValue {
            PlaceholderName = "server",
            Value = new[] { "67.45.23.10" }
         };
         var expectedSb = new StringBuilder();
         expectedSb.AppendLine("Get-VM -Id 'vm-2' -Server '10.23.45.67'");
         expectedSb.AppendLine("Get-VM -Id 'vm-2' -Server '67.45.23.10'");
         var expected = expectedSb.ToString().Trim();

         // Act
         var actual = new PCLIScriptsGenerator(
            template.ScriptTemplate,
            new[]{
               new PlaceholderValueList { Values = new []{idParameter, serverParameter} },
               new PlaceholderValueList { Values = new []{idParameter, server2Parameter} }
            }).Generate();

         // Assert
         Assert.AreEqual(expected, actual.Script);
      }
   }
}
