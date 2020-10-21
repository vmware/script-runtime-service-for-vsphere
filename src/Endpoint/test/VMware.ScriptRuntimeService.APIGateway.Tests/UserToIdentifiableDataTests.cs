// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.Authentication;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class UserToIdentifiableDataTests {
      [Test]
      public void AddContains() {
         // Arrange
         const string UserID1 = "user-id1";
         const string UserID2 = "user-id2";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         const string RunspaceID3 = "runspace-id3";
         var userRunspaces = new UserToIdentifiableData<string>();

         // Act
         userRunspaces.Add(UserID1, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID1, RunspaceID2, RunspaceID2);
         userRunspaces.Add(UserID2, RunspaceID3, RunspaceID3);

         // Assert
         Assert.IsTrue(userRunspaces.Contains(UserID1, RunspaceID1));
         Assert.IsTrue(userRunspaces.Contains(UserID1, RunspaceID2));
         Assert.IsTrue(userRunspaces.Contains(UserID2, RunspaceID3));
      }

      [Test]
      public void List() {
         // Arrange
         const string UserID = "user-id";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceData1 = "runspace-data1";
         const string RunspaceID2 = "runspace-id2";
         const string RunspaceData2 = "runspace-data2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID, RunspaceID1, RunspaceData1);
         userRunspaces.Add(UserID, RunspaceID2, RunspaceData2);

         // Act
         var listResult = userRunspaces.List(UserID);

         // Assert
         Assert.NotNull(listResult);
         Assert.AreEqual(2, listResult.Length);
         Assert.IsTrue(listResult.Contains(RunspaceData1));
         Assert.IsTrue(listResult.Contains(RunspaceData2));
      }

      [Test]
      public void GetData() {
         // Arrange
         const string UserID = "user-id";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceData1 = "runspace-data1";
         const string RunspaceID2 = "runspace-id2";
         const string RunspaceData2 = "runspace-data2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID, RunspaceID1, RunspaceData1);
         userRunspaces.Add(UserID, RunspaceID2, RunspaceData2);

         // Act
         var actualData = userRunspaces.GetData(UserID, RunspaceID2);

         // Assert
         Assert.NotNull(actualData);
         Assert.AreEqual(RunspaceData2, actualData);         
      }

      [Test]
      public void RemoveUser() {
         // Arrange
         const string UserID1 = "user-id1";
         const string UserID2 = "user-id2";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID1, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID2, RunspaceID2, RunspaceID2);

         // Act
         userRunspaces.RemoveUser(UserID2);

         // Assert
         Assert.IsFalse(userRunspaces.Contains(UserID2));
      }

      [Test]
      public void RemoveRunspace() {
         // Arrange
         const string UserID = "user-id";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID, RunspaceID2, RunspaceID2);

         // Act
         userRunspaces.RemoveData(UserID, RunspaceID1);

         // Assert
         var listResult = userRunspaces.List(UserID);
         Assert.NotNull(listResult);
         Assert.AreEqual(1, listResult.Length);
         Assert.IsFalse(listResult.Contains(RunspaceID1));
         Assert.IsTrue(listResult.Contains(RunspaceID2));
         Assert.IsTrue(userRunspaces.Contains(UserID, RunspaceID2));
         Assert.IsFalse(userRunspaces.Contains(UserID, RunspaceID1));
      }

      [Test]
      public void ListWhenNoUser() {
         // Arrange
         const string UserID1 = "user-id1";
         const string UserID2 = "user-id2";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID1, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID1, RunspaceID2, RunspaceID2);

         // Act
         var listResult = userRunspaces.List(UserID2);

         // Assert
         Assert.IsNull(listResult);
      }

      [Test]
      public void ContainsWhenNoUser() {
         // Arrange
         const string UserID1 = "user-id1";
         const string UserID2 = "user-id2";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID1, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID1, RunspaceID2, RunspaceID2);

         // Act
         var containsResult = userRunspaces.Contains(UserID2);

         // Assert
         Assert.IsFalse(containsResult);
      }

      [Test]
      public void RemoveUserWhenNoUser() {
         // Arrange
         const string UserID1 = "user-id1";
         const string UserID2 = "user-id2";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID1, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID1, RunspaceID2, RunspaceID2);

         // Act && Assert
         Assert.DoesNotThrow(() => userRunspaces.RemoveUser(UserID2));

         Assert.IsTrue(userRunspaces.Contains(UserID1));
         Assert.IsTrue(userRunspaces.Contains(UserID1, RunspaceID1));
         Assert.IsTrue(userRunspaces.Contains(UserID1, RunspaceID2));
      }


      [Test]
      public void RemoveRunspaceWhenNoUser() {
         // Arrange
         const string UserID1 = "user-id1";
         const string UserID2 = "user-id2";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID1, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID1, RunspaceID2, RunspaceID2);

         // Act && Assert
         Assert.DoesNotThrow(() => userRunspaces.RemoveData(UserID2, RunspaceID2));

         Assert.IsTrue(userRunspaces.Contains(UserID1));
         Assert.IsTrue(userRunspaces.Contains(UserID1, RunspaceID1));
         Assert.IsTrue(userRunspaces.Contains(UserID1, RunspaceID2));
      }

      [Test]
      public void RemoveRunspaceWhenNoRunspace() {
         // Arrange
         const string UserID1 = "user-id1";
         const string UserID2 = "user-id2";
         const string RunspaceID1 = "runspace-id1";
         const string RunspaceID2 = "runspace-id2";
         var userRunspaces = new UserToIdentifiableData<string>();
         userRunspaces.Add(UserID1, RunspaceID1, RunspaceID1);
         userRunspaces.Add(UserID2, RunspaceID2, RunspaceID1);

         // Act && Assert
         Assert.DoesNotThrow(() => userRunspaces.RemoveData(UserID1, RunspaceID2));

         Assert.IsTrue(userRunspaces.Contains(UserID1));
         Assert.IsTrue(userRunspaces.Contains(UserID2));
         Assert.IsTrue(userRunspaces.Contains(UserID1, RunspaceID1));
         Assert.IsTrue(userRunspaces.Contains(UserID2, RunspaceID2));
      }
   }
}
