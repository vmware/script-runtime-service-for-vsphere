// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class PollingScriptExecutionPersisterTests {
      class NamedScriptExecutionMock : INamedScriptExecution {
         public string Name { get; set; }
         public string Id { get; set; }

         public ScriptState State { get; set; }

         public string Reason { get; set; }

         public OutputObjectsFormat OutputObjectsFormat { get; set; }
         public DateTime? StarTime { get; set; }
         public DateTime? EndTime { get; set; }

         public override bool Equals(object obj) {
            INamedScriptExecution source = obj as INamedScriptExecution;
            if (source == null) {
               return false;
            }

            return source.Name == Name && source.State == State;
         }
         
         public override int GetHashCode() {
             return this.Id.GetHashCode();
         }
      }
      [Test]
      public void PollsScriptsStateUntilComplete() {
         // Arrange
         ILogger logger = new Mock<ILogger>().Object;
         var scriptId = "testScriptId";
         var scriptName = "testScriptName";
         var runspaceMock = new Mock<IRunspace>();
         var pollCount = 0;
         long completed = 0;

         var runningScriptExecutionResultMock = new Mock<IScriptExecutionResult>();
         runningScriptExecutionResultMock.Setup(m => m.State).Returns(ScriptState.Running);
         runningScriptExecutionResultMock.Setup(m => m.OutputObjectCollection).Returns(new OutputObjectCollection());

         var completeScriptExecutionResultMock = new Mock<IScriptExecutionResult>();
         completeScriptExecutionResultMock.Setup(m => m.State).Returns(ScriptState.Success);
         completeScriptExecutionResultMock.Setup(m => m.OutputObjectCollection).Returns(new OutputObjectCollection());

         runspaceMock.Setup(m => m.GetScript(scriptId)).Returns(() => {
            Interlocked.Increment(ref pollCount);
            if (pollCount < 3) {
               return runningScriptExecutionResultMock.Object;
            } else {
               return completeScriptExecutionResultMock.Object;
            }
         });

         var scriptExecutionWriterMock = new Mock<IScriptExecutionStoreProvider>();
         var runningNamedScriptExecution = new NamedScriptExecutionMock {
            Name = scriptName,
            State = ScriptState.Running

         };

         scriptExecutionWriterMock.Setup(m => m.WriteScriptExecution(runningNamedScriptExecution)).Verifiable();
         var completeNamedScriptExecution = new NamedScriptExecutionMock {
            Name = scriptName,
            State = ScriptState.Success
         };
         scriptExecutionWriterMock.Setup(m => m.WriteScriptExecution(completeNamedScriptExecution)).Verifiable();

         var testObject = new PollingScriptExecutionPersister(logger);

         // Act
         testObject.ScriptResultPersisted += (object sender, ScriptResultStoredEventArgs e) => {
            Interlocked.Exchange(ref completed, 1);
         };
         testObject.Start(runspaceMock.Object, scriptId, scriptName, scriptExecutionWriterMock.Object);

         while (Interlocked.Read(ref completed) == 0) {
            Thread.Sleep(500);
         }

         // Assert
         Assert.IsTrue(pollCount >= 3);
         scriptExecutionWriterMock.Verify(x => x.WriteScriptExecution(runningNamedScriptExecution), Times.Exactly(2));
         scriptExecutionWriterMock.Verify(x => x.WriteScriptExecution(completeNamedScriptExecution), Times.Exactly(1));
      }

      [Test]
      public void StartInitializesRunningScriptWithNameAndId() {
         // Arrange
         ILogger logger = new Mock<ILogger>().Object;
         var scriptId = "testScriptId";
         var scriptName = "testScriptName";
         var runspaceMock = new Mock<IRunspace>();         

         var runningScriptExecutionResultMock = new Mock<IScriptExecutionResult>();
         runningScriptExecutionResultMock.Setup(m => m.State).Returns(ScriptState.Running);
         runningScriptExecutionResultMock.Setup(m => m.OutputObjectCollection).Returns(new OutputObjectCollection());

         var completeScriptExecutionResultMock = new Mock<IScriptExecutionResult>();
         completeScriptExecutionResultMock.Setup(m => m.State).Returns(ScriptState.Success);
         completeScriptExecutionResultMock.Setup(m => m.OutputObjectCollection).Returns(new OutputObjectCollection());

         runspaceMock.Setup(m => m.GetScript(scriptId)).Returns(() => {
            return runningScriptExecutionResultMock.Object;          
         });

         var scriptExecutionWriterMock = new Mock<IScriptExecutionStoreProvider>();
     
         var runningNamedScriptExecution = new NamedScriptExecutionMock {
            Name = scriptName,
            Id = scriptId,
            State = ScriptState.Running            
         };
         scriptExecutionWriterMock.Setup(m => m.WriteScriptExecution(runningNamedScriptExecution)).Verifiable();

         var testObject = new PollingScriptExecutionPersister(logger);

         // Act
         testObject.Start(runspaceMock.Object, scriptId, scriptName, scriptExecutionWriterMock.Object);
                
         // Assert         
         scriptExecutionWriterMock.Verify(x => x.WriteScriptExecution(runningNamedScriptExecution), Times.AtLeastOnce());
      }
   }
}
