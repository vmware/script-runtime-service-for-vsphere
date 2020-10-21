// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using NUnit.Framework;
using Moq;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy;
using VMware.ScriptRuntimeService.APIGateway.Runspace;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class RunspaceRetentionPolicyTests {
      [Test]
      public void KeepActiveRuleRemovesIdleWithNoSession() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.IsActive).Returns(false);
         runspaceStatsMock.Setup(r => r.HasActiveSession).Returns(false);
         var rule = new KeepActiveRunspaceOnInactiveSessionRule();

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsTrue(actual);
      }

      [Test]
      public void KeepActiveRuleKeepsActiveWithNoSession() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.IsActive).Returns(true);
         runspaceStatsMock.Setup(r => r.HasActiveSession).Returns(false);
         var rule = new KeepActiveRunspaceOnInactiveSessionRule();

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void KeepActiveRuleKeepsIdleWithSession() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.IsActive).Returns(false);
         runspaceStatsMock.Setup(r => r.HasActiveSession).Returns(true);
         var rule = new KeepActiveRunspaceOnInactiveSessionRule();

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void KeepActiveRuleKeepsActiveWithSession() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.IsActive).Returns(true);
         runspaceStatsMock.Setup(r => r.HasActiveSession).Returns(true);
         var rule = new KeepActiveRunspaceOnInactiveSessionRule();

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void RemoveIdleRuleRemovesExpiredIdle() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.IdleTimeSeconds).Returns(6 * 60);
         runspaceStatsMock.Setup(r => r.IsActive).Returns(false);
         var rule = new RemoveExpiredIdleRunspaceRule(5);

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsTrue(actual);
      }

      [Test]
      public void RemoveIdleRuleKeepsActive() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.IdleTimeSeconds).Returns(6 * 60);
         runspaceStatsMock.Setup(r => r.ActiveTimeSeconds).Returns(6 * 60);
         runspaceStatsMock.Setup(r => r.IsActive).Returns(true);
         var rule = new RemoveExpiredIdleRunspaceRule(5);

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void RemoveIdleRuleKeepsIdleNotExpired() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.IdleTimeSeconds).Returns(3 * 60);
         runspaceStatsMock.Setup(r => r.IsActive).Returns(false);
         var rule = new RemoveExpiredIdleRunspaceRule(5);

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void RemoveActiveRuleRemovesExpiredActive() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.ActiveTimeSeconds).Returns(6 * 60);
         runspaceStatsMock.Setup(r => r.IsActive).Returns(true);
         var rule = new RemoveExpiredActiveRunspaceRule(5);

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsTrue(actual);
      }

      [Test]
      public void RemoveActiveRuleKeepsIdle() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.ActiveTimeSeconds).Returns(6 * 60);
         runspaceStatsMock.Setup(r => r.IsActive).Returns(false);
         var rule = new RemoveExpiredActiveRunspaceRule(5);

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void RemoveActiveRuleKeepsActiveNotExpired() {
         // Arrange
         var runspaceStatsMock = new Mock<IRunspaceStats>();
         runspaceStatsMock.Setup(r => r.ActiveTimeSeconds).Returns(3 * 60);
         runspaceStatsMock.Setup(r => r.IsActive).Returns(true);
         var rule = new RemoveExpiredActiveRunspaceRule(5);

         // Act
         var actual = rule.ShouldRemove(runspaceStatsMock.Object);

         // Assert
         Assert.IsFalse(actual);
      }

      [Test]
      public void PolicyShouldRemoveWhenOneRuleShouldRemove() {
         // Arrange
         var rule1Mock = new Mock<IRunspaceRetentionRule>();
         rule1Mock.Setup(r => r.ShouldRemove(It.IsAny<IRunspaceStats>())).Returns(true);

         var rule2Mock = new Mock<IRunspaceRetentionRule>();
         rule2Mock.Setup(r => r.ShouldRemove(It.IsAny<IRunspaceStats>())).Returns(false);

         var policy = new RunspaceRetentionPolicy(new []{rule1Mock.Object, rule2Mock.Object});

         // Act
         var actual = policy.ShouldRemove(null);

         // Assert
         Assert.IsTrue(actual);
      }

      [Test]
      public void PolicyKeepsWhenAllRulesKeep() {
         // Arrange
         var rule1Mock = new Mock<IRunspaceRetentionRule>();
         rule1Mock.Setup(r => r.ShouldRemove(It.IsAny<IRunspaceStats>())).Returns(false);

         var rule2Mock = new Mock<IRunspaceRetentionRule>();
         rule2Mock.Setup(r => r.ShouldRemove(It.IsAny<IRunspaceStats>())).Returns(false);

         var policy = new RunspaceRetentionPolicy(new[] { rule1Mock.Object, rule2Mock.Object });

         // Act
         var actual = policy.ShouldRemove(null);

         // Assert
         Assert.IsFalse(actual);
      }
   }
}
