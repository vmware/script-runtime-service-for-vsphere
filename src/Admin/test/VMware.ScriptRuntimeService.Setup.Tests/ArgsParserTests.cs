// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************
using NUnit.Framework;
using NUnit.Framework.Internal;
using VMware.ScriptRuntimeService.Setup;

namespace VMware.ScriptRuntimeService.Setup.Tests
{
   public class ArgsParserTests
   {
      [SetUp]
      public void Setup()
      {
      }

      [Test]
      public void AssertValidLowCasedNamedParameters()
      {
         // Arrage
         string[] testArgs = {
            "-psc", "testHostname",
            "-user", "test@domain"
         };

         // Act
         var userInput = new ArgsParser().Parse(testArgs);

         // Assert
         Assert.NotNull(userInput);
         Assert.AreEqual("testHostname", userInput.Psc);
         Assert.AreEqual("test@domain", userInput.User);
      }

      [Test]
      public void AssertValidMixCasedNamedParameters()
      {
         // Arrage
         string[] testArgs = {
            "-Psc", "testHostname",
            "-USER", "test@domain"
         };

         // Act
         var userInput = new ArgsParser().Parse(testArgs);

         // Assert
         Assert.NotNull(userInput);
         Assert.AreEqual("testHostname", userInput.Psc);
         Assert.AreEqual("test@domain", userInput.User);;
      }
   }
}
