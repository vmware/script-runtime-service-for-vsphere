// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using k8s.KubeConfigModels;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.Setup.SetupFlows;

namespace VMware.ScriptRuntimeService.Setup {
   internal class Program {
      private static int Main(string[] args) {
         using (var loggerFactory = LoggerFactory.Create(builder => {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter("VMware.ScriptRuntimeService.Setup", LogLevel.Debug)
                   .AddConsole();})) {

            var logger = loggerFactory.CreateLogger(typeof(Program));

            // Override Newtonsoft.Json MaxDepth de/serialization because
            // as of version 13.0.1 the default MaxDepth is 64
            Newtonsoft.Json.JsonConvert.DefaultSettings =
               () => new Newtonsoft.Json.JsonSerializerSettings() {
                  MaxDepth = int.MaxValue,
                  NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
               };

            try {
               var userInput = new ArgsParser().Parse(args);
               var setupFlow = SetupFlowFactory.Create(loggerFactory, userInput.Run);
               return setupFlow.Run(userInput);
            } catch (Exception exc) {
               logger.LogError(exc, "No valid input is extracted from environment variables and commandline arguments");
               return 1;
            }  
         }
      }      
   }
}
