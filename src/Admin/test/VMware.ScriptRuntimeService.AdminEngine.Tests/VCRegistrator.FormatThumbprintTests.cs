// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using NUnit.Framework;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;

namespace VMware.ScriptRuntimeService.AdminEngine.Tests {
   internal class FormatThumbprintTests {

      [SetUp]
      public void Setup() {

      }

      [Test]
      [TestCase("85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2",
            "85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2")] // SHA1 : separator
      [TestCase("85:f0:76:ef:04:1c:9c:5e:74:2b:ff:fb:40:40:82:75:4b:f0:f4:b2",
            "85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2")] // SHA1 lowercase, : separator
      [TestCase("85F076EF041C9C5E742BFFFB404082754BF0F4B2",
            "85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2")] // SHA1 no separator
      [TestCase("85 F0 76 EF 04 1C 9C 5E 74 2B FF FB 40 40 82 75 4B F0 F4 B2",
            "85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2")] // SHA1 space separator
      [TestCase(" 85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2 ",
            "85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2")] // SHA1 leading & trailling whitespaces
      [TestCase("93:FC:6F:AD:2D:D8:2F:DE:6D:E1:4A:D1:65:D3:83:9D:5F:8E:B8:74:F8:6D:A9:5D:FB:05:CA:50:FD:C7:CD:23",
            "93:FC:6F:AD:2D:D8:2F:DE:6D:E1:4A:D1:65:D3:83:9D:5F:8E:B8:74:F8:6D:A9:5D:FB:05:CA:50:FD:C7:CD:23")] // SHA256
      [Parallelizable(ParallelScope.All)]
      public void Valid_Thumbprint_ReturnsWellFormattedThumbprint(string input, string expected) {
         // Arrage

         // Act
         string result = VCRegistrator.FormatThumbprint(input);

         // Assert
         Assert.AreEqual(expected, result);
      }

      [Test]
      [TestCase("85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4")] // invalid length / shorter
      [TestCase("85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:40:82:75:4B:F0:F4:B2:B3")] // invalid length / longer
      [TestCase("85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:H0:82:75:4B:F0:F4:B2")] // invalid symbol - H
      [TestCase("85:F0:76:EF:04:1C:9C:5E:74:2B:FF:FB:40:H0:82:P5:4B:F0:F4:B2")] // invalid symbol - H, P
      [TestCase("85/F0/76/EF/04/1C/9C/5E/74/2B/FF/FB/40/40/82/75/4B/F0/F4/B2")] // invalid separator - /
      [Parallelizable(ParallelScope.All)]
      public void Invalid_Thumbprint_FormatExceptionIsThrown(string input) {
         // Arrage

         // Act / Assert
         Assert.Catch<FormatException>(() => VCRegistrator.FormatThumbprint(input));
      }

      [Test]
      public void Null_Input_ArgumentNullExceptionExceptionIsThrown() {
         // Arrage
         string input = null;

         // Act / Assert
         Assert.Catch<ArgumentNullException>(() => VCRegistrator.FormatThumbprint(input));
      }

      [Test]
      public void Empty_Input_ArgumentNullExceptionExceptionIsThrown() {
         // Arrage
         string input = "";

         // Act / Assert
         Assert.Catch<ArgumentException>(() => VCRegistrator.FormatThumbprint(input));
      }
   }
}
