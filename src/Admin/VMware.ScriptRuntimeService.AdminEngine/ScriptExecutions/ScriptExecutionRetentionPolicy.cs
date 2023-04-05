// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;
using VMware.ScriptRuntimeService.AdminEngine.ScriptExecutions;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;

namespace VMware.ScriptRuntimeService.AdminEngine.ScriptExecutions {
   public class ScriptExecutionRetentionPolicy {
      private readonly ILoggerFactory _loggerFactory;
      private readonly IConfigWriter _configWriter;
      private readonly IConfigReader _configReader;
      private readonly ILogger _logger;

      public ScriptExecutionRetentionPolicy(ILoggerFactory loggerFactory, IConfigWriter configWriter, IConfigReader configReader) {
         _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
         _logger = loggerFactory.CreateLogger(typeof(VCRegistrator));
         _configWriter = configWriter ?? throw new ArgumentNullException(nameof(configWriter));
         _configReader = configReader ?? throw new ArgumentNullException(nameof(configReader));
      }

      public RetentionPolicy UpdatePolicy(
         uint? numberOfScriptsPerUser,
         uint? noOlderThanDays) {

         dynamic data = GetPolicy();

         if (numberOfScriptsPerUser.HasValue) {
            data.ScriptExecutionStorageSettings.NumberOfScriptsPerUser = numberOfScriptsPerUser.Value;
         }

         if (noOlderThanDays.HasValue) {
            data.ScriptExecutionStorageSettings.NoOlderThanDays = noOlderThanDays.Value;
         }

         _configWriter.WriteSettings("service-settings", data, "settings.json");

         return GetPolicy();
      }

      public RetentionPolicy GetPolicy() {
         dynamic data = _configReader.ReadSettings<dynamic>("service-settings", "settings.json");

         return new RetentionPolicy() {
            MaxNumberOfScriptsPerUser = data.ScriptExecutionStorageSettings.NumberOfScriptsPerUser,
            NoOlderThanDays = data.ScriptExecutionStorageSettings.NoOlderThanDays
         };
      }
   }
}
