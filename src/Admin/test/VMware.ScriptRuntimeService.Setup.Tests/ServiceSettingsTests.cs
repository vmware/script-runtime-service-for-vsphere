// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.IO;
using NUnit.Framework;
using VMware.ScriptRuntimeService.AdminEngine;
using VMware.ScriptRuntimeService.Setup;

namespace VMware.ScriptRuntimeService.Setup.Tests
{
   public class ServiceSettingsTests
   {
      [SetUp]
      public void Setup()
      {
      }

      [Test]
      public void TestThrowsExceptionWheFileDoesntExist()
      {
         // Arrange
         var jsonFileName = "Unexistent.json";

         // Act, Assert
         Assert.Throws(typeof(FileNotFoundException), () => SetupServiceSettings.FromConfigDir(jsonFileName));
      }
   }
}
