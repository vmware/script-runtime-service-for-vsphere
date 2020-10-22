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
   public class NonceTests {
      [Test]
      public void Parse() {
         // Arrange
         var date = DateTimeOffset.Now.ToUnixTimeMilliseconds();
         var expected = $"{date}:text";

         // Act
         var actual = Nonce.FromString(expected);

         // Assert
         Assert.AreEqual(expected, actual.ToString());
      }

      [Test]
      public void EqualsCreateFromStringAndDate() {
         // Arrange
         var expected = Nonce.FromDate(DateTime.Now);
         var actual = Nonce.FromString(expected.ToString());

         // Act
         var result = actual.Equals(expected);

         // Assert
         Assert.IsTrue(result);
      }

      [Test]
      public void TestToString() {
         // Arrange
         var expected = "123:text";

         // Act
         var actual = Nonce.FromString(expected).ToString();

         // Assert
         Assert.AreEqual(expected, actual);
      }

      [Test]
      public void AppenderDiffers() {
         // Arrange
         var date = DateTime.Now;
         
         // Act
         var appender1 = Nonce.FromDate(date).ToString().Split(':')[1];
         var appender2 = Nonce.FromDate(date).ToString().Split(':')[1];

         // Assert
         Assert.AreNotEqual(appender1, appender2);
      }

      [Test]
      public void DateIsUnixTimeInUtc() {
         // Arrange
         var date = DateTime.Now;
         var nonce = Nonce.FromDate(date);

         var expected = ((DateTimeOffset)date.ToUniversalTime()).ToUnixTimeMilliseconds().ToString();

         // Act
         var actual = nonce.ToString().Split(':')[0];

         // Assert
         Assert.AreEqual(expected, actual);
      }

      [Test]
      public void ParseInvalidStringThrows() {
         // Arrange
         var noDelimiter = "1234";
         var noAppender = "1234:";
         var noDate = ":1234";
         var delimiterOnly = ":";
         var empty = string.Empty;
         string nullString = null;

         // Act && Assert
         Assert.Throws<AuthException>(() => { Nonce.FromString(noDelimiter); });
         Assert.Throws<AuthException>(() => { Nonce.FromString(noAppender); });
         Assert.Throws<AuthException>(() => { Nonce.FromString(noDate); });
         Assert.Throws<AuthException>(() => { Nonce.FromString(delimiterOnly); });
         Assert.Throws<AuthException>(() => { Nonce.FromString(empty); });
         Assert.Throws<AuthException>(() => { Nonce.FromString(nullString); });
      }

      [Test]
      public void CompareToSameDateNonce() {
         // Arrange
         var date = DateTime.Now;
         var nonce1 = Nonce.FromDate(date);
         var nonce2 = Nonce.FromDate(date);

         // Act
         var actual = nonce1.CompareTo(nonce2);

         // Assert
         Assert.AreEqual(0, actual);
      }

      [Test]
      public void CompareToOlderDateNonce() {
         // Arrange
         var date = DateTime.Now;
         var nonce1 = Nonce.FromDate(date);
         var nonce2 = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 1)));

         // Act
         var actual = nonce2.CompareTo(nonce1);

         // Assert
         Assert.IsTrue(actual < 0);
      }

      [Test]
      public void CompareToNewerDateNonce() {
         // Arrange
         var date = DateTime.Now;
         var nonce1 = Nonce.FromDate(date);
         var nonce2 = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 1)));

         // Act
         var actual = nonce1.CompareTo(nonce2);

         // Assert
         Assert.IsTrue(actual > 0);
      }
   }
}
