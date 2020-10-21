// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class FolderPersistedScriptExecutionRecordTests {
      [Test]
      public void RemoveDeletesFolderWithFiles() {
         // Arrange
         var logger = new Mock<ILogger>().Object;

         var rootFolder = Path.Combine("c", "scripts");
         var fileSystem = new MockFileSystem();
         foreach (var script in new[] {"script1", "script2"}) {
            var scriptFolder = Path.Combine(rootFolder, script);
            fileSystem.Directory.CreateDirectory(scriptFolder);
            fileSystem.File.Create(Path.Combine(scriptFolder, "execution.txt"));
            fileSystem.File.Create(Path.Combine(scriptFolder, "executionOutput.txt"));
            fileSystem.File.Create(Path.Combine(scriptFolder, "executionStreams.txt"));
         }

         var scriptRecord = new FolderPersistedScriptExecutionRecord(logger, fileSystem, Path.Combine(rootFolder, "script1"));

         // Act
         scriptRecord.Remove();

         // Assert
         Assert.IsFalse(fileSystem.Directory.Exists(Path.Combine(rootFolder, "script1")));
         Assert.IsTrue(fileSystem.Directory.Exists(Path.Combine(rootFolder, "script2")));
         Assert.IsTrue(fileSystem.FileExists(Path.Combine(rootFolder, "script2", "execution.txt")));
         Assert.IsTrue(fileSystem.FileExists(Path.Combine(rootFolder, "script2", "executionOutput.txt")));
         Assert.IsTrue(fileSystem.FileExists(Path.Combine(rootFolder, "script2", "executionStreams.txt")));
      }

      [Test]
      public void RemoveCantDeleteFolderWithFolder() {
         // Arrange
         var logger = new Mock<ILogger>().Object;

         var fileSystem = new MockFileSystem();
         var scriptFolder = Path.Combine("c", "scripts", "script1");
         fileSystem.Directory.CreateDirectory(scriptFolder);
         fileSystem.Directory.CreateDirectory(Path.Combine(scriptFolder, "child"));

         var scriptRecord = new FolderPersistedScriptExecutionRecord(logger, fileSystem, scriptFolder);

         // Act
         scriptRecord.Remove();

         // Assert
         Assert.IsTrue(fileSystem.Directory.Exists(scriptFolder));
      }
   }
}
