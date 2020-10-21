// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Security.Permissions;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model;
using StreamRecord = VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class ScriptExecutionFileReaderTests {
      private NamedScriptExecution _scriptExecution;
      ILogger _logger = new Mock<ILogger>().Object;
      string _rootFolder = "c:\\scripts";
      string _userId = "testuser";
      string _scriptId = "testscript";

      [SetUp]
      public void SetUp() {
         var scriptName = "MyTestScript";
         var scriptState = "success";
         var scriptReason = "ScriptReason";
         var scriptOutputObjectFormat = ScriptExecutionResponse.OutputObjectsFormatEnum.Json;
         var scriptStreams = new VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model.ScriptExecutionStreams(
            new List<StreamRecord>(new []{new StreamRecord {
               Time = DateTime.Today,
               Message = "InfoStream"
            }}),
            new List<StreamRecord>(new[]{new StreamRecord {
               Time = DateTime.Today,
               Message = "ErrorStream"
            }}),
            new List<StreamRecord>(new[]{new StreamRecord {
               Time = DateTime.Today,
               Message = "WarningStream"
            }}),
            new List<StreamRecord>(new[]{new StreamRecord {
               Time = DateTime.Today,
               Message = "DebugStream"
            }}),
            new List<StreamRecord>(new[]{new StreamRecord {
               Time = DateTime.Today,
               Message = "VerboseStream"
            }}));

         var scriptJsonOutput =
            new VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model.OutputObjectCollection(serializedObjects: new List<string>(new[] { "{'name':'JsonResult'}" }));

         
         var scriptResult = new ScriptResult(
            new ScriptExecutionResponse(
               _scriptId,
               scriptState,
               scriptReason,
               scriptJsonOutput,
               scriptStreams,
               scriptOutputObjectFormat));

         _scriptExecution = new NamedScriptExecution(
            scriptName,
            scriptResult);
      }


      [Test]
      public void ReadScriptExecution() {
         // Arrange
         var fileSystem = new MockFileSystem();
         var scriptExecutionJson = JsonConvert.SerializeObject(_scriptExecution);
         fileSystem.Directory.CreateDirectory(Path.Combine(_rootFolder, _userId, _scriptId));
         fileSystem.File.WriteAllText(Path.Combine(_rootFolder, _userId, _scriptId, ScriptExecutionFileNames.ScriptExecution), scriptExecutionJson);

         // Act
         var fileStoreReader = new ScriptExecutionFileReader(_logger, _rootFolder, _userId, _scriptId, fileSystem);
         var actual = fileStoreReader.ReadScriptExecution();

         // Assert
         Assert.AreEqual(_scriptExecution.Name, actual.Name);
         Assert.AreEqual(_scriptExecution.Id, actual.Id);
         Assert.AreEqual(_scriptExecution.OutputObjectsFormat, actual.OutputObjectsFormat);
         Assert.AreEqual(_scriptExecution.Reason, actual.Reason);
         Assert.AreEqual(_scriptExecution.State, actual.State);
      }

      [Test]
      public void ReadScriptExecutionOutput() {
         // Arrange
         var fileSystem = new MockFileSystem();
         var scriptExecutionOutput = new ScriptExecutionOutput {
            OutputObjects = new[] {"TestObject1", "TestObject2" }
         };
         var scriptExecutionJson = JsonConvert.SerializeObject(_scriptExecution);
         var scriptExecutionOutputJson = JsonConvert.SerializeObject(scriptExecutionOutput);
         fileSystem.Directory.CreateDirectory(Path.Combine(_rootFolder, _userId, _scriptId));
         fileSystem.File.WriteAllText(Path.Combine(_rootFolder, _userId, _scriptId, ScriptExecutionFileNames.ScriptExecution), scriptExecutionJson);
         fileSystem.File.WriteAllText(Path.Combine(_rootFolder, _userId, _scriptId, ScriptExecutionFileNames.ScriptExecutionOutput), scriptExecutionOutputJson);

         // Act
         var fileStoreReader = new ScriptExecutionFileReader(_logger, _rootFolder, _userId, _scriptId, fileSystem);
         var actual = fileStoreReader.ReadScriptExecutionOutput();

         // Assert
         Assert.AreEqual(2, actual.OutputObjects.Length);
         Assert.AreEqual(scriptExecutionOutput.OutputObjects[0], actual.OutputObjects[0]);
         Assert.AreEqual(scriptExecutionOutput.OutputObjects[1], actual.OutputObjects[1]);
      }

      [Test]
      public void ReadScriptExecutionStreams() {
         // Arrange
         var fileSystem = new MockFileSystem();
         var scriptExecutionStreams= new ScriptExecutionDataStreams(new DataStreams() {
            Information = new ScriptExecutionStorage.DataTypes.StreamRecord[]{new ScriptExecutionStorage.DataTypes.StreamRecord {
               Time = DateTime.Today,
               Message = "Info"
            }},
            Debug = new ScriptExecutionStorage.DataTypes.StreamRecord[]{new ScriptExecutionStorage.DataTypes.StreamRecord {
               Time = DateTime.Today,
               Message = "Debug"
            }},
            Error = new ScriptExecutionStorage.DataTypes.StreamRecord[]{new ScriptExecutionStorage.DataTypes.StreamRecord {
               Time = DateTime.Today,
               Message = "Error"
            }},
            Verbose = new ScriptExecutionStorage.DataTypes.StreamRecord[]{new ScriptExecutionStorage.DataTypes.StreamRecord {
               Time = DateTime.Today,
               Message = "Verbose"
            }},
            Warning = new ScriptExecutionStorage.DataTypes.StreamRecord[]{new ScriptExecutionStorage.DataTypes.StreamRecord {
               Time = DateTime.Today,
               Message = "Warning"
            }}
         });
         var scriptExecutionJson = JsonConvert.SerializeObject(_scriptExecution);
         var scriptExecutionStreamsJson = JsonConvert.SerializeObject(scriptExecutionStreams);
         fileSystem.Directory.CreateDirectory(Path.Combine(_rootFolder, _userId, _scriptId));
         fileSystem.File.WriteAllText(Path.Combine(_rootFolder, _userId, _scriptId, ScriptExecutionFileNames.ScriptExecution), scriptExecutionJson);
         fileSystem.File.WriteAllText(Path.Combine(_rootFolder, _userId, _scriptId, ScriptExecutionFileNames.ScriptExecutionStreams), scriptExecutionStreamsJson);

         // Act
         var fileStoreReader = new ScriptExecutionFileReader(_logger, _rootFolder, _userId, _scriptId, fileSystem);
         var actual = fileStoreReader.ReadScriptExecutionDataStreams();

         // Assert
         Assert.AreEqual(scriptExecutionStreams.Streams.Debug[0].Time, actual.Streams.Debug[0].Time);
         Assert.AreEqual(scriptExecutionStreams.Streams.Debug[0].Message, actual.Streams.Debug[0].Message);

         Assert.AreEqual(scriptExecutionStreams.Streams.Information[0].Time, actual.Streams.Information[0].Time);
         Assert.AreEqual(scriptExecutionStreams.Streams.Information[0].Message, actual.Streams.Information[0].Message);

         Assert.AreEqual(scriptExecutionStreams.Streams.Warning[0].Time, actual.Streams.Warning[0].Time);
         Assert.AreEqual(scriptExecutionStreams.Streams.Warning[0].Message, actual.Streams.Warning[0].Message);

         Assert.AreEqual(scriptExecutionStreams.Streams.Error[0].Time, actual.Streams.Error[0].Time);
         Assert.AreEqual(scriptExecutionStreams.Streams.Error[0].Message, actual.Streams.Error[0].Message);

         Assert.AreEqual(scriptExecutionStreams.Streams.Verbose[0].Time, actual.Streams.Verbose[0].Time);
         Assert.AreEqual(scriptExecutionStreams.Streams.Verbose[0].Message, actual.Streams.Verbose[0].Message);
      }
   }
}
