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