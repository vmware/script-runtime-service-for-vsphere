// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.Tests {
   public class SyncExecuteScriptTests {
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
      public void SingleCmdletCall() {
         // Arrange
         var scriptContent = "Get-ChildItem";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.FormattedTextPresentation);
         Assert.NotNull(result.StarTime);
         Assert.NotNull(result.EndTime);
      }

      [Test]
      public void MultipleScriptExecution() {
         // Arrange
         var script1Content = "Get-Item";
         var script2Content = "Get-ChildItem";
         _scriptExecutionEngine.ExecuteScript(script1Content);

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(script2Content);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.FormattedTextPresentation);
         Assert.NotNull(result.StarTime);
         Assert.NotNull(result.EndTime);
      }

      [Test]
      public void ErrorStreamIsCaptured() {
         // Arrange
         var message = "Test Error";
         var script1Content = $"Write-Error '{message}'";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(script1Content);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(message, result.Streams.Error[0].Message);
         Assert.IsEmpty(result.Streams.Information);
         Assert.IsEmpty(result.Streams.Debug);
         Assert.IsEmpty(result.Streams.Verbose);
         Assert.IsEmpty(result.Streams.Warning);
         Assert.IsEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void WarningStreamIsCaptured() {
         // Arrange
         var message = "Test Warning";
         var script1Content = $"Write-Warning '{message}'";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(script1Content);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(message, result.Streams.Warning[0].Message);
         Assert.IsEmpty(result.Streams.Information);
         Assert.IsEmpty(result.Streams.Debug);
         Assert.IsEmpty(result.Streams.Verbose);
         Assert.IsEmpty(result.Streams.Error);
         Assert.IsEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void InformationStreamIsCaptured() {
         // Arrange
         var message = "Test Information";
         var script1Content = $"Write-Information '{message}'";
         //    Set PowerShell InformationPreference to Continue
         _scriptExecutionEngine.ExecuteScript("$InformationPreference = Continue");

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(script1Content);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(message, result.Streams.Information[0].Message);
         Assert.IsEmpty(result.Streams.Warning);
         Assert.IsEmpty(result.Streams.Debug);
         Assert.IsEmpty(result.Streams.Verbose);
         Assert.IsEmpty(result.Streams.Error);
         Assert.IsEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void VerboseStreamIsCaptured() {
         // Arrange
         var message = "Test Verbose";
         var script1Content = $"Write-Verbose '{message}'";
         //    Set PowerShell InformationPreference to Continue
         _scriptExecutionEngine.ExecuteScript("$VerbosePreference = 'Continue'");

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(script1Content);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(message, result.Streams.Verbose[0].Message);
         Assert.IsEmpty(result.Streams.Warning);
         Assert.IsEmpty(result.Streams.Debug);
         Assert.IsEmpty(result.Streams.Information);
         Assert.IsEmpty(result.Streams.Error);
         Assert.IsEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void DebugStreamIsCaptured() {
         // Arrange
         var message = "Test Debug";
         var script1Content = $"Write-Debug '{message}'";
         //    Set PowerShell InformationPreference to Continue
         _scriptExecutionEngine.ExecuteScript("$DebugPreference = 'Continue'");

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(script1Content);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(message, result.Streams.Debug[0].Message);
         Assert.IsEmpty(result.Streams.Warning);
         Assert.IsEmpty(result.Streams.Verbose);
         Assert.IsEmpty(result.Streams.Information);
         Assert.IsEmpty(result.Streams.Error);
         Assert.IsEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void ScriptStatusIsSuccessOnNonTerminatingError() {
         // Arrange
         var scriptContent = "Write-Error 'Test Error'; Get-ChildItem";
         //    Set PowerShell ErrorActionPreference to Continue
         _scriptExecutionEngine.ExecuteScript("$ErrorActionPreference = 'Continue'");

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNull(result.Reason);
         Assert.IsNotEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void ScriptStatusIsErrorOnException() {
         // Arrange
         var scriptContent = "throw 'Test Error'; Get-ChildItem";
         //    Set PowerShell ErrorActionPreference to Continue
         _scriptExecutionEngine.ExecuteScript("$ErrorActionPreference = 'Continue'");

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Error, result.State);
         Assert.IsTrue(result.Reason.Contains("Test Error"));
         Assert.IsEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void SingleTypeObjectsResult() {
         // Arrange
         var scriptContent = @"$foo = '' | select Bar
            $foo.Bar = 'bar'
            Write-Output $foo";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent);

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

   }
}