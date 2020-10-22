// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics;
using Moq;
using VMware.ScriptRuntimeService.DockerRunspaceProvider;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class RunspaceStatisticsProvidersTests {
      [Test]
      public void RunspaceIsAcriveWhenLastScriptEndDateIsNull() {
         // Arrange
         var lastScriptResultMock = new Mock<IScriptExecutionResult>();

         lastScriptResultMock.Setup(r => r.StarTime).Returns(DateTime.Now - TimeSpan.FromSeconds(30));
         lastScriptResultMock.Setup(r => r.EndTime).Returns(() => null);
         lastScriptResultMock.Setup(r => r.State).Returns(ScriptState.Running);

         var runspaceClientMock = new Mock<IRunspace>();
         runspaceClientMock.Setup(c => c.GetLastScript()).Returns(lastScriptResultMock.Object);

         var runspaceClientFactoryMock = new Mock<IRunspaceClientFactory>();
         runspaceClientFactoryMock.Setup(cf => cf.Create(It.IsAny<IPEndPoint>())).Returns(runspaceClientMock.Object);

         var runspaceInfoMock = new Mock<IRunspaceInfo>();
         runspaceInfoMock.Setup(r => r.Endpoint).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));

         var actievIdleProvider = new ActiveIdleInfoProvider(
            runspaceInfoMock.Object, 
            runspaceClientFactoryMock.Object);

         var testStats = new RunspaceStats("", new Mock<IRunspaceSessionInfoProvider>().Object, actievIdleProvider);

         // Act
         testStats.Refresh();

         // Assert
         Assert.IsTrue(testStats.IsActive);
      }

      [Test]
      public void RunspaceIsNotAcriveWhenLastScriptEndDateIsLessThanNow() {
         // Arrange
         var lastScriptResultMock = new Mock<IScriptExecutionResult>();

         lastScriptResultMock.Setup(r => r.StarTime).Returns(DateTime.Now - TimeSpan.FromSeconds(30));
         lastScriptResultMock.Setup(r => r.EndTime).Returns(DateTime.Now - TimeSpan.FromSeconds(10));
         lastScriptResultMock.Setup(r => r.State).Returns(ScriptState.Canceled);

         var runspaceClientMock = new Mock<IRunspace>();
         runspaceClientMock.Setup(c => c.GetLastScript()).Returns(lastScriptResultMock.Object);

         var runspaceClientFactoryMock = new Mock<IRunspaceClientFactory>();
         runspaceClientFactoryMock.Setup(cf => cf.Create(It.IsAny<IPEndPoint>())).Returns(runspaceClientMock.Object);

         var actievIdleProvider = new ActiveIdleInfoProvider(
            new Mock<IRunspaceInfo>().Object,
            runspaceClientFactoryMock.Object);

         var testStats = new RunspaceStats("", new Mock<IRunspaceSessionInfoProvider>().Object, actievIdleProvider);

         // Act
         testStats.Refresh();

         // Assert
         Assert.IsFalse(testStats.IsActive);
      }

      [Test]
      public void RunspaceActiveTimeIsZeroWhenLastScriptIsNotRunning() {
         // Arrange
         var lastScriptResultMock = new Mock<IScriptExecutionResult>();

         lastScriptResultMock.Setup(r => r.StarTime).Returns(DateTime.Now - TimeSpan.FromSeconds(30));
         lastScriptResultMock.Setup(r => r.EndTime).Returns(DateTime.Now + TimeSpan.FromSeconds(10));
         lastScriptResultMock.Setup(r => r.State).Returns(ScriptState.Success);

         var runspaceClientMock = new Mock<IRunspace>();
         runspaceClientMock.Setup(c => c.GetLastScript()).Returns(lastScriptResultMock.Object);

         var runspaceClientFactoryMock = new Mock<IRunspaceClientFactory>();
         runspaceClientFactoryMock.Setup(cf => cf.Create(It.IsAny<IPEndPoint>())).Returns(runspaceClientMock.Object);

         var actievIdleProvider = new ActiveIdleInfoProvider(
            new Mock<IRunspaceInfo>().Object,
            runspaceClientFactoryMock.Object);

         var testStats = new RunspaceStats("", new Mock<IRunspaceSessionInfoProvider>().Object, actievIdleProvider);

         // Act
         testStats.Refresh();

         // Assert
         Assert.AreEqual(0, testStats.ActiveTimeSeconds);
      }

      [Test]
      public void RunspaceIdleTimeIsZeroWhenLastScriptIsRunning() {
         // Arrange
         var lastScriptResultMock = new Mock<IScriptExecutionResult>();

         lastScriptResultMock.Setup(r => r.StarTime).Returns(DateTime.Now - TimeSpan.FromSeconds(30));
         lastScriptResultMock.Setup(r => r.EndTime).Returns(DateTime.Now + TimeSpan.FromSeconds(10));
         lastScriptResultMock.Setup(r => r.State).Returns(ScriptState.Running);

         var runspaceClientMock = new Mock<IRunspace>();
         runspaceClientMock.Setup(c => c.GetLastScript()).Returns(lastScriptResultMock.Object);

         var runspaceClientFactoryMock = new Mock<IRunspaceClientFactory>();
         runspaceClientFactoryMock.Setup(cf => cf.Create(It.IsAny<IPEndPoint>())).Returns(runspaceClientMock.Object);

         var actievIdleProvider = new ActiveIdleInfoProvider(
            new Mock<IRunspaceInfo>().Object,
            runspaceClientFactoryMock.Object);

         var testStats = new RunspaceStats("", new Mock<IRunspaceSessionInfoProvider>().Object, actievIdleProvider);

         // Act
         testStats.Refresh();

         // Assert
         Assert.AreEqual(0, testStats.IdleTimeSeconds);
      }

      [Test]
      public void RunspaceIdleTimeIsIsValidWhenLastScriptIsNotRunning() {
         // Arrange
         var lastScriptResultMock = new Mock<IScriptExecutionResult>();

         var now = DateTime.Now;

         lastScriptResultMock.Setup(r => r.StarTime).Returns(now - TimeSpan.FromSeconds(30));
         lastScriptResultMock.Setup(r => r.EndTime).Returns(now  - TimeSpan.FromSeconds(10));
         lastScriptResultMock.Setup(r => r.State).Returns(ScriptState.Success);

         var runspaceClientMock = new Mock<IRunspace>();
         runspaceClientMock.Setup(c => c.GetLastScript()).Returns(lastScriptResultMock.Object);

         var runspaceClientFactoryMock = new Mock<IRunspaceClientFactory>();
         runspaceClientFactoryMock.Setup(cf => cf.Create(It.IsAny<IPEndPoint>())).Returns(runspaceClientMock.Object);

         var runspaceInfoMock = new Mock<IRunspaceInfo>();
         runspaceInfoMock.Setup(r => r.Endpoint).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));

         var actievIdleProvider = new ActiveIdleInfoProvider(
            runspaceInfoMock.Object,
            runspaceClientFactoryMock.Object);

         var testStats = new RunspaceStats("", new Mock<IRunspaceSessionInfoProvider>().Object, actievIdleProvider);

         // Act
         testStats.Refresh();

         // Assert
         Assert.AreEqual(10, testStats.IdleTimeSeconds);
      }

      [Test]
      public void RunspaceActiveTimeIsIsValidWhenLastScriptIsRunning() {
         // Arrange
         var lastScriptResultMock = new Mock<IScriptExecutionResult>();

         lastScriptResultMock.Setup(r => r.StarTime).Returns(DateTime.Now - TimeSpan.FromSeconds(30));
         lastScriptResultMock.Setup(r => r.EndTime).Returns(() => null);
         lastScriptResultMock.Setup(r => r.State).Returns(ScriptState.Running);

         var runspaceClientMock = new Mock<IRunspace>();
         runspaceClientMock.Setup(c => c.GetLastScript()).Returns(lastScriptResultMock.Object);

         var runspaceClientFactoryMock = new Mock<IRunspaceClientFactory>();
         runspaceClientFactoryMock.Setup(cf => cf.Create(It.IsAny<IPEndPoint>())).Returns(runspaceClientMock.Object);

         var runspaceInfoMock = new Mock<IRunspaceInfo>();
         runspaceInfoMock.Setup(r => r.Endpoint).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));

         var actievIdleProvider = new ActiveIdleInfoProvider(
            runspaceInfoMock.Object,
            runspaceClientFactoryMock.Object);

         var testStats = new RunspaceStats("", new Mock<IRunspaceSessionInfoProvider>().Object, actievIdleProvider);

         // Act
         testStats.Refresh();

         // Assert
         Assert.AreEqual(30, testStats.ActiveTimeSeconds);
      }

      [Test]
      public void RunspaceIdleTimeIsIsValidWhenLastScriptIsNull() {
         // Arrange
         var runspaceClientMock = new Mock<IRunspace>();
         runspaceClientMock.Setup(c => c.GetLastScript()).Returns(() => null);

         var runspaceClientFactoryMock = new Mock<IRunspaceClientFactory>();
         runspaceClientFactoryMock.Setup(cf => cf.Create(It.IsAny<IPEndPoint>())).Returns(runspaceClientMock.Object);

         var actievIdleProvider = new ActiveIdleInfoProvider(
            new Mock<IRunspaceInfo>().Object,
            runspaceClientFactoryMock.Object);

         var testStats = new RunspaceStats("", new Mock<IRunspaceSessionInfoProvider>().Object, actievIdleProvider);

         // Act
         Thread.Sleep(new TimeSpan(0, 0, 2));
         testStats.Refresh();

         // Assert
         Assert.AreEqual(2, testStats.IdleTimeSeconds);
         Assert.AreEqual(0, testStats.ActiveTimeSeconds);
      }

      [Test]
      public void HasActiveSessionIsTrueWhenSessionIsActive() {
         // Arrange
         var sessionInfoProviderMock = new Mock<IRunspaceSessionInfoProvider>();
         sessionInfoProviderMock.Setup(s => s.IsActive).Returns(true);

         var testStats = new RunspaceStats(
            "",
            sessionInfoProviderMock.Object, 
            new Mock<IActiveIdleInfoProvider>().Object);

         // Act
         testStats.Refresh();

         // Assert
         Assert.IsTrue(testStats.HasActiveSession);
      }

      [Test]
      public void HasActiveSessionIsFalseWhenSessionIsNotActive() {
         // Arrange
         var sessionInfoProviderMock = new Mock<IRunspaceSessionInfoProvider>();
         sessionInfoProviderMock.Setup(s => s.IsActive).Returns(false);

         var testStats = new RunspaceStats(
            "",
            sessionInfoProviderMock.Object,
            new Mock<IActiveIdleInfoProvider>().Object);

         // Act
         testStats.Refresh();

         // Assert
         Assert.IsFalse(testStats.HasActiveSession);
      }

      [Test]
      public void HasActiveSessionIsFalseWhenNoSession() {
         // Arrange
         var sessionInfoProviderMock = new Mock<IRunspaceSessionInfoProvider>();
         sessionInfoProviderMock.Setup(s => s.SessionId).Returns("invalid");

         var testStats = new RunspaceStats(
            "",
            sessionInfoProviderMock.Object,
            new Mock<IActiveIdleInfoProvider>().Object);

         // Act
         testStats.Refresh();

         // Assert
         Assert.IsFalse(testStats.HasActiveSession);
      }


      [Test]
      public void RunspaceStatsReadsDataFromAllProviders() {
         // Arrange
         var sessionInfoProviderMock = new Mock<IRunspaceSessionInfoProvider>();
         sessionInfoProviderMock.Setup(s => s.IsActive).Returns(true);

         var activeIdleInfoProvider = new Mock<IActiveIdleInfoProvider>();
         activeIdleInfoProvider.Setup(s => s.IsActive).Returns(true);

         var testStats = new RunspaceStats(
            "",
            sessionInfoProviderMock.Object,
            activeIdleInfoProvider.Object);

         // Act
         testStats.Refresh();

         // Assert
         Assert.IsTrue(testStats.HasActiveSession);
         Assert.IsTrue(testStats.IsActive);
      }
   }
}
