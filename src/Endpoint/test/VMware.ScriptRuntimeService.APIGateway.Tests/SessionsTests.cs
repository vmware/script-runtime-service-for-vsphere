// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Text;
using System.Threading;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.Authentication;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class SessionsTests {
      [TearDown]
      public void TearDown() {
         Sessions.Instance.Dispose();
      }

      [Test]
      public void ValidateAndUpdateActiveSession() {
         // Arrange
         var authzToken = new SessionToken {
            UserName = "administrator@vsphere.local",
            SessionId = "session-id1"
         };
         Sessions.Instance.RegisterSession(authzToken);

         // Act & Assert
         Assert.DoesNotThrow(() => Sessions.Instance.ValidateAndUpdateSession(authzToken));
      }

      [Test]
      public void ValidateAndUpdateExpiredSessionThrows() {
         // Arrange
         Sessions.Instance.IdleSessionValidPeriodSeconds = 1;
         var authzToken = new SessionToken {
            UserName = "administrator@vsphere.local",
            SessionId = "session-id1"
         };
         Sessions.Instance.RegisterSession(authzToken);

         // Act
         Thread.Sleep(2*1000);

         // Assert
         Assert.Throws<InvalidSessionException>(() => Sessions.Instance.ValidateAndUpdateSession(authzToken));
      }

      [Test]
      public void ValidateAndUpdateInvalidSessionThrows() {
         // Arrange
         Sessions.Instance.IdleSessionValidPeriodSeconds = 1;
         var authzToken = new SessionToken {
            UserName = "administrator@vsphere.local",
            SessionId = "session-id1"
         };

         // 

         // Act && Assert
         Assert.Throws<InvalidSessionException>(() => Sessions.Instance.ValidateAndUpdateSession(authzToken));
      }

      [Test]
      public void IsActiveReturnsCorretValues() {
         // Arrange
         var activeSession = "session-active";
         var inactiveSession = "session-inactive";
         Sessions.Instance.IdleSessionValidPeriodSeconds = 1;
         var authzToken1 = new SessionToken {
            UserName = "administrator@vsphere.local",
            SessionId = inactiveSession
         };

         var authzToken2 = new SessionToken {
            UserName = "administrator@vsphere.local",
            SessionId = activeSession
         };
         Sessions.Instance.RegisterSession(authzToken1);

         Thread.Sleep(2 * 1000);

         Sessions.Instance.RegisterSession(authzToken2);


         // Act
         var actualInactive = Sessions.Instance.IsActive(inactiveSession);
         var actualActive = Sessions.Instance.IsActive(activeSession);

         // Assert
         Assert.IsFalse(actualInactive);
         Assert.IsTrue(actualActive);
      }
   }
}
