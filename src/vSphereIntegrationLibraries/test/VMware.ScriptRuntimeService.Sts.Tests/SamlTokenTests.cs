// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using VMware.ScriptRuntimeService.Sts.Tests.Properties;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Sts.SamlToken;

namespace VMware.ScriptRuntimeService.Sts.Tests {
   public class SamlTokenTests {
      [Test]
      public void ParseHoKToken() {
         // Arrange & Act
         var samlToken = new SamlToken.SamlToken(Resources.HoKSamlToken);

         // Assert
         Assert.AreEqual(samlToken.Id, "_58c4a343-5401-4684-8459-6878af0381ca");
         Assert.AreEqual(samlToken.SubjectNameId, "Administrator@VSPHERE.LOCAL");
         Assert.AreEqual(samlToken.ConfirmationType, ConfirmationType.HolderOfKey);
         Assert.AreEqual(samlToken.StartTime.Date, new DateTime(2020, 1, 14));
         Assert.AreEqual(samlToken.ExpirationTime.Date, new DateTime(2020, 2, 13));
         Assert.NotNull(samlToken.ConfirmationCertificate);
      }

      [Test]
      public void ParseBearerToken() {
         // Arrange & Act
         var samlToken = new SamlToken.SamlToken(Resources.BearerSamlToken);

         // Assert
         Assert.AreEqual(samlToken.Id, "_c92309e6-30c4-43a5-8895-44e5cc337aaf");
         Assert.AreEqual(samlToken.SubjectNameId, "Administrator@VSPHERE.LOCAL");
         Assert.AreEqual(samlToken.ConfirmationType, ConfirmationType.Bearer);
         Assert.AreEqual(samlToken.StartTime.AddMilliseconds(-samlToken.StartTime.Millisecond), new DateTime(2020, 1, 14, 9, 13, 58));
         Assert.AreEqual(samlToken.ExpirationTime.AddMilliseconds(-samlToken.ExpirationTime.Millisecond), new DateTime(2020, 1, 14, 9, 18, 58));
         Assert.IsNull(samlToken.ConfirmationCertificate);
      }
   }
}
