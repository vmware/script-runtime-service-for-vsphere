// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.Tests {
   public class SerializedOutputObjectsTests {
      PSScriptExecutionEngine _scriptExecutionEngine;
      [SetUp]
      public void Setup() {
         _scriptExecutionEngine = new PSScriptExecutionEngine();
      }

      [TearDown]
      public void TearDown() {
         _scriptExecutionEngine.Dispose();
      }

      [Test]
      public void SinglePSObjectOutput() {
         // Arrange
         var scriptContent = new StringBuilder();
         scriptContent.AppendLine("$psObject = New-Object PSObject");
         scriptContent.AppendLine("$psObject | Add-Member -MemberType NoteProperty -Name 'Foo' -Value 'TestFoo'");
         scriptContent.AppendLine("$psObject | Add-Member -MemberType NoteProperty -Name 'Bar' -Value 'TestBar'");
         scriptContent.AppendLine("$psObject");
         
         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent.ToString(), OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(1, result.OutputObjectCollection.SerializedObjects.Length);
         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.Management.Automation.PSCustomObject"","));
         Assert.IsTrue(jsonObject.Contains(@"""Interfaces"": null,"));
         Assert.IsTrue(jsonObject.Contains(@"""Foo"": ""TestFoo"""));
         Assert.IsTrue(jsonObject.Contains(@"""Bar"": ""TestBar"""));
      }

      [Test]
      public void MultiplePSObjectOutput() {
         // Arrange
         var scriptContent = new StringBuilder();
         scriptContent.AppendLine("$psObject1 = New-Object PSObject");
         scriptContent.AppendLine("$psObject1 | Add-Member -MemberType NoteProperty -Name 'Foo' -Value 'TestFoo1'");
         scriptContent.AppendLine("$psObject1 | Add-Member -MemberType NoteProperty -Name 'Bar' -Value 'TestBar1'");
         
         scriptContent.AppendLine("$psObject2 = New-Object PSObject");
         scriptContent.AppendLine("$psObject2 | Add-Member -MemberType NoteProperty -Name 'Foo' -Value 'TestFoo2'");
         scriptContent.AppendLine("$psObject2 | Add-Member -MemberType NoteProperty -Name 'Bar' -Value 'TestBar2'");

         scriptContent.AppendLine("Write-Output $psObject1");
         scriptContent.AppendLine("Write-Output $psObject2");

         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent.ToString(), OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(2, result.OutputObjectCollection.SerializedObjects.Length);

         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.Management.Automation.PSCustomObject"","));
         Assert.IsTrue(jsonObject.Contains(@"""Interfaces"": null,"));
         Assert.IsTrue(jsonObject.Contains(@"""Foo"": ""TestFoo1"""));
         Assert.IsTrue(jsonObject.Contains(@"""Bar"": ""TestBar1"""));

         jsonObject = result.OutputObjectCollection.SerializedObjects[1];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.Management.Automation.PSCustomObject"","));
         Assert.IsTrue(jsonObject.Contains(@"""Interfaces"": null,"));
         Assert.IsTrue(jsonObject.Contains(@"""Foo"": ""TestFoo2"""));
         Assert.IsTrue(jsonObject.Contains(@"""Bar"": ""TestBar2"""));
      }

      [Test]
      public void HashtableOutput() {
         // Arrange
         var scriptContent = new StringBuilder();
         scriptContent.AppendLine("$ht = @{}");
         scriptContent.AppendLine("$ht['Foo'] = 'TestFoo'");
         scriptContent.AppendLine("$ht['Bar'] = 'TestBar'");
         scriptContent.AppendLine("$ht");

         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent.ToString(), OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(1, result.OutputObjectCollection.SerializedObjects.Length);
         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.Collections.Hashtable"","));
         Assert.IsTrue(jsonObject.Contains(@"""System.Collections.IDictionary"""));
         Assert.IsTrue(jsonObject.Contains(@"""Foo"": ""TestFoo"""));
         Assert.IsTrue(jsonObject.Contains(@"""Bar"": ""TestBar"""));
      }

      [Test]
      public void StringOutput() {
         // Arrange
         var scriptContent = "Write-Output 'Foo'";

         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent, OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(1, result.OutputObjectCollection.SerializedObjects.Length);
         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.String"","));
         Assert.IsTrue(jsonObject.Contains(@"""Value"": ""Foo"""));
      }

      [Test]
      public void IntOutput() {
         // Arrange
         var scriptContent = "[int]35";

         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent, OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(1, result.OutputObjectCollection.SerializedObjects.Length);
         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.Int32"","));
         Assert.IsTrue(jsonObject.Contains(@"""Value"": 35"));
      }

      [Test]
      public void DoubleOutput() {
         // Arrange
         var scriptContent = "[double]35.6";

         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent, OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(1, result.OutputObjectCollection.SerializedObjects.Length);
         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.Double"","));
         Assert.IsTrue(jsonObject.Contains(@"""Value"": 35.6"));
      }

      [Test]
      public void DateOutput() {
         // Arrange
         var scriptContent = "Get-Date -Day 3 -Month 3 -Year 2013 -Hour 12 -Minute 0 -Second 0 -Millisecond 0";
         //    Expect ISO 8601 date format
         var expectedDateValue = new DateTime(2013, 3, 3, 12, 0, 0, DateTimeKind.Local).
            ToUniversalTime().
            GetDateTimeFormats('o')[0];

         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent, OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(1, result.OutputObjectCollection.SerializedObjects.Length);
         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains(@"""TypeName"": ""System.DateTime"","));
         Assert.IsTrue(jsonObject.Contains($"\"Value\": \"{expectedDateValue}\""));
      }

      [Test]
      public void ObjectWithDatePropertyOutput() {
         // Arrange
         var scriptContent = new StringBuilder();
         scriptContent.AppendLine("$dt = Get-Date -Day 3 -Month 3 -Year 2013 -Hour 12 -Minute 0 -Second 0 -Millisecond 0");
         scriptContent.AppendLine("$psObject = New-Object PSObject");
         scriptContent.AppendLine("$psObject | Add-Member -MemberType NoteProperty -Name 'Time' -Value $dt");
         scriptContent.AppendLine("$psObject");
         //    Expect ISO 8601 date format
         var expectedDateValue = new DateTime(2013, 3, 3, 12, 0, 0, DateTimeKind.Local).
            ToUniversalTime().
            GetDateTimeFormats('o')[0];

         // Act
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent.ToString(), OutputObjectsFormat.Json).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.SerializedObjects);
         Assert.AreEqual(1, result.OutputObjectCollection.SerializedObjects.Length);
         var jsonObject = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(jsonObject.Contains($"\"Time\": \"{expectedDateValue}\""));
      }
   }
}
