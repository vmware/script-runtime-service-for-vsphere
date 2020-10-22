// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using Castle.DynamicProxy.Contributors;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Runspace;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl;
using VMware.ScriptRuntimeService.APIGateway.Sts;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Sts.SamlToken;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class MultiTenantRunspaceProviderTests {
      class MockRunspaceInfo : IRunspaceInfo {
         public string Id { get; set; }
         public IPEndPoint Endpoint { get; set; }

         public RunspaceCreationState CreationState { get; set; }

         public RunspaceProviderException CreationError { get; set; }
      }

      private ILoggerFactory MockLoggerFactory() {
         var result = new Mock<ILoggerFactory>();
         result.Setup(x => x.CreateLogger(typeof(MultiTenantRunspaceProvider).FullName))
            .Returns(new Mock<ILogger>().Object);
         return result.Object;
      }
      
      private Mock<IRunspaceProvider> MockRunspaceProvider(string runspaceId, string endpoint, Mock<IRunspaceProvider> runspaceProviderMockClass) {
         // Mock IRunspaceProvider.Create
         runspaceProviderMockClass.Setup(m => m.StartCreate()).Returns(
            new MockRunspaceInfo {
               Id = runspaceId,
               Endpoint = new IPEndPoint(IPAddress.Parse(endpoint), 80),
               CreationState = RunspaceCreationState.Ready
            });

         runspaceProviderMockClass.Setup(m => m.WaitCreateCompletion(It.IsAny<IRunspaceInfo>())).Returns(
            new MockRunspaceInfo {
               Id = runspaceId,
               Endpoint = new IPEndPoint(IPAddress.Parse(endpoint), 80),
               CreationState = RunspaceCreationState.Ready
            });

         // Mock IRunspaceProvider.Get
         runspaceProviderMockClass.Setup(m => m.Get(It.IsAny<string>())).Returns(
            new MockRunspaceInfo {
               Id = runspaceId
            });

         // Mock IRunspaceProvider.Kill
         runspaceProviderMockClass.Setup(m => m.Kill(It.IsAny<string>()));

         return runspaceProviderMockClass;
      }

      private MultiTenantRunspaceProvider _multiTenantRunspaceProvider;

      [TearDown]
      public void SetUp() {
         _multiTenantRunspaceProvider?.Dispose();
         _multiTenantRunspaceProvider = null;
      }

      [Test]
      public void CreateRunspaceWithoutConnectVcScript() {
         // Arrange
         const string userId = "UserID";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         const string runspaceIp = "127.0.0.1";
         var testStartTime = DateTime.Now;
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         _multiTenantRunspaceProvider = 
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);

         // Act
         var actual =_multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);

         // Assert
         runspaceProviderMockClass.Verify(mock => mock.StartCreate(), Times.Once());         
         Assert.AreEqual(runspaceId, actual.Id);
         Assert.NotNull(actual.Endpoint);
         Assert.AreEqual(runspaceIp, actual.Endpoint.Address.ToString());
         Assert.AreEqual(RunspaceState.Ready, actual.State);
         Assert.AreEqual(false, actual.RunVcConnectionScript);
         Assert.AreEqual(null, actual.Name);
         Assert.AreEqual(null, actual.ErrorDetails);
         Assert.AreEqual(null, actual.VcConnectionScriptId);
         Assert.IsTrue(actual.CreationTime >= testStartTime && actual.CreationTime <= DateTime.Now);
      }

      [Test]
      public void CreateNamedRunspaceWithoutConnectVcScript() {
         // Arrange
         const string userId = "UserID";
         const string runspaceId = "RunspaceId";         
         const string sessionId = "SessionId";
         const string runspaceName = "TestRunspaceName";
         var testStartTime = DateTime.Now;
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);

         // Act
         var actual = _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, runspaceName, false, null, null);

         // Assert
         runspaceProviderMockClass.Verify(mock => mock.StartCreate(), Times.Once());
         Assert.AreEqual(runspaceId, actual.Id);
         Assert.AreEqual(RunspaceState.Ready, actual.State);
         Assert.AreEqual(false, actual.RunVcConnectionScript);
         Assert.AreEqual(runspaceName, actual.Name);
         Assert.AreEqual(null, actual.ErrorDetails);
         Assert.AreEqual(null, actual.VcConnectionScriptId);
         Assert.IsTrue(actual.CreationTime >= testStartTime && actual.CreationTime <= DateTime.Now);
      }

      [Test]
      public void CreateRunspaceStartingVcConnectionProcessingSsoRequests() {
         // Arrange
         const string userId = "UserID";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         const string runspaceName = "TestRunspaceName";
         var testStartTime = DateTime.Now;
         
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;

         var rawSamlToken= new XmlDocument();
         rawSamlToken.LoadXml("<saml />");

         var samlTokenMock = new Mock<ISamlToken>();
         samlTokenMock.Setup(x => x.RawXmlElement).Returns(rawSamlToken.DocumentElement);

         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         sessionTokenMock.Setup(x => x.HoKSamlToken).Returns(samlTokenMock.Object);

         var stsClientMock = new Mock<ISolutionStsClient>();
         stsClientMock
            .Setup(x => x.IssueBearerTokenBySolutionToken(rawSamlToken.DocumentElement))
            .Callback(() => { Thread.Sleep(200); })
            .Returns(rawSamlToken.DocumentElement);

         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);

         // Act
         var actual = _multiTenantRunspaceProvider
            .StartCreate(userId, sessionTokenMock.Object, runspaceName, true, stsClientMock.Object, "vcEndpoint");

         // Assert
         runspaceProviderMockClass.Verify(mock => mock.StartCreate(), Times.Once());
         Assert.AreEqual(runspaceId, actual.Id);
         Assert.AreEqual(RunspaceState.Creating, actual.State);
         Assert.AreEqual(true, actual.RunVcConnectionScript);
         Assert.AreEqual(runspaceName, actual.Name);
         Assert.AreEqual(null, actual.ErrorDetails);
         Assert.AreEqual(null, actual.VcConnectionScriptId);
         Assert.IsTrue(actual.CreationTime >= testStartTime && actual.CreationTime <= DateTime.Now);
      }

      [Test]
      public void CreateRunspaceStartingVcConnectionSsoError() {
         // Arrange
         const string userId = "UserID";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         const string runspaceName = "TestRunspaceName";
         const string ssoError = "Acquire Bearer Token Fails";
         var testStartTime = DateTime.Now;

         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;

         var rawSamlToken = new XmlDocument();
         rawSamlToken.LoadXml("<saml />");

         var samlTokenMock = new Mock<ISamlToken>();
         samlTokenMock.Setup(x => x.RawXmlElement).Returns(rawSamlToken.DocumentElement);

         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         sessionTokenMock.Setup(x => x.HoKSamlToken).Returns(samlTokenMock.Object);

         var stsClientMock = new Mock<ISolutionStsClient>();
         stsClientMock
            .Setup(x => x.IssueBearerTokenBySolutionToken(rawSamlToken.DocumentElement))
            .Callback(() => { throw new Exception(ssoError); })
            .Returns(rawSamlToken.DocumentElement);

         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);

         // Act
         var actual = _multiTenantRunspaceProvider
            .StartCreate(userId, sessionTokenMock.Object, runspaceName, true, stsClientMock.Object, "vcEndpoint");
         //   Give small amount of time internal thread to start and SSO Error to happen
         Thread.Sleep(100);
         actual = _multiTenantRunspaceProvider.Get(userId, runspaceId);

         // Assert         
         Assert.AreEqual(runspaceId, actual.Id);
         Assert.AreEqual(RunspaceState.Error, actual.State);
         Assert.AreEqual(true, actual.RunVcConnectionScript);
         Assert.AreEqual(runspaceName, actual.Name);
         Assert.NotNull(actual.ErrorDetails);
         Assert.AreEqual(ssoError, actual.ErrorDetails.Message);
         Assert.AreEqual(null, actual.VcConnectionScriptId);
         Assert.IsTrue(actual.CreationTime >= testStartTime && actual.CreationTime <= DateTime.Now);
      }

      [Test]
      public void Get() {
         // Arrange
         const string userId = "UserID";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);
         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);

         // Act
         var actual = _multiTenantRunspaceProvider.Get(userId, runspaceId);

         // Assert
         runspaceProviderMockClass.Verify(
            mock => mock.Get(
               It.Is<string>(s => s == runspaceId)),
              Times.Exactly(1));
         Assert.AreEqual(runspaceId, actual.Id);
      }

      [Test]
      public void List() {
         // Arrange
         const string userId1 = "UserID1";
         const string userId2 = "UserID2";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);
         _multiTenantRunspaceProvider.StartCreate(userId1, sessionTokenMock.Object, null, false, null, null);
         _multiTenantRunspaceProvider.StartCreate(userId1, sessionTokenMock.Object, null, false, null, null);
         _multiTenantRunspaceProvider.StartCreate(userId2, sessionTokenMock.Object, null, false, null, null);

         // Act
         var actualUser1 = _multiTenantRunspaceProvider.List(userId1);
         var actualUser2 = _multiTenantRunspaceProvider.List(userId2);

         // Assert
         runspaceProviderMockClass.Verify(
            mock => mock.Get(
               It.IsAny<string>()),
            Times.Exactly(3));
         Assert.AreEqual(2, actualUser1.Count());
         Assert.AreEqual(1, actualUser2.Count());
      }

      [Test]
      public void ListWhenNoRunspaces() { }

      [Test]
      public void Kill() {
         // Arrange
         const string userId1 = "UserID1";
         const string userId2 = "UserID2";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);
         _multiTenantRunspaceProvider.StartCreate(userId1, sessionTokenMock.Object, null, false, null, null);
         _multiTenantRunspaceProvider.StartCreate(userId1, sessionTokenMock.Object, null, false, null, null);
         _multiTenantRunspaceProvider.StartCreate(userId2, sessionTokenMock.Object, null, false, null, null);

         // Act
         _multiTenantRunspaceProvider.Kill(userId1, runspaceId);

         // Assert
         runspaceProviderMockClass.Verify(
            mock => mock.Kill(
               It.Is<string>(s => s == runspaceId)),
            Times.Exactly(1));
      }

      [Test]
      public void CreateWhenInvalidUser() {
         // Arrange
         const string userId = null;
         const string sessionId = "SessionId";
         const string runspaceId = "RunspaceId";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);

         // Act && Assert
         Assert.Throws<RunspaceProviderException>(
            () => _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null));
      }

      [Test]
      public void GetWhenInvalidUser() {
         // Arrange
         const string invalidUserId = "";
         const string userId = "UserId";
         const string sessionId = "SessionId";
         const string runspaceId = "RunspaceId";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);
         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);

         // Act && Assert
         Assert.Throws<RunspaceProviderException>(
            () => _multiTenantRunspaceProvider.Get(invalidUserId, runspaceId));
      }

      [Test]
      public void ListWhenInvalidUser() {
         // Arrange
         const string invalidUserId = "";
         const string userId = "UserId";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);
         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);

         // Act && Assert
         Assert.Throws<RunspaceProviderException>(
            () => _multiTenantRunspaceProvider.List(invalidUserId));
      }

      [Test]
      public void KillWhenInvalidUser() {
         // Arrange
         const string invalidUserId = "";
         const string userId = "UserId";
         const string runspaceId = "RunspaceId";
         const string sessionId = "SessionId";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);
         var runspaceProviderMockClass = new Mock<IRunspaceProvider>();
         var runspaceProviderMock = MockRunspaceProvider(runspaceId, "127.0.0.1", runspaceProviderMockClass).Object;
         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               runspaceProviderMock);
         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);

         // Act && Assert
         Assert.Throws<RunspaceProviderException>(
            () => _multiTenantRunspaceProvider.Kill(invalidUserId, runspaceId));
      }

      [Test]
      public void CleanupCallsRunspaceProviderKillAndRemovesLocalDataForNeededRunspaces() {
         // Arrange
         const string userId = "UserID";
         const string sessionId = "session-id-1";
         const string runspaceId1 = "runspace-id-1";
         const string runspaceId2 = "runspace-id-2";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);

         // Mock Runspaces Info
         var runspace1 = new Mock<IRunspaceInfo>();
         runspace1.Setup(r => r.Id).Returns(runspaceId1);

         var runspace2 = new Mock<IRunspaceInfo>();
         runspace2.Setup(r => r.Id).Returns(runspaceId2);

         // Mock RunspaceProvider
         var rsProvider = new Mock<IRunspaceProvider>();
         rsProvider.Setup(p => p.Kill(It.IsAny<string>()));
         int callCount = 0;
         rsProvider.Setup(p => p.StartCreate()).Returns(() => {
            if (callCount == 0) {
               callCount = 1;
               return runspace1.Object;
            } else {
               return runspace2.Object;
            }
         });
         rsProvider.Setup(p => p.List()).Returns(new []{runspace1.Object, runspace2.Object});
         rsProvider.Setup(p => p.Get(runspaceId1)).Returns(runspace1.Object);
         rsProvider.Setup(p => p.Get(runspaceId2)).Returns(runspace2.Object);

         // Mock RunsapcesStatsMonitor
         var runspacesStatsMonitor = new Mock<IRunspacesStatsMonitor>();
         runspacesStatsMonitor.Setup(m => m.EvaluateRunspacesToRemove()).Returns(new[] {runspaceId1});
         runspacesStatsMonitor.Setup(m => m.Unregister(It.IsAny<string>()));

         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               rsProvider.Object,
               Int32.MaxValue, 
               Int32.MaxValue, 
               Int32.MaxValue, 
               runspacesStatsMonitor.Object);

         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);
         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);

         // Act
         _multiTenantRunspaceProvider.Cleanup();

         // Assert

         // Provider Kill is called
         rsProvider.Verify(mock => mock.Kill(It.IsAny<string>()), Times.AtLeastOnce());

         // Stats Monitor Unregister is called
         runspacesStatsMonitor.Verify(mock => mock.Unregister(It.IsAny<string>()), Times.Exactly(2));
         
         // Only Runspace2 is left
         var userRunspaces = _multiTenantRunspaceProvider.List(userId);
         Assert.AreEqual(1, userRunspaces.Count());
         Assert.AreEqual(runspaceId2, userRunspaces.First().Id);
      }

      [Test]
      public void CleanupRemovesLocalDataForDisappearedRunspaces() {
         // Arrange
         const string userId = "UserID";
         const string sessionId = "session-id-1";
         const string runspaceId1 = "runspace-id-1";
         const string runspaceId2 = "runspace-id-2";
         var sessionTokenMock = new Mock<ISessionToken>();
         sessionTokenMock.Setup(x => x.SessionId).Returns(sessionId);

         // Mock Runspaces Info
         var runspace1 = new Mock<IRunspaceInfo>();
         runspace1.Setup(r => r.Id).Returns(runspaceId1);

         var runspace2 = new Mock<IRunspaceInfo>();
         runspace2.Setup(r => r.Id).Returns(runspaceId2);

         // Mock RunspaceProvider
         var rsProvider = new Mock<IRunspaceProvider>();
         rsProvider.Setup(p => p.Kill(It.IsAny<string>()));
         int callCount = 0;
         rsProvider.Setup(p => p.StartCreate()).Returns(
            () => {
               if (callCount == 0) {
                  callCount = 1;
                  return runspace1.Object;
               } else {
                  return runspace2.Object;
               }
            });
         rsProvider.Setup(p => p.List()).Returns(new[] {runspace1.Object});
         rsProvider.Setup(p => p.Get(runspaceId1)).Returns(runspace1.Object);
         rsProvider.Setup(p => p.Get(runspaceId2)).Returns(runspace2.Object);

         // Mock RunsapcesStatsMonitor
         var runspacesStatsMonitor = new Mock<IRunspacesStatsMonitor>();
         runspacesStatsMonitor.Setup(m => m.EvaluateRunspacesToRemove()).Returns(new string[]{});

         runspacesStatsMonitor.Setup(m => m.GetRegisteredRunspaces()).Returns(new []{runspaceId1, runspaceId2});
         runspacesStatsMonitor.Setup(m => m.Unregister(runspaceId2));

         _multiTenantRunspaceProvider =
            new MultiTenantRunspaceProvider(
               MockLoggerFactory(),
               rsProvider.Object,
               Int32.MaxValue,
               Int32.MaxValue,
               Int32.MaxValue,
               runspacesStatsMonitor.Object);

         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);
         _multiTenantRunspaceProvider.StartCreate(userId, sessionTokenMock.Object, null, false, null, null);

         // Act
         _multiTenantRunspaceProvider.Cleanup();

         // Assert
         // Stats Monitor Unregister is called
         runspacesStatsMonitor.Verify(mock => mock.Unregister(runspaceId2), Times.AtLeastOnce);

         // Only Runspace2 is left
         var userRunspaces = _multiTenantRunspaceProvider.List(userId);
         Assert.AreEqual(1, userRunspaces.Count());
         Assert.AreEqual(runspaceId1, userRunspaces.First().Id);
      }
   }
}
