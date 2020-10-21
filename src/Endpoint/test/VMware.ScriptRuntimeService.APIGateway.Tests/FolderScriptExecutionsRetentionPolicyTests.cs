// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.RetentionPolicy;

namespace VMware.ScriptRuntimeService.APIGateway.Tests {
   public class FolderScriptExecutionsRetentionPolicyTests {
      private IFileSystem _fileSystem;
      private string _rootFolder;

      [SetUp]
      public void SetupScriptFiles() {
         var tomorrow = DateTime.Today + new TimeSpan(1, 0, 0, 0);
         _rootFolder = Path.Combine("c", "scripts");
         _fileSystem = new MockFileSystem();
         var userName = "user1";
         for (int i = 1; i <= 25; i++) {
            // Setup different user folders
            if (i % 5 == 0) {
               userName = $"user{i}";
            }

            var scriptFolder = Path.Combine(_rootFolder, userName, $"script{i}");

            _fileSystem.Directory.CreateDirectory(scriptFolder);
            _fileSystem.File.Create(Path.Combine(scriptFolder, "execution.txt"));
            _fileSystem.File.Create(Path.Combine(scriptFolder, "executionOutput.txt"));

            _fileSystem.Directory.SetLastWriteTime(scriptFolder, tomorrow - new TimeSpan(i, 0, 0, 0));
         }
      }

      [Test]
      public void ApplyRemovesUserScriptFoldersThatMatchRules() {
         // Arrange
         var policy = new FolderScriptExecutionsRetentionPolicy(
            new Mock<ILoggerFactory>().Object,
            _fileSystem,
            _rootFolder,
            new IScriptsRetentionRule[] {
               new LastNumberOfRecordsRetentionRule(3),
               new OlderThanRetentionRule(new TimeSpan(9, 0, 0, 0))
            });

         // Act
         policy.Apply();

         // Assert
         //  Last 3 scripts for user1 exist
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user1", "script1")));
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user1", "script2")));
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user1", "script3")));
         Assert.IsFalse(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user1", "script4")));

         //  Last 3 scripts for user5 exist
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user5", "script5")));
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user5", "script6")));
         Assert.IsTrue(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user5", "script7")));
         Assert.IsFalse(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user5", "script8")));
         Assert.IsFalse(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, "user5", "script9")));

         //  All other scripts are older than 9 days, so they shouldn't exist
         var userName = "user1";
         for (int i = 10; i <= 25; i++) {
            if (i % 5 == 0) {
               userName = $"user{i}";
            }

            Assert.IsFalse(_fileSystem.Directory.Exists(Path.Combine(_rootFolder, userName, $"script{i}")));
         }
      }

      [Test]
      public void ApplyWhenNoScriptsFolder() {
         // Arrange
         var policy = new FolderScriptExecutionsRetentionPolicy(
            new Mock<ILoggerFactory>().Object,
            _fileSystem,
            Path.Combine("c", "nofolder"),
            new IScriptsRetentionRule[] {
               new LastNumberOfRecordsRetentionRule(3),
               new OlderThanRetentionRule(new TimeSpan(9, 0, 0, 0))
            });

         // Act
         // Assert
         Assert.DoesNotThrow(() => policy.Apply());

         Assert.IsTrue(_fileSystem.Directory.Exists(_rootFolder));
      }

      [Test]
      public void ApplyWhenEmptyScriptsFolder() {
         // Arrange
         var emptyRoot = Path.Combine("c", "emptyfolder");
         _fileSystem.Directory.CreateDirectory(emptyRoot);
         var policy = new FolderScriptExecutionsRetentionPolicy(
            new Mock<ILoggerFactory>().Object,
            _fileSystem,
            emptyRoot,
            new IScriptsRetentionRule[] {
               new LastNumberOfRecordsRetentionRule(3),
               new OlderThanRetentionRule(new TimeSpan(9, 0, 0, 0))
            });

         // Act
         // Assert
         Assert.DoesNotThrow(() => policy.Apply());

         Assert.IsTrue(_fileSystem.Directory.Exists(_rootFolder));
      }

      [Test]
      public void ApplyWhenEmptyUserFolders() {
         // Arrange
         var policy = new FolderScriptExecutionsRetentionPolicy(
            new Mock<ILoggerFactory>().Object,
            _fileSystem,
            _rootFolder,
            new IScriptsRetentionRule[] {
               new LastNumberOfRecordsRetentionRule(3),
               new OlderThanRetentionRule(new TimeSpan(9, 0, 0, 0))
            });

         var emptyUserFolder = Path.Combine(_rootFolder, "noScriptsUser");
         _fileSystem.Directory.CreateDirectory(emptyUserFolder);

         // Act
         // Assert
         Assert.DoesNotThrow(() => policy.Apply());
         Assert.IsTrue(_fileSystem.Directory.Exists(emptyUserFolder));
      }
   }
}
