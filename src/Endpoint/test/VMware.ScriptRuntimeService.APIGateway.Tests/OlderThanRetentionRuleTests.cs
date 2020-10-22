// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class OlderThanRetentionRuleTests {
      [Test]
      public void OlderThanDateRecordsAreSelected() {
         // Arrange
         var tomorrow = DateTime.Today + new TimeSpan(1, 0, 0, 0);
         List<IPersistedScriptExecutionRecord> records = new List<IPersistedScriptExecutionRecord>();
         for (int i = 1; i <= 25; i++) {
            var recordMock = new Mock<IPersistedScriptExecutionRecord>();
            recordMock.Setup(r => r.LastUpdateDate).Returns(tomorrow - new TimeSpan(i, 0, 0, 0));
            records.Add(recordMock.Object);
         }

         // Act
         var rule = new OlderThanRetentionRule(new TimeSpan(20, 0, 0, 0));
         var actual = rule.Evaluate(records.ToArray());

         // Assert
         Assert.AreEqual(5, actual.Length);
         Assert.IsNotNull(actual.Select(r => r.LastUpdateDate == (tomorrow - new TimeSpan(25))));
         Assert.IsNotNull(actual.Select(r => r.LastUpdateDate == (tomorrow - new TimeSpan(24))));
         Assert.IsNotNull(actual.Select(r => r.LastUpdateDate == (tomorrow - new TimeSpan(23))));
         Assert.IsNotNull(actual.Select(r => r.LastUpdateDate == (tomorrow - new TimeSpan(22))));
         Assert.IsNotNull(actual.Select(r => r.LastUpdateDate == (tomorrow - new TimeSpan(21))));
      }

      [Test]
      public void EvaluateReturnsNullOnNullInput() {
         // Arrange
         var rule = new OlderThanRetentionRule(new TimeSpan(20, 0, 0, 0));

         // Act
         var actual = rule.Evaluate(null);

         // Assert
         Assert.IsNull(actual);
      }

      [Test]
      public void EvaluateReturnsNullOnEmptyInput() {
         // Arrange
         var rule = new OlderThanRetentionRule(new TimeSpan(20, 0, 0, 0));

         // Act
         var actual = rule.Evaluate(new IPersistedScriptExecutionRecord[]{});

         // Assert
         Assert.IsNull(actual);
      }
   }
}
