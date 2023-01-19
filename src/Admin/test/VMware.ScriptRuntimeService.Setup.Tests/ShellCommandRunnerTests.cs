// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using VMware.ScriptRuntimeService.AdminEngine;

namespace VMware.ScriptRuntimeService.Setup.Tests
{
   public class ShellCommandRunnerTests {
      Mock<ILoggerFactory> _loggerFactoryMock;
      [SetUp]
      public void Setup() {

         _loggerFactoryMock = new Mock<ILoggerFactory>();
         _loggerFactoryMock.Setup(
            x => x.CreateLogger(typeof(ShellCommandRunner).FullName))
            .Returns(new Mock<ILogger>().Object);
      }

      [Test]
      public void RunDirCommandInPowerShell() {
         // Arrange
         var shell = "pwsh";
         var arguments = "--command dir";
         var actor = new ShellCommandRunner(_loggerFactoryMock.Object, shell, arguments);

         // Act
         var actual = actor.Run();

         // Assert
         Assert.NotNull(actual);
         Assert.AreEqual(0, actual.ExitCode);
         Assert.NotNull(actual.OutputStream);
         Assert.IsEmpty(actual.ErrorStream);
      }

      [Test]
      public void RunCommandThatTimesOut() {
         // Arrange
         var shell = "pwsh";
         var arguments = "--command 'Start-Sleep -Seconds 20'";
         var actor = new ShellCommandRunner(_loggerFactoryMock.Object, shell, arguments, 100);

         // Act
         var actual = actor.Run();

         // Assert
         Assert.IsNull(actual);         
      }

      [Test]
      public void RunWrongCommand() {
         // Arrange
         var shell = "inexistent";
         var arguments = "fake fake fake";
         var actor = new ShellCommandRunner(_loggerFactoryMock.Object, shell, arguments);

         /// Logger LogError mock is not supported by Moq because it is ExtensionMethod
         //var loggerMock = new Mock<ILogger>();
         //loggerMock.Setup(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()));

         //_loggerFactoryMock.Setup(
         //   x => x.CreateLogger(typeof(ShellCommandRunner).FullName))
         //   .Returns(loggerMock.Object);

         //loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once());         

         // Act
         var actual = actor.Run();

         // Assert
         Assert.IsNull(actual);
      }
   }
}
