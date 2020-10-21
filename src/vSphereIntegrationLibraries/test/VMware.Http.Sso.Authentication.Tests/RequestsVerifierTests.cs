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
   public class RequestsVerifierTests {
      [Test]
      public void DoesntThrowOnRequestsInAgeAndClockTolerance() {
         // Arrange
         var date = DateTime.Now;
         var nonce1 = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 1)));
         var nonce2 = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 2)));
         var nonce3 = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 3)));
         var nonce4 = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 4)));
         var nonce5 = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 5)));

         var verifier = new RequestsVerifier(2, 4);

         // Act & Assert
         Assert.DoesNotThrow(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce1));
         Assert.DoesNotThrow(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce2));
         Assert.DoesNotThrow(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce3));
         Assert.DoesNotThrow(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce4));
         Assert.DoesNotThrow(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce5));
      }

      [Test]
      public void ThrowsOnRepeat() {
         // Arrange
         var date = DateTime.Now;
         var nonce = Nonce.FromDate(date);
         

         var verifier = new RequestsVerifier(2, 4);

         // Act & Assert
         Assert.DoesNotThrow(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce));
         Assert.Throws<AuthException>(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce));
      }

      [Test]
      public void ThrowsOnOld() {
         // Arrange
         var date = DateTime.Now;
         var nonce = Nonce.FromDate(date.Subtract(new TimeSpan(0, 0, 0, 6)));


         var verifier = new RequestsVerifier(1, 4);

         // Act & Assert
         Assert.Throws<AuthException>(() => verifier.VerifyAgeAndRepeatOnNewRequest(nonce));
      }
   }
}
