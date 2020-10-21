// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.Tests {
   public class ParametersWithArgumentTransformationTests {
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
      public void ScriptWithOneParameter() {
         // Arrange
         var scriptContent = "param([Parameter()][int]$a) $result = $a + 5; $result";
         var aParameter = new ScriptParameter {
            Name = "a",
            Value = 1,
            Script = "param($argument) $argument + 2"
         };
         var expected = 8;

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter });

         // Assert
         Assert.NotNull(result);
         Assert.IsTrue(int.TryParse(
            result.OutputObjectCollection.FormattedTextPresentation.Trim(),
            out var actual));
         Assert.AreEqual(expected, actual);
      }

      [Test]
      public void ScriptWithTwoNoTypeParameters() {
         // Arrange
         var scriptContent = "param($a, $b) $a + $b";
         var aParameter = new ScriptParameter {
            Name = "a",
            Value = "Good",
            Script = "param($argument) $argument + 4"
         };
         var bParameter = new ScriptParameter {
            Name = "b",
            Value = "Yo",
            Script = "param($argument) $argument + 'u'"
         };

         var expected = "Good4You";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter, bParameter });

         // Assert
         Assert.NotNull(result);
         var actual = result.OutputObjectCollection.FormattedTextPresentation.Trim();
         Assert.AreEqual(expected, actual);
      }

      [Test]
      public void PSObjectTransformation() {
         // Arrange
         var scriptContent = "param($a) $a.Age += 15; $a";
         var aParameter = new ScriptParameter {
            Name = "a",
            Value = "{'Name':'Dimitar','Age':23}",
            Script = "param($argument) ConvertFrom-Json $argument"
         };

         var expectedName = "\"Name\": \"Dimitar\"";
         var expectedAge = "\"Age\": 38";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter }, Types.OutputObjectsFormat.Json);

         // Assert
         Assert.NotNull(result);
         var actual = result.OutputObjectCollection.SerializedObjects[0];
         Assert.IsTrue(actual.Contains(expectedName));
         Assert.IsTrue(actual.Contains(expectedAge));
      }

      [Test]
      public void ArgumentTransformationStreamsAreHandled() {
         // Arrange
         var scriptContent = "param([Parameter()][int]$a) $result = $a + 5; $result";
         var aParameter = new ScriptParameter {
            Name = "a",
            Value = 1,
            Script = "param($argument) Write-Error 'ArgTransformError'; $argument + 2"
         };
         var expected = 8;
         var expectedErrorStreamRecord = "ArgTransformError";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter });

         // Assert
         Assert.NotNull(result);
         Assert.IsTrue(int.TryParse(
            result.OutputObjectCollection.FormattedTextPresentation.Trim(),
            out var actual));
         Assert.AreEqual(expected, actual);
         Assert.AreEqual(1, result.Streams.Error.Length);
         Assert.AreEqual(expectedErrorStreamRecord, result.Streams.Error[0].Message);
      }

      [Test]
      public void ArgumentTransformationErrorMakesScriptFail() {
         // Arrange
         var scriptContent = "param([Parameter()][int]$a) $result = $a + 5; $result";
         var aParameter = new ScriptParameter {
            Name = "a",
            Value = 1,
            Script = "param($argument) throw 'ArgTransformError'; $argument + 2"
         };
         var expectedError = "ArgTransformError";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter });

         // Assert
         Assert.NotNull(result);
         Assert.AreEqual(Types.ScriptState.Error, result.State);
         Assert.IsTrue(result.Reason.Contains(expectedError));
      }
   }
}
