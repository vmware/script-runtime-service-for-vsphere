// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SesClient
{
   class Program
   {
      private static string _usage = @"Usage: dotnet SesClient <ses address> <username> <password> <PowerCLI script>
Example: SesClient.exe https://10.23.82.159 administrator@vsphere.local Admin!23 Get-Folder";
      static void Main(string[] args) {

         if (args == null || args.Length != 4) {
            Console.WriteLine(_usage);
            Environment.Exit(2);
         }

         var sesAddress = args[0];
         var username = args[1];
         var password = args[2];
         var scriptText = args[3];

         if (string.IsNullOrEmpty(sesAddress) ||
            string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(scriptText)) {
            Console.WriteLine(_usage);
            Environment.Exit(2);
         }

         try {

            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, certificate, chain, sslPolicyErrors) => true;

            // Login with username and password
            Configuration config = new Configuration {
               BasePath = sesAddress,
               Username = username,
               Password = password
            };
            AuthenticationApi authApi = new AuthenticationApi(config);
            var loginResponse = authApi.LoginWithHttpInfo();
            var sesApiKey = loginResponse.Headers["X-SRS-API-KEY"];

            config = new Configuration {
               BasePath = sesAddress,
               ApiKey = new Dictionary<string, string> {
               { "X-SRS-API-KEY", sesApiKey}
            }
            };

            // Create Runspace
            RunspacesApi runspaceApi = new RunspacesApi(config);
            var runspace = runspaceApi.CreateRunspace(new Runspace {
               Name = "MyPSRunsapce",
               RunVcConnectionScript = true
            });

            while (runspace.State == RunspaceState.Creating) {
               Thread.Sleep(500);
               runspace = runspaceApi.GetRunspace(runspace.Id);
            }

            if (runspace.State == RunspaceState.Error) {
               Console.WriteLine($"Error on runspace creation: {runspace.ErrorDetails.Details}");
               Environment.Exit(3);
            }

            // Run Script
            ScriptexecutionsApi scriptExecutionsApi = new ScriptexecutionsApi(config);
            var scriptExecution = scriptExecutionsApi.CreateScriptExecution(
               new ScriptExecution(runspace.Id, "MyScript", scriptText));
               

            while (scriptExecution.State == ScriptExecutionState.Running) {
               Thread.Sleep(500);
               scriptExecution = scriptExecutionsApi.GetScriptExecution(scriptExecution.Id);
            }

            if (scriptExecution.State == ScriptExecutionState.Error) {
               Console.WriteLine($"Error on script execution: {scriptExecution.Reason}");
               Environment.Exit(4);
            }

            // Read Script Output
            var scriptOutput = scriptExecutionsApi.GetScriptExecutionOutput(scriptExecution.Id);
            if (scriptOutput != null && scriptOutput.Count > 0) {
               Console.WriteLine("Script Output:");
               foreach (var output in scriptOutput) {
                  Console.WriteLine(output);
               }
            }

            // Read Script Errors   
            var scriptErrorRecords = scriptExecutionsApi.GetScriptExecutionStream(scriptExecution.Id, StreamType.Error);
            if (scriptErrorRecords != null && scriptErrorRecords.Count > 0) {
               Console.WriteLine("Script Error:");
               foreach (var errorRecord in scriptErrorRecords) {
                  Console.WriteLine(errorRecord.Message);
               }
            }

            // Delete Runspace
            runspaceApi.DeleteRunspace(runspace.Id);
         } catch (Exception exc) {
            Console.WriteLine($"Error: {exc}");
            Environment.Exit(100);
         }
      }
   }
}
