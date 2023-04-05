// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;

namespace VMware.ScriptRuntimeService.AdminEngine.RunspaceProviders {
   public class RunspaceProviders {
      private readonly ILoggerFactory _loggerFactory;
      private readonly IConfigWriter _configWriter;
      private readonly IConfigReader _configReader;
      private readonly ILogger _logger;

      public RunspaceProviders(ILoggerFactory loggerFactory, IConfigWriter configWriter, IConfigReader configReader) {
         _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
         _logger = loggerFactory.CreateLogger(typeof(VCRegistrator));
         _configWriter = configWriter ?? throw new ArgumentNullException(nameof(configWriter));
         _configReader = configReader ?? throw new ArgumentNullException(nameof(configReader));
      }

      public RunspaceProviderSettings UpdateSettings(
         uint? numberOfRunspaces,
         uint? runspaceIdleTimeMinutes,
         uint? runspaceActiveTimeMinutes) {

         dynamic data = GetSettings();

         if (numberOfRunspaces.HasValue) {
            data.RunspaceProviderSettings.MaxNumberOfRunspaces = numberOfRunspaces.Value;
         }

         if (runspaceIdleTimeMinutes.HasValue) {
            data.RunspaceProviderSettings.MaxRunspaceIdleTimeMinutes = runspaceIdleTimeMinutes.Value;
         }

         if (runspaceActiveTimeMinutes.HasValue) {
            data.RunspaceProviderSettings.MaxRunspaceActiveTimeMinutes = runspaceActiveTimeMinutes.Value;
         }

         _configWriter.WriteSettings("service-settings", data, "settings.json");

         return GetSettings();
      }

      public RunspaceProviderSettings GetSettings() {
         dynamic data = _configReader.ReadSettings<dynamic>("service-settings", "settings.json");

         return new RunspaceProviderSettings() {
            MaxNumberOfRunspaces = data.RunspaceProviderSettings.MaxNumberOfRunspaces,
            MaxRunspaceIdleTimeMinutes = data.RunspaceProviderSettings.MaxRunspaceIdleTimeMinutes,
            MaxRunspaceActiveTimeMinutes = data.RunspaceProviderSettings.MaxRunspaceActiveTimeMinutes
         };
      }
   }
}
