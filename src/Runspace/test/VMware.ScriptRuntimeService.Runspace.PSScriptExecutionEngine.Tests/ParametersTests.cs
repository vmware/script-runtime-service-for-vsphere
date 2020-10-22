// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.Tests {
   public class ParametersTests {
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
            Value = 1
         };
         var expected = 6;

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter});

         // Assert
         Assert.NotNull(result);
         Assert.IsTrue(int.TryParse(
            result.OutputObjectCollection.FormattedTextPresentation.Trim(), 
            out var actual));
         Assert.AreEqual(expected, actual);
      }

      [Test]
      public void ScriptWithTwoParameters() {
         // Arrange
         var scriptContent = "param([Parameter()][int]$a, [int]$b) $result = $a + $b; $result";
         var aParameter = new ScriptParameter {
            Name = "a",
            Value = 1
         };
         var bParameter = new ScriptParameter {
            Name = "b",
            Value = 4
         };
         var expected = 5;

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter, bParameter });

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
         var scriptContent = "param($a, $b) $result = $a + $b; $result";
         var aParameter = new ScriptParameter {
            Name = "a",
            Value = "Good"
         };
         var bParameter = new ScriptParameter {
            Name = "b",
            Value = 1
         };
         var expected = "Good1";

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter, bParameter });

         // Assert
         Assert.NotNull(result);
         var actual =
            result.OutputObjectCollection.FormattedTextPresentation.Trim();
         Assert.AreEqual(expected, actual);
      }

      [Test]
      public void SwitchParameter() {
         // Arrange
         var scriptContent = "param([Parameter()][Switch]$a) if ($a) { 5 } else { 0 }";
         var aParameter = new ScriptParameter {
            Name = "a"
         };
         var expected = 5;

         // Act
         var result = _scriptExecutionEngine.ExecuteScript(scriptContent, new[] { aParameter });

         // Assert
         Assert.NotNull(result);
         Assert.IsTrue(int.TryParse(
            result.OutputObjectCollection.FormattedTextPresentation.Trim(),
            out var actual));
         Assert.AreEqual(expected, actual);
      }
   }
}
