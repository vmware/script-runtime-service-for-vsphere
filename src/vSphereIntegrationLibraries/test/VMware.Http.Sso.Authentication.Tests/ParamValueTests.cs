// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VMware.Http.Sso.Authentication.Impl;

namespace VMware.Http.Sso.Authentication.Tests {
   public class ParamValueTests {
      [Test]
      public void PublicProperties() {
         // Arrange
         var excpectedKey = ParamValue.Param.signature;
         var expectedValue = "test-signature";

         // Act
         var actual = new ParamValue(excpectedKey, expectedValue);

         // Assert
         Assert.AreEqual(excpectedKey, actual.Key);
         Assert.AreEqual(expectedValue, actual.Value);
      }

      [Test]
      public void TestToString() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var expectedValue = "test-token";
         var paramValue = new ParamValue(excpectedKey, expectedValue);
         var expected = $"Param({excpectedKey}, {expectedValue})";
         

         // Act
         var actual = paramValue.ToString();

         // Assert
         Assert.AreEqual(expected, actual);
      }

      [Test]
      public void EqualsEqualParamValue() {
         // Arrange
         var key = ParamValue.Param.bodyhash;
         var value = "test-hash";
         var param1 = new ParamValue(key, value);
         var param2 = new ParamValue(key, value);

         // Act
         var actual = param2.Equals(param1);

         // Assert
         Assert.IsTrue(actual);
      }

      [Test]
      public void EqualsAgainstNull() {
         // Arrange
         var key = ParamValue.Param.bodyhash;
         var value = "test-hash";
         var param = new ParamValue(key, value);

         // Act
         var actual = param.Equals(null);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void EqualsAgainstDifferentKey() {
         // Arrange
         var key = ParamValue.Param.bodyhash;
         var value = "test-hash";
         var param1 = new ParamValue(ParamValue.Param.token, value);
         var param2 = new ParamValue(key, value);

         // Act
         var actual = param2.Equals(param1);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void EqualsAgainstDifferentValue() {
         // Arrange
         var key = ParamValue.Param.bodyhash;
         var value1 = "test-hash1";
         var value2 = "test-hash2";
         var param1 = new ParamValue(key, value1);
         var param2 = new ParamValue(key, value2);

         // Act
         var actual = param2.Equals(param1);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void TrimLeft() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var toTrim = "asdf";
         var expectedValue = "test-token";
         var paramValue = new ParamValue(excpectedKey, $"{toTrim}{expectedValue}");

         // Act
         var actual = paramValue.TrimLeft(toTrim.Length);

         // Assert
         Assert.AreEqual(expectedValue, actual.Value);
         Assert.AreEqual(excpectedKey, actual.Key);
      }

      [Test]
      public void TrimLeftWholeValue() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var value = "test-token";
         var paramValue = new ParamValue(excpectedKey, value);

         // Act
         var actual = paramValue.TrimLeft(value.Length);

         // Assert
         Assert.IsNull(actual);
      }

      [Test]
      public void TrimLeftMoreCharsThanValue() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var value = "test-token";
         var paramValue = new ParamValue(excpectedKey, value);

         // Act
         var actual = paramValue.TrimLeft(value.Length + 1);

         // Assert
         Assert.IsNull(actual);
      }

      [Test]
      public void TrimLeftZeroChars() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var expectedValue = "test-token";
         var paramValue = new ParamValue(excpectedKey, expectedValue);

         // Act
         var actual = paramValue.TrimLeft(0);

         // Assert
         Assert.AreEqual(expectedValue, actual.Value);
         Assert.AreEqual(excpectedKey, actual.Key);
      }

      [Test]
      public void First() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var expectedValue = "first-";
         var last = "test-token";
         var paramValue = new ParamValue(excpectedKey, $"{expectedValue}{last}");

         // Act
         var actual = paramValue.First(expectedValue.Length);

         // Assert
         Assert.AreEqual(expectedValue, actual.Value);
         Assert.AreEqual(excpectedKey, actual.Key);
      }

      [Test]
      public void FirstWholeValue() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var expectedValue = "test-token";
         var paramValue = new ParamValue(excpectedKey, expectedValue);

         // Act
         var actual = paramValue.First(expectedValue.Length);

         // Assert
         Assert.AreEqual(expectedValue, actual.Value);
         Assert.AreEqual(excpectedKey, actual.Key);
      }

      [Test]
      public void FirstMoreCharsThanValue() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var expectedValue = "test-token";
         var paramValue = new ParamValue(excpectedKey, expectedValue);

         // Act
         var actual = paramValue.First(expectedValue.Length + 1);

         // Assert
         Assert.AreEqual(expectedValue, actual.Value);
         Assert.AreEqual(excpectedKey, actual.Key);
      }

      [Test]
      public void FirstZeroChars() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var value = "test-token";
         var paramValue = new ParamValue(excpectedKey, value);

         // Act
         var actual = paramValue.First(0);

         // Assert
         Assert.IsNull(actual);
      }

      [Test]
      public void Concat() {
         // Arrange
         var excpectedKey = ParamValue.Param.token;
         var part1 = "test-";
         var part2 = "value";
         var expectedValue = $"{part1}{part2}";
         var paramValue1 = new ParamValue(excpectedKey, part1);
         var paramValue2 = new ParamValue(excpectedKey, part2);

         // Act
         var actual = paramValue1.Concat(paramValue2);

         // Assert
         Assert.AreEqual(expectedValue, actual.Value);
         Assert.AreEqual(excpectedKey, actual.Key);
      }
   }
}
