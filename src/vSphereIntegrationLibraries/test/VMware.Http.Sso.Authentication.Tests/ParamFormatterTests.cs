// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using NUnit.Framework;
using VMware.Http.Sso.Authentication.Impl;

namespace VMware.Http.Sso.Authentication.Tests {
   public class ParamFormatterTests {
      [Test]
      public void ParseValidNonceParameter() {
         // Arrange
         var expectedKey = ParamValue.Param.nonce;
         var expectedValue = "137131200:dj83hs9s";
         var paramValue = $"{expectedKey}=\"{expectedValue}\"";

         // Act
         var actual = ParamFormatter.Parse(paramValue);

         // Assert
         Assert.AreEqual(expectedKey, actual.Key);
         Assert.AreEqual(expectedValue, actual.Value);
      }

      [Test]
      public void ParseTokenParameter() {
         // Arrange
         var expectedKey = ParamValue.Param.token;
         var expectedValue = "k9kbtCIy0CkI3/FEfpS/oIDjk6k=";
         var paramValue = $"{expectedKey}=\"{expectedValue}\"";

         // Act
         var actual = ParamFormatter.Parse(paramValue);

         // Assert
         Assert.AreEqual(expectedKey, actual.Key);
         Assert.AreEqual(expectedValue, actual.Value);
      }

      [Test]
      public void ParseBodyHashParameter() {
         // Arrange
         var expectedKey = ParamValue.Param.bodyhash;
         var expectedValue = "k9kbtCIy0CkI3/FEfpS/oIDjk6k=";
         var paramValue = $"{expectedKey}=\"{expectedValue}\"";

         // Act
         var actual = ParamFormatter.Parse(paramValue);

         // Assert
         Assert.AreEqual(expectedKey, actual.Key);
         Assert.AreEqual(expectedValue, actual.Value);
      }

      [Test]
      public void ParseSignatureParameter() {
         // Arrange
         var expectedKey = ParamValue.Param.signature;
         var expectedValue = "k9kbtCIy0CkI3FEfpoIDjk6k";
         var paramValue = $"{expectedKey}=\"{expectedValue}\"";

         // Act
         var actual = ParamFormatter.Parse(paramValue);

         // Assert
         Assert.AreEqual(expectedKey, actual.Key);
         Assert.AreEqual(expectedValue, actual.Value);
      }

      [Test]
      public void ParseSignatureAlgorithmParameter() {
         // Arrange
         var expectedKey = ParamValue.Param.signature_alg;
         var expectedValue = "RSA-SHA256";
         var paramValue = $"{expectedKey}=\"{expectedValue}\"";

         // Act
         var actual = ParamFormatter.Parse(paramValue);

         // Assert
         Assert.AreEqual(expectedKey, actual.Key);
         Assert.AreEqual(expectedValue, actual.Value);
      }
   }
}
