// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using NUnit.Framework;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.Tests {
   public class JsonToPsObjectConvertion {
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
      public void ConvertStructTypeObject() {
         // Arrange
         var inputJson = "{ 'Age': 10, 'Name': 'TestName'}";

         // Act
         var actual = _scriptExecutionEngine.JsonObjectToNativeEngineObject(inputJson);

         // Assert
         Assert.NotNull(actual);
         Assert.IsInstanceOf<PSObject>(actual);
         Assert.AreEqual(10, ((PSObject) actual).Properties["Age"].Value);
         Assert.AreEqual("TestName", ((PSObject) actual).Properties["Name"].Value);
      }

      [Test]
      public void ConvertIntArray() {
         // Arrange
         var inputJson = "[10, 11]";

         // Act
         var actual = _scriptExecutionEngine.JsonObjectToNativeEngineObject(inputJson);

         // Assert
         Assert.NotNull(actual);
         Assert.IsInstanceOf<PSObject>(actual);
         Assert.AreEqual(10, ((object[]) ((PSObject) actual).BaseObject)[0]);
         Assert.AreEqual(11, ((object[]) ((PSObject) actual).BaseObject)[1]);
      }

      [Test]
      public void ConvertStructArray() {
         // Arrange
         var inputJson = "[{ 'Age': 10, 'Name': 'Test'}, { 'Age': 11, 'Name': 'Name'}]";

         // Act
         var actual = _scriptExecutionEngine.JsonObjectToNativeEngineObject(inputJson);

         // Assert
         Assert.NotNull(actual);
         Assert.IsInstanceOf<PSObject>(actual);
         Assert.AreEqual(10, ((PSObject) ((object[]) ((PSObject) actual).BaseObject)[0]).Properties["Age"].Value);
         Assert.AreEqual("Test", ((PSObject) ((object[]) ((PSObject) actual).BaseObject)[0]).Properties["Name"].Value);
         Assert.AreEqual(11, ((PSObject) ((object[]) ((PSObject) actual).BaseObject)[1]).Properties["Age"].Value);
         Assert.AreEqual("Name", ((PSObject) ((object[]) ((PSObject) actual).BaseObject)[1]).Properties["Name"].Value);
      }

      [Test]
      public void ConvertStructWithStructProperty() {
         // Arrange
         var inputJson = "{ 'Person': { 'Age': 10, 'Name': 'Test'} }";

         // Act
         var actual = _scriptExecutionEngine.JsonObjectToNativeEngineObject(inputJson);

         // Assert
         Assert.NotNull(actual);
         Assert.IsInstanceOf<PSObject>(actual);
         Assert.IsInstanceOf<PSObject>(((PSObject)actual).Properties["Person"].Value);
         Assert.AreEqual(10, ((PSObject)((PSObject)actual).Properties["Person"].Value).Properties["Age"].Value);
         Assert.AreEqual("Test", ((PSObject)((PSObject)actual).Properties["Person"].Value).Properties["Name"].Value);
      }
   }
}
