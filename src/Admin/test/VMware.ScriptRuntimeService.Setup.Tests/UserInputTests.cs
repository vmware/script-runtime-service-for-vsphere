// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************


using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;

namespace VMware.ScriptRuntimeService.Setup.Tests
{
   public class UserInputTests  {
      [TearDown]
      public void CleanEnvironment() {
         Environment.SetEnvironmentVariable("TARGET_VC_SERVER", null);
         Environment.SetEnvironmentVariable("TARGET_VC_USER", null);
         Environment.SetEnvironmentVariable("TARGET_VC_PASSWORD", null);
         Environment.SetEnvironmentVariable("TARGET_VC_THUMBPRINT", null);
         Environment.SetEnvironmentVariable("TARGET_VC_INSECURE", null);
         Environment.SetEnvironmentVariable("SERVICE_HOSTNAME", null);
      }

      [Test]
      public void InitializeFromEnvironmentWithThumbPrint() {
         // Arrange
         var expectedVc = "10.10.10.10";
         var expectedVcUser = "VcUser";
         var expectedVcPassword = "VcPassword";
         var expectedVcThumbprint = "VcThumbprint";
         var expectedForceSpecified = false;
         var expectedHostName = "test.ses.hostname";

         Environment.SetEnvironmentVariable("SERVICE_HOSTNAME", expectedHostName);
         Environment.SetEnvironmentVariable("TARGET_VC_SERVER", expectedVc);
         Environment.SetEnvironmentVariable("TARGET_VC_USER", expectedVcUser);
         Environment.SetEnvironmentVariable("TARGET_VC_PASSWORD", expectedVcPassword);
         Environment.SetEnvironmentVariable("TARGET_VC_THUMBPRINT", expectedVcThumbprint);

         // Act
         var actual = new UserInput();

         // Assert
         Assert.AreEqual(expectedHostName, actual.ServiceHostname);
         Assert.AreEqual(expectedVc, actual.Psc);
         Assert.AreEqual(expectedVcUser, actual.User);
         Assert.NotNull(actual.Password);
         Assert.AreEqual(expectedVcThumbprint, actual.VcThumbprint);
         Assert.AreEqual(expectedForceSpecified, actual.ForceSpecified);
      }

      [Test]
      public void InitializeFromEnvironmentInsecure() {
         // Arrange
         var expectedVc = "10.10.10.10";
         var expectedVcUser = "VcUser";
         var expectedVcPassword = "VcPassword";         
         var expectedForceSpecified = true;

         Environment.SetEnvironmentVariable("TARGET_VC_SERVER", expectedVc);
         Environment.SetEnvironmentVariable("TARGET_VC_USER", expectedVcUser);
         Environment.SetEnvironmentVariable("TARGET_VC_PASSWORD", expectedVcPassword);
         Environment.SetEnvironmentVariable("TARGET_VC_INSECURE", "TRUE");

         // Act
         var actual = new UserInput();

         // Assert
         Assert.AreEqual(expectedVc, actual.Psc);
         Assert.AreEqual(expectedVcUser, actual.User);
         Assert.NotNull(actual.Password);
         Assert.IsNull(actual.VcThumbprint);
         Assert.AreEqual(expectedForceSpecified, actual.ForceSpecified);
      }
   }
}
