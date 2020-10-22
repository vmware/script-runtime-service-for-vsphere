// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes;
using VMware.ScriptRuntimeService.RunspaceClient;
using ScriptExecutionResponse = VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model.ScriptExecutionResponse;

namespace VMware.ScriptRuntimeService.APIGateway.Tests
{
    public class ScriptExecutionFileStoreProviderTests
    {
       [Test]
       public void WriteReadDontProduceFiles() {
          // Arrange
          var fileSystem = new MockFileSystem();
          var logger = new Mock<ILogger>();
          var rootFolder = "c:\\scripts";
          var userId = "testuser";
         var testId = "testscript";
         var fileStoreProvider = new ScriptExecutionFileStoreProvider(logger.Object, rootFolder, userId, testId, fileSystem);
          var scriptResult = new ScriptResult(
             new ScriptExecutionResponse(
                testId,
                "success",
                outputObjectCollection :
                new VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model.OutputObjectCollection("TextResult"),
                dataStreams:new ScriptRuntimeService.RunspaceClient.Bindings.Model.ScriptExecutionStreams(
                   new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                      Time = DateTime.Today,
                      Message = "InfoStream"
                   }}))));
          var scriptExecution = new NamedScriptExecution(
             "testScript",
             scriptResult);
          var scriptExecutionOutput = new ScriptExecutionOutput(scriptResult);
          var scriptExecutionStreams = new ScriptExecutionDataStreams(scriptResult.Streams);

         // Act
         fileStoreProvider.WriteScriptExecution(scriptExecution);
         fileStoreProvider.WriteScriptExecutionOutput(scriptExecutionOutput);
         fileStoreProvider.WriteScriptExecutionDataStreams(scriptExecutionStreams);

         fileStoreProvider.ReadScriptExecution();
         fileStoreProvider.ReadScriptExecutionOutput();
         fileStoreProvider.ReadScriptExecutionDataStreams();

         fileStoreProvider.WriteScriptExecution(scriptExecution);
         fileStoreProvider.WriteScriptExecutionOutput(scriptExecutionOutput);
         fileStoreProvider.WriteScriptExecutionDataStreams(scriptExecutionStreams);

         // Assert
         Assert.IsEmpty(fileSystem.Directory.GetFileSystemEntries(Path.Combine(rootFolder, userId, testId)));
       }

       [Test]
       public void FlushProducesFiles() {
          // Arrange
          var fileSystem = new MockFileSystem();
          var logger = new Mock<ILogger>();
          var rootFolder = "c:\\scripts";
          var userId = "testuser";
          var testId = "testscript";
          var fileStoreProvider = new ScriptExecutionFileStoreProvider(logger.Object, rootFolder, userId, testId, fileSystem);
          var scriptResult = new ScriptResult(
             new ScriptExecutionResponse(
                testId,
                "success",
                outputObjectCollection:
                new VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model.OutputObjectCollection("TextResult"),
                dataStreams: new ScriptRuntimeService.RunspaceClient.Bindings.Model.ScriptExecutionStreams(
                   new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                      Time = DateTime.Today,
                      Message = "InfoStream"
                   }}))));
          var scriptExecution = new NamedScriptExecution(
             "testScript",
             scriptResult);
          var scriptExecutionOutput = new ScriptExecutionOutput(scriptResult);
          var scriptExecutionStreams = new ScriptExecutionDataStreams(scriptResult.Streams);

          // Act
          fileStoreProvider.WriteScriptExecution(scriptExecution);
          fileStoreProvider.WriteScriptExecutionOutput(scriptExecutionOutput);
          fileStoreProvider.WriteScriptExecutionDataStreams(scriptExecutionStreams);

          fileStoreProvider.Flush();

          // Assert
          Assert.IsTrue(fileSystem.Directory.Exists(Path.Combine(rootFolder, userId, testId)));
          Assert.IsTrue(fileSystem.File.Exists(Path.Combine(rootFolder, userId, testId, ScriptExecutionFileNames.ScriptExecution)));
          Assert.IsTrue(fileSystem.File.Exists(Path.Combine(rootFolder, userId, testId, ScriptExecutionFileNames.ScriptExecutionOutput)));
          Assert.IsTrue(fileSystem.File.Exists(Path.Combine(rootFolder, userId, testId, ScriptExecutionFileNames.ScriptExecutionStreams)));
      }

       [Test]
       public void FilesContentIsCorrect() {
          // Arrange
          var fileSystem = new MockFileSystem();
          var logger = new Mock<ILogger>();
          var rootFolder = "c:\\scripts";
          var userId = "testuser";
          var scriptId = "testscript";
          var scriptName = "MyTestScript";
          var scriptState = "success";
          var scriptReason = "ScriptReason";
          var scriptOutputObjectFormat = ScriptExecutionResponse.OutputObjectsFormatEnum.Json;
          var scriptStreams = new ScriptRuntimeService.RunspaceClient.Bindings.Model.ScriptExecutionStreams(
             new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                Time = DateTime.Today,
                Message = "InfoStream"
             }}),
             new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                Time = DateTime.Today,
                Message = "ErrorStream"
             }}),
             new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                Time = DateTime.Today,
                Message = "WarningStream"
             }}),
             new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                Time = DateTime.Today,
                Message = "DebugStream"
             }}),
             new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                Time = DateTime.Today,
                Message = "VerboseStream"
             }}));
          var scriptJsonOutput =
             new ScriptRuntimeService.RunspaceClient.Bindings.Model.OutputObjectCollection(serializedObjects: new List<string>(new [] {"{'name':'JsonResult'}"}));

         var fileStoreProvider = new ScriptExecutionFileStoreProvider(logger.Object, rootFolder, userId, scriptId, fileSystem);
          var scriptResult = new ScriptResult(
             new ScriptExecutionResponse(
                scriptId,
                scriptState,
                scriptReason,
                scriptJsonOutput,
                scriptStreams,
                scriptOutputObjectFormat));

          var scriptExecution = new NamedScriptExecution(
             scriptName,
             scriptResult);

          var scriptExecutionOutput = new ScriptExecutionOutput(scriptResult);
          var scriptExecutionStreams = new ScriptExecutionDataStreams(scriptResult.Streams);

          // Act
          fileStoreProvider.WriteScriptExecution(scriptExecution);
          fileStoreProvider.WriteScriptExecutionOutput(scriptExecutionOutput);
          fileStoreProvider.WriteScriptExecutionDataStreams(scriptExecutionStreams);

          fileStoreProvider.Flush();

          // Read Actual Files Content
          var scriptExecutionFileContent = fileSystem.File.ReadAllText(Path.Combine(rootFolder, userId, scriptId, ScriptExecutionFileNames.ScriptExecution));
          var scriptExecutionOutputFileContent = fileSystem.File.ReadAllText(Path.Combine(rootFolder, userId, scriptId, ScriptExecutionFileNames.ScriptExecutionOutput));
          var scriptExecutionStreamsFileContent = fileSystem.File.ReadAllText(Path.Combine(rootFolder, userId, scriptId, ScriptExecutionFileNames.ScriptExecutionStreams));

          var actualScriptExecution = JsonConvert.DeserializeObject<NamedScriptExecution>(scriptExecutionFileContent);
          var actualScriptExecutionOutput = JsonConvert.DeserializeObject<ScriptExecutionOutput>(scriptExecutionOutputFileContent);
          var actualScriptExecutionStreams = JsonConvert.DeserializeObject<ScriptExecutionDataStreams>(scriptExecutionStreamsFileContent);

          // Assert
          Assert.AreEqual(scriptExecution.Id, actualScriptExecution.Id);
          Assert.AreEqual(scriptExecution.Name, actualScriptExecution.Name);
          Assert.AreEqual(scriptExecution.Reason, actualScriptExecution.Reason);
          Assert.AreEqual(scriptExecution.State, actualScriptExecution.State);
          Assert.AreEqual(scriptExecution.OutputObjectsFormat, actualScriptExecution.OutputObjectsFormat);

          Assert.AreEqual(scriptJsonOutput.SerializedObjects[0], actualScriptExecutionOutput.OutputObjects[0]);

          Assert.AreEqual(scriptStreams.Information[0].Message, actualScriptExecutionStreams.Streams.Information[0].Message);
          Assert.AreEqual(scriptStreams.Information[0].Time, actualScriptExecutionStreams.Streams.Information[0].Time);

          Assert.AreEqual(scriptStreams.Debug[0].Message, actualScriptExecutionStreams.Streams.Debug[0].Message);
          Assert.AreEqual(scriptStreams.Debug[0].Time, actualScriptExecutionStreams.Streams.Debug[0].Time);

          Assert.AreEqual(scriptStreams.Error[0].Message, actualScriptExecutionStreams.Streams.Error[0].Message);
          Assert.AreEqual(scriptStreams.Error[0].Time, actualScriptExecutionStreams.Streams.Error[0].Time);

         Assert.AreEqual(scriptStreams.Verbose[0].Message, actualScriptExecutionStreams.Streams.Verbose[0].Message);
          Assert.AreEqual(scriptStreams.Verbose[0].Time, actualScriptExecutionStreams.Streams.Verbose[0].Time);

         Assert.AreEqual(scriptStreams.Warning[0].Message, actualScriptExecutionStreams.Streams.Warning[0].Message);
          Assert.AreEqual(scriptStreams.Warning[0].Time, actualScriptExecutionStreams.Streams.Warning[0].Time);
      }

       [Test]
       public void DifferentUserNameFormatsStoreFiles() {
          foreach (var userId in new[]
             {"Administrator@VSPHERE.LOCAL", "VSPHERE.LOCAL/Administrator", "VSPHERE.LOCAL\\Administrator"}) {
             // Arrange
            var fileSystem = new MockFileSystem();
             var logger = new Mock<ILogger>();
             var rootFolder = "c:\\scripts";
             var testId = "testscript";

             var fileStoreProvider = new ScriptExecutionFileStoreProvider(logger.Object, rootFolder, userId, testId, fileSystem);
             var scriptResult = new ScriptResult(
                new ScriptExecutionResponse(
                   testId,
                   "success",
                   outputObjectCollection:
                   new VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model.OutputObjectCollection("TextResult"),
                   dataStreams: new ScriptRuntimeService.RunspaceClient.Bindings.Model.ScriptExecutionStreams(
                      new List<ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord>(new[]{new ScriptRuntimeService.RunspaceClient.Bindings.Model.StreamRecord {
                         Time = DateTime.Today,
                         Message = "InfoStream"
                      }}))));
             var scriptExecution = new NamedScriptExecution(
                "testScript",
                scriptResult);
             var scriptExecutionOutput = new ScriptExecutionOutput(scriptResult);
             var scriptExecutionStreams = new ScriptExecutionDataStreams(scriptResult.Streams);

             // Act
             fileStoreProvider.WriteScriptExecution(scriptExecution);
             fileStoreProvider.WriteScriptExecutionOutput(scriptExecutionOutput);
             fileStoreProvider.WriteScriptExecutionDataStreams(scriptExecutionStreams);

             fileStoreProvider.Flush();

             // Assert
             Assert.IsTrue(fileSystem.Directory.Exists(Path.Combine(rootFolder, userId, testId)));
             Assert.IsTrue(fileSystem.File.Exists(Path.Combine(rootFolder, userId, testId, ScriptExecutionFileNames.ScriptExecution)));
             Assert.IsTrue(fileSystem.File.Exists(Path.Combine(rootFolder, userId, testId, ScriptExecutionFileNames.ScriptExecutionOutput)));
             Assert.IsTrue(fileSystem.File.Exists(Path.Combine(rootFolder, userId, testId, ScriptExecutionFileNames.ScriptExecutionStreams)));
          }
       }
   }
}
