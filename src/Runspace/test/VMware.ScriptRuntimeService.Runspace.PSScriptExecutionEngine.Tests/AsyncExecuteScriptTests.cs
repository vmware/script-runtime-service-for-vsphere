// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using NUnit.Framework;
using System.Threading;
using VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.Tests {
   public class AsyncExecuteScriptTests {
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
         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

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
         var script2Content = "Get-ChildItem";

         // Act
         _scriptExecutionEngine.ExecuteScriptAsync(script2Content).Wait();

         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(script2Content).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Success, result.State);
         Assert.IsNotEmpty(result.OutputObjectCollection.FormattedTextPresentation);
      }

      [Test]
      public void CancelScriptExecution() {
         // Arrange
         var scriptContent = "while ($true) { Write-Output '23'; Start-Sleep 1; }";         

         // Act         
         var cancellationToken = new CancellationTokenSource();

         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent, cancellationToken.Token).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });
         
         cancellationToken.Cancel();
         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Canceled, result.State);
         Assert.IsTrue(result.Reason.Contains("The pipeline has been stopped"));
         Assert.NotNull(result.StarTime);
         Assert.NotNull(result.EndTime);
      }

      [Test]
      public void StreamsAreCapturedBeforeScriptCancellation() {
         // Arrange
         var errorMessage = "Test Error";
         var scriptContent = "while ($true) { Write-Error " + $"'{errorMessage}'" + "; Start-Sleep 1; }";

         // Act         
         var cancellationToken = new CancellationTokenSource();

         IScriptExecutionResult result = null;
         var task = _scriptExecutionEngine.ExecuteScriptAsync(scriptContent, cancellationToken.Token).
            ContinueWith((executionResult) => {
               result = executionResult.Result;
            });

         Thread.Sleep(TimeSpan.FromSeconds(2));
         cancellationToken.Cancel();
         task.Wait();

         // Assert
         Assert.IsNotNull(result);
         Assert.AreEqual(ScriptState.Canceled, result.State);
         Assert.IsNotEmpty(result.Streams.Error);
         Assert.AreEqual(errorMessage, result.Streams.Error[0].Message);
      }
   }
}