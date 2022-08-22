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
            // this keeps pre 13.0.1 behaviour
            Func<Newtonsoft.Json.JsonSerializerSettings> defaultSettingsFunc =
               Newtonsoft.Json.JsonConvert.DefaultSettings;
            if (null == Newtonsoft.Json.JsonConvert.DefaultSettings) {
               defaultSettingsFunc =
                  () => new Newtonsoft.Json.JsonSerializerSettings() {
                     MaxDepth = int.MaxValue
                  };
            } else {
               var settings = defaultSettingsFunc();
               defaultSettingsFunc =
                  () => {
                     settings.MaxDepth = int.MaxValue;
                     return settings;
                  };
            }

            Newtonsoft.Json.JsonConvert.DefaultSettings = defaultSettingsFunc;

            try {
               var userInput = new ArgsParser().Parse(args);
               var setupFlow = SetupFlowFactory.Create(loggerFactory, userInput.Run);
               return setupFlow.Run(userInput);
            } catch (Exception exc) {
               logger.LogError(exc, "No valid input is extracted from environment variables and command line arguments");
               return 1;
            }
         }
      }
   }
}
