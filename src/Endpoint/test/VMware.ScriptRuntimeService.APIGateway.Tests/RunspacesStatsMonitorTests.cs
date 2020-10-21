// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.APIGateway.Runspace;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class RunspacesStatsMonitorTests
   {
      [Test]
      public void IsCreateNewAllowedReturnsFalseWhenMaximumRunspacesReached() {
         // Arrange
         var testMonitor = new RunspacesStatsMonitor(
            2,
            10,
            10,
            Mock.Of<IRunspaceStatsFactory>(),
            new RemoveExpiredIdleRunspaceRuleFactory(),
            new RemoveExpiredActiveRunspaceRuleFactory(),
            new RunspaceRetentionPolicy(new IRunspaceRetentionRule[] { }));

         testMonitor.Register(Mock.Of<IRunspaceInfo>(), string.Empty);
         testMonitor.Register(Mock.Of<IRunspaceInfo>(), string.Empty);

         // Act
         var actual = testMonitor.IsCreateNewRunspaceAllowed();

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void IsCreateNewAllowedReturnsTrueWhenMaximumRunspacesReached() {
         // Arrange
         var testMonitor = new RunspacesStatsMonitor(
            2,
            10,
            10,
            Mock.Of<IRunspaceStatsFactory>(),
            new RemoveExpiredIdleRunspaceRuleFactory(),
            new RemoveExpiredActiveRunspaceRuleFactory(),
            new RunspaceRetentionPolicy(new IRunspaceRetentionRule[] { }));

         testMonitor.Register(Mock.Of<IRunspaceInfo>(), string.Empty);

         // Act
         var actual = testMonitor.IsCreateNewRunspaceAllowed();

         // Assert
         Assert.IsTrue(actual);
      }

      [Test]
      public void RegisterAddsRunspacesForMonitoring() {
         // Arrange
         var runspaceId1 = "runspace-id-1";
         var runspaceInfo1 = new Mock<IRunspaceInfo>();
         runspaceInfo1.Setup(r => r.Id).Returns(runspaceId1);

         var runspaceStats1 = new Mock<IRunspaceStats>();
         runspaceStats1.Setup(r => r.RunspaceId).Returns(runspaceId1);

         var runspaceStatsFactory = new Mock<IRunspaceStatsFactory>();

         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId1,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats1.Object);

         var runspaceId2 = "runspace-id-2";
         var runspaceInfo2 = new Mock<IRunspaceInfo>();
         runspaceInfo2.Setup(r => r.Id).Returns(runspaceId2);

         var runspaceStats2 = new Mock<IRunspaceStats>();
         runspaceStats2.Setup(r => r.RunspaceId).Returns(runspaceId2);


         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId2,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats2.Object);


         var testMonitor = new RunspacesStatsMonitor(
            2,
            10,
            10,
            runspaceStatsFactory.Object,
            new RemoveExpiredIdleRunspaceRuleFactory(),
            new RemoveExpiredActiveRunspaceRuleFactory(),
            new RunspaceRetentionPolicy(new IRunspaceRetentionRule[] { }));



         // Act
         testMonitor.Register(runspaceInfo1.Object, string.Empty);
         testMonitor.Register(runspaceInfo2.Object, string.Empty);

         // Assert
         var registeredRunspaces = testMonitor.GetRegisteredRunspaces();

         Assert.NotNull(registeredRunspaces);
         Assert.AreEqual(2, registeredRunspaces.Length);
         Assert.AreEqual(runspaceId1, registeredRunspaces[0]);
         Assert.AreEqual(runspaceId2, registeredRunspaces[1]);
      }

      [Test]
      public void UnregisterRemovecRunspace() {
         // Arrange
         var runspaceId1 = "runspace-id-1";
         var runspaceInfo1 = new Mock<IRunspaceInfo>();
         runspaceInfo1.Setup(r => r.Id).Returns(runspaceId1);

         var runspaceStats1 = new Mock<IRunspaceStats>();
         runspaceStats1.Setup(r => r.RunspaceId).Returns(runspaceId1);

         var runspaceStatsFactory = new Mock<IRunspaceStatsFactory>();

         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId1,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats1.Object);

         var runspaceId2 = "runspace-id-2";
         var runspaceInfo2 = new Mock<IRunspaceInfo>();
         runspaceInfo2.Setup(r => r.Id).Returns(runspaceId2);

         var runspaceStats2 = new Mock<IRunspaceStats>();
         runspaceStats2.Setup(r => r.RunspaceId).Returns(runspaceId2);


         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId2,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats2.Object);


         var testMonitor = new RunspacesStatsMonitor(
            2,
            10,
            10,
            runspaceStatsFactory.Object,
            new RemoveExpiredIdleRunspaceRuleFactory(),
            new RemoveExpiredActiveRunspaceRuleFactory(),
            new RunspaceRetentionPolicy(new IRunspaceRetentionRule[] { }));
         testMonitor.Register(runspaceInfo1.Object, string.Empty);
         testMonitor.Register(runspaceInfo2.Object, string.Empty);


         // Act
         testMonitor.Unregister(runspaceId1);

         // Assert
         var registeredRunspaces = testMonitor.GetRegisteredRunspaces();

         Assert.NotNull(registeredRunspaces);
         Assert.AreEqual(1, registeredRunspaces.Length);
         Assert.AreEqual(runspaceId2, registeredRunspaces[0]);
      }

      [Test]
      public void EvaluateCallRefreshOfAllStatsAndReturnRunspacesAccordingToPolicy() {
         // Arrange
         var runspaceId1 = "runspace-id-1";
         var runspaceInfo1 = new Mock<IRunspaceInfo>();
         runspaceInfo1.Setup(r => r.Id).Returns(runspaceId1);

         var runspaceStats1 = new Mock<IRunspaceStats>();
         runspaceStats1.Setup(r => r.RunspaceId).Returns(runspaceId1);
         runspaceStats1.Setup(r => r.Refresh());

         var runspaceStatsFactory = new Mock<IRunspaceStatsFactory>();

         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId1,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats1.Object);

         var runspaceId2 = "runspace-id-2";
         var runspaceInfo2 = new Mock<IRunspaceInfo>();
         runspaceInfo2.Setup(r => r.Id).Returns(runspaceId2);

         var runspaceStats2 = new Mock<IRunspaceStats>();
         runspaceStats2.Setup(r => r.RunspaceId).Returns(runspaceId2);
         runspaceStats2.Setup(r => r.Refresh());


         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId2,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats2.Object);

         var retentionRule = new Mock<IRunspaceRetentionRule>();
         retentionRule.Setup(r => r.ShouldRemove(runspaceStats1.Object)).Returns(true);
         retentionRule.Setup(r => r.ShouldRemove(runspaceStats2.Object)).Returns(false);

         var testMonitor = new RunspacesStatsMonitor(
            2,
            10,
            10,
            runspaceStatsFactory.Object,
            new RemoveExpiredIdleRunspaceRuleFactory(),
            new RemoveExpiredActiveRunspaceRuleFactory(),
            new RunspaceRetentionPolicy(new[] { retentionRule.Object }));
         testMonitor.Register(runspaceInfo1.Object, string.Empty);
         testMonitor.Register(runspaceInfo2.Object, string.Empty);


         // Act
         var actual = testMonitor.EvaluateRunspacesToRemove();

         // Assert
         runspaceStats1.Verify(r => r.Refresh(), Times.Once());
         runspaceStats2.Verify(r => r.Refresh(), Times.Once());
         Assert.NotNull(actual);
         Assert.AreEqual(1, actual.Length);
         Assert.AreEqual(runspaceId1, actual[0]);
      }

      [Test]
      public void UpdateMaxActiveTimeSettingCreatesNewRetentionRules() {
         // Arrange
         var runspaceId1 = "runspace-id-1";
         var runspaceInfo1 = new Mock<IRunspaceInfo>();
         runspaceInfo1.Setup(r => r.Id).Returns(runspaceId1);

         var runspaceStats1 = new Mock<IRunspaceStats>();
         runspaceStats1.Setup(r => r.RunspaceId).Returns(runspaceId1);

         var runspaceStatsFactory = new Mock<IRunspaceStatsFactory>();

         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId1,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats1.Object);

         var unchangedIdleTimeValue = 10;
         var updatedActiveTimeValue = 99;
         var activeTimeRuleMock = new Mock<IRemoveExpiredActiveRunspaceRuleFactory>();
         activeTimeRuleMock.Setup(mock => mock.Create(updatedActiveTimeValue));         
         var idleTimeRuleMock = new Mock<IRemoveExpiredIdleRunspaceRuleFactory>();
         idleTimeRuleMock.Setup(mock => mock.Create(unchangedIdleTimeValue));


         var testMonitor = new RunspacesStatsMonitor(
            2,
            unchangedIdleTimeValue,
            10,
            runspaceStatsFactory.Object,
            idleTimeRuleMock.Object,
            activeTimeRuleMock.Object,
            null);
         testMonitor.Register(runspaceInfo1.Object, string.Empty);

         // Act
         testMonitor.UpdateConfiguration(2, unchangedIdleTimeValue, updatedActiveTimeValue);

         // Assert
         activeTimeRuleMock.Verify(mock => mock.Create(updatedActiveTimeValue), Times.Once());
         idleTimeRuleMock.Verify(mock => mock.Create(unchangedIdleTimeValue), Times.Exactly(2));
      }

      [Test]
      public void UpdateMaxIdleTimeSettingCreatesNewRetentionRules() {
         // Arrange
         var runspaceId1 = "runspace-id-1";
         var runspaceInfo1 = new Mock<IRunspaceInfo>();
         runspaceInfo1.Setup(r => r.Id).Returns(runspaceId1);

         var runspaceStats1 = new Mock<IRunspaceStats>();
         runspaceStats1.Setup(r => r.RunspaceId).Returns(runspaceId1);

         var runspaceStatsFactory = new Mock<IRunspaceStatsFactory>();

         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId1,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats1.Object);

         var updatedIdleTimeValue = 89;
         var unchangedActiveTimeValue = 99;
         var activeTimeRuleMock = new Mock<IRemoveExpiredActiveRunspaceRuleFactory>();
         activeTimeRuleMock.Setup(mock => mock.Create(unchangedActiveTimeValue));
         var idleTimeRuleMock = new Mock<IRemoveExpiredIdleRunspaceRuleFactory>();
         idleTimeRuleMock.Setup(mock => mock.Create(updatedIdleTimeValue));


         var testMonitor = new RunspacesStatsMonitor(
            2,
            10,
            unchangedActiveTimeValue,
            runspaceStatsFactory.Object,
            idleTimeRuleMock.Object,
            activeTimeRuleMock.Object,
            null);
         testMonitor.Register(runspaceInfo1.Object, string.Empty);

         // Act
         testMonitor.UpdateConfiguration(2, updatedIdleTimeValue, unchangedActiveTimeValue);

         // Assert
         activeTimeRuleMock.Verify(mock => mock.Create(unchangedActiveTimeValue), Times.Exactly(2));
         idleTimeRuleMock.Verify(mock => mock.Create(updatedIdleTimeValue), Times.Once);
      }

      [Test]
      public void UpdateMaxNumberRunspacesDoesnotCreateNewRetentionRules() {
         // Arrange
         var runspaceId1 = "runspace-id-1";
         var runspaceInfo1 = new Mock<IRunspaceInfo>();
         runspaceInfo1.Setup(r => r.Id).Returns(runspaceId1);

         var runspaceStats1 = new Mock<IRunspaceStats>();
         runspaceStats1.Setup(r => r.RunspaceId).Returns(runspaceId1);

         var runspaceStatsFactory = new Mock<IRunspaceStatsFactory>();

         runspaceStatsFactory.Setup(
            f =>
               f.Create(
                  runspaceId1,
                  It.IsAny<IRunspaceSessionInfoProvider>(),
                  It.IsAny<IActiveIdleInfoProvider>())).Returns(runspaceStats1.Object);

         var unchangedIdleTimeValue = 15;
         var unchangedActiveTimeValue = 10;
         var activeTimeRuleMock = new Mock<IRemoveExpiredActiveRunspaceRuleFactory>();
         activeTimeRuleMock.Setup(mock => mock.Create(unchangedActiveTimeValue));
         var idleTimeRuleMock = new Mock<IRemoveExpiredIdleRunspaceRuleFactory>();
         idleTimeRuleMock.Setup(mock => mock.Create(unchangedIdleTimeValue));

         var updatedMaxRunspaces = 99;

         var testMonitor = new RunspacesStatsMonitor(
            2,
            unchangedIdleTimeValue,
            unchangedActiveTimeValue,
            runspaceStatsFactory.Object,
            idleTimeRuleMock.Object,
            activeTimeRuleMock.Object,
            null);
         testMonitor.Register(runspaceInfo1.Object, string.Empty);

         // Act
         testMonitor.UpdateConfiguration(updatedMaxRunspaces, unchangedIdleTimeValue, unchangedActiveTimeValue);

         // Assert
         activeTimeRuleMock.Verify(mock => mock.Create(unchangedActiveTimeValue), Times.Once);
         idleTimeRuleMock.Verify(mock => mock.Create(unchangedIdleTimeValue), Times.Once);
      }
   }
}

