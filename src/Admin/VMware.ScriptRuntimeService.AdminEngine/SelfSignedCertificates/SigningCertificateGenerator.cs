// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;

namespace VMware.ScriptRuntimeService.AdminEngine.SelfSignedCertificates {
   public class SigningCertificateGenerator : ISelfSignedCertificateGenerator {
      private ILoggerFactory _loggerFactory;
      private ILogger _logger;
      private string _certificateCommonName;
      private string _shellScript;
      private readonly string _assemblyDir;
      private IConfigWriter _certificateFileWriter;
      private const string signCertDir = "sign-cert";

      public SigningCertificateGenerator(
         ILoggerFactory loggerFactory,
         string certificateCommonName,
         IConfigWriter certificateFileWriter) {

         _loggerFactory = loggerFactory;
         _logger = loggerFactory.CreateLogger(typeof(SigningCertificateGenerator).FullName);
         _certificateCommonName = certificateCommonName;
         _assemblyDir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(SigningCertificateGenerator)).Location);
         _shellScript = Path.Combine(_assemblyDir, "shellscripts", "gen-signing-cert.sh");
         _certificateFileWriter = certificateFileWriter;
      }

      public X509Certificate2 Generate(string certName) {
         var actionResult = true;
         X509Certificate2 result = null;

         try {
            // Make shell script executable
            var commandRunner = new ShellCommandRunner(
               _loggerFactory,
               "/bin/bash",
               $"-c \"chmod +x '{_shellScript}'\"");

            var commandResult = commandRunner.Run();
            if (commandResult == null || commandResult.ExitCode != 0) {
               _logger.LogError($"Make {_shellScript} script executable failed:\n {commandResult.ErrorStream}");
               actionResult = false;
            }

            // Run shell script executable
            var outputCertDir = Path.Combine(_assemblyDir, signCertDir);
            var signCertName = certName;
            var signCertFormat = "p12";
            if (actionResult) {
               Directory.CreateDirectory(outputCertDir);
               commandRunner = new ShellCommandRunner(
               _loggerFactory,
               "/bin/bash",
               $"{_shellScript} {_certificateCommonName} {signCertName} {outputCertDir}");

               commandResult = commandRunner.Run();
               if (commandResult == null || commandResult.ExitCode != 0) {
                  _logger.LogError($"Command '{_shellScript} {_certificateCommonName} {outputCertDir}' failed:\n {commandResult.ErrorStream}");
                  actionResult = false;
               }
            }

            if (actionResult) {
               // Output certificate
               _certificateFileWriter.WriteBinaryFile("sign-cert", Path.Combine(_assemblyDir, signCertDir, $"{signCertName}.{signCertFormat}"));
               result = new X509Certificate2(Path.Combine(_assemblyDir, signCertDir, $"{signCertName}.{signCertFormat}"));
               DeleteLocalFiles();
            }
         } catch (Exception exc) {
            actionResult = false;
            _logger.LogError(exc, "Certificate generation failded");
         }

         return result;
      }

      private void DeleteLocalFiles() {
         var outputCertDir = Path.Combine(_assemblyDir, signCertDir);
         if (Directory.Exists(outputCertDir)) {
            Directory.Delete(outputCertDir, true);
         }
      }
   }
}
