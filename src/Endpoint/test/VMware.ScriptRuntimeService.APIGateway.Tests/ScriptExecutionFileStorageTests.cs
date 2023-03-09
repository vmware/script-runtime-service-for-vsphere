// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient;
using ScriptState = VMware.ScriptRuntimeService.Runspace.Types.ScriptState;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class ScriptExecutionFileStorageTests {
      private ScriptExecutionFileStorage _scriptExecutionFileStorage;
      private string _rootPath = Path.Combine("c","scripts");
      private string _userId1 = "user1";
      private string _userId2 = "user2";
      private string _scriptName1 = "script1";
      private string _scriptName2 = "script2";
      private string _scriptName3 = "script3";
      private string _scriptId1 = "scriptId1";
      private string _scriptId2 = "scriptId2";
      private string _scriptId3 = "scriptId3";
      private string _script2Output= "Script2Output";
      private ScriptExecutionStorage.DataTypes.StreamRecord[] _script2InfoStream = new ScriptExecutionStorage.DataTypes.StreamRecord[]{new ScriptExecutionStorage.DataTypes.StreamRecord {
         Time = DateTime.Today,
         Message = "Info"
      }};
      private IRunspace _runspace;
      private IFileSystem _fileSystem;

      [SetUp]
      public void SetUpMultipleScriptsForDifferentUsers() {
         var loggerFactoryMock = new Mock<ILoggerFactory>();
         ILogger logger = new Mock<ILogger>().Object;
         loggerFactoryMock.Setup(m => m.CreateLogger(typeof(ScriptExecutionFileStorage).FullName)).Returns(logger);
         _fileSystem = new MockFileSystem();
         var runspaceMock = new Mock<IRunspace>();

         _scriptExecutionFileStorage = new ScriptExecutionFileStorage(
            loggerFactoryMock.Object, 
            new ScriptExecutionStorageSettings {
               ServiceScriptStorageDir = _rootPath
            },
            _fileSystem,
            new ScriptExecutionFileStoreProviderFactory(), 
            new PollingScriptExecutionPersisterFactory());

         var script1ResultMock = new Mock<IScriptExecutionResult>();
         script1ResultMock.SetupGet(m => m.Id).Returns(_scriptId1);
         script1ResultMock.Setup(m => m.State).Returns(ScriptState.Success);
         script1ResultMock.Setup(m => m.OutputObjectCollection).Returns(new OutputObjectCollection());

         runspaceMock.Setup(m => m.GetScript(_scriptId1)).Returns(() => script1ResultMock.Object);

         var script2ResultMock = new Mock<IScriptExecutionResult>();
         script2ResultMock.SetupGet(m => m.Id).Returns(_scriptId2);
         script2ResultMock.Setup(m => m.State).Returns(ScriptState.Success);
         script2ResultMock.Setup(m => m.OutputObjectCollection).Returns(new OutputObjectCollection{FormattedTextPresentation = _script2Output });
         script2ResultMock.Setup(m => m.Streams).Returns(new DataStreams { Information = _script2InfoStream });

         runspaceMock.Setup(m => m.GetScript(_scriptId2)).Returns(() => script2ResultMock.Object);

         var script3ResultMock = new Mock<IScriptExecutionResult>();
         script3ResultMock.SetupGet(m => m.Id).Returns(_scriptId3);
         script3ResultMock.Setup(m => m.State).Returns(ScriptState.Success);
         script3ResultMock.Setup(m => m.OutputObjectCollection).Returns(new OutputObjectCollection());

         runspaceMock.Setup(m => m.GetScript(_scriptId3)).Returns(() => script3ResultMock.Object);
         _runspace = runspaceMock.Object;
      }

      private void ArrangeDataRetrievalTests() {
         List<string> completedScripts = new List<string>();
         _scriptExecutionFileStorage.ScriptResultStored += (sender, args) => {
            completedScripts.Add(args.ScriptId);
         };

         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId1, _runspace, _scriptId1, _scriptName1, false);
         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId2, _runspace, _scriptId2, _scriptName2, false);
         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId2, _runspace, _scriptId3, _scriptName3, false);

         while (!completedScripts.Contains(_scriptId1) ||
                !completedScripts.Contains(_scriptId2) ||
                !completedScripts.Contains(_scriptId3)) {
            Thread.Sleep(500);
         }
      }

      [Test]
      public void MultipleScriptsForDifferentUsersAreStoredInFiles() {
         // Arrange
         List<string> completedScripts = new List<string>();
         _scriptExecutionFileStorage.ScriptResultStored += (sender, args) => {
            completedScripts.Add(args.ScriptId);
         };

         // Act
         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId1, _runspace, _scriptId1, _scriptName1, false);
         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId2, _runspace, _scriptId2, _scriptName2, false);
         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId2, _runspace, _scriptId3, _scriptName3, false);

         while (!completedScripts.Contains(_scriptId1) ||
                !completedScripts.Contains(_scriptId2) ||
                !completedScripts.Contains(_scriptId3)) {
            Thread.Sleep(500);
         }
         
         // Assert
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootPath, _userId1, _scriptId1)));
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootPath, _userId2, _scriptId2)));
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootPath, _userId2, _scriptId3)));

         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId1, _scriptId1, ScriptExecutionFileNames.ScriptExecution)));
         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId1, _scriptId1, ScriptExecutionFileNames.ScriptExecutionOutput)));
         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId1, _scriptId1, ScriptExecutionFileNames.ScriptExecutionStreams)));

         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId2, _scriptId2, ScriptExecutionFileNames.ScriptExecution)));
         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId2, _scriptId2, ScriptExecutionFileNames.ScriptExecutionOutput)));
         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId2, _scriptId2, ScriptExecutionFileNames.ScriptExecutionStreams)));

         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId2, _scriptId3, ScriptExecutionFileNames.ScriptExecution)));
         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId2, _scriptId3, ScriptExecutionFileNames.ScriptExecutionOutput)));
         Assert.IsTrue(_fileSystem.File.Exists(Path.Combine(_rootPath, _userId2, _scriptId3, ScriptExecutionFileNames.ScriptExecutionStreams)));

      }

      [Test]
      public void ListScriptsForUser() {
         // Arrange
         ArrangeDataRetrievalTests();

         // Act
         var user2Scripts = _scriptExecutionFileStorage.ListScriptExecutions(_userId2, false);

         // Assert
         Assert.AreEqual(2, user2Scripts.Length);
         Assert.Contains(_scriptId2, user2Scripts.Select(s => s.Id).ToArray());
         Assert.Contains(_scriptId3, user2Scripts.Select(s => s.Id).ToArray());
      }

      [Test]
      public void GetScriptExecution() {
         // Arrange
         ArrangeDataRetrievalTests();

         // Act
         var user1Script1 = _scriptExecutionFileStorage.GetScriptExecution(_userId1, _scriptId1);

         // Assert
         Assert.IsNotNull(user1Script1);
         Assert.AreEqual(_scriptId1, user1Script1.Id);
         Assert.AreEqual(_scriptName1, user1Script1.Name);
         Assert.AreEqual(ScriptState.Success, user1Script1.State);
      }

      [Test]
      public void GetScriptExecutionOutput() {
         // Arrange
         ArrangeDataRetrievalTests();

         // Act
         var user2Script2Output = _scriptExecutionFileStorage.GetScriptExecutionOutput(_userId2, _scriptId2);

         // Assert
         Assert.NotNull(user2Script2Output);
         Assert.NotNull(user2Script2Output.OutputObjects);
         Assert.AreEqual(_script2Output, user2Script2Output.OutputObjects[0]);
      }

      [Test]
      public void GetScriptExecutionStreams() {
         // Arrange
         List<string> completedScripts = new List<string>();
         _scriptExecutionFileStorage.ScriptResultStored += (sender, args) => {
            completedScripts.Add(args.ScriptId);
         };

         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId1, _runspace, _scriptId1, _scriptName1, false);
         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId2, _runspace, _scriptId2, _scriptName2, false);
         _scriptExecutionFileStorage.StartStoringScriptExecution(_userId2, _runspace, _scriptId3, _scriptName3, false);

         while (!completedScripts.Contains(_scriptId1) ||
                !completedScripts.Contains(_scriptId2) ||
                !completedScripts.Contains(_scriptId3)) {
            Thread.Sleep(500);
         }

         // Act
         var user2Script2Streams = _scriptExecutionFileStorage.GetScriptExecutionDataStreams(_userId2, _scriptId2);

         // Assert
         Assert.NotNull(user2Script2Streams);
         Assert.NotNull(user2Script2Streams.Streams);
         Assert.AreEqual(_script2InfoStream, user2Script2Streams.Streams.Information);
      }

      [Test]
      public void UserCantAccessOtherUsersScripts() {
         // Arrange
         ArrangeDataRetrievalTests();

         // Act
         var inaccessibleScript = _scriptExecutionFileStorage.GetScriptExecution(_userId1, _scriptId2);

         // Assert
         Assert.IsNull(inaccessibleScript);
      }
   }
}
