// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;
using VMware.ScriptRuntimeService.AdminEngine.K8sClient;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;

namespace VMware.ScriptRuntimeService.AdminApi.Controllers {
   [Authorize]
   [ApiController]
   [Route("admin/runspace-providers/settings")]
   [Produces("application/json")]
   [Consumes("application/json")]
   public class RunspaceProvidersSettingsController : ControllerBase {
      private readonly ILoggerFactory _loggerFactory;
      private readonly IK8sController _k8sController;
      private readonly ILogger _logger;
      private readonly IConfiguration _configuration;
      private readonly K8sSettings _k8sSettings;

      public RunspaceProvidersSettingsController(IConfiguration Configuration, ILoggerFactory loggerFactory, IK8sController k8sController) {
         _configuration = Configuration;
         _k8sSettings = _configuration.
               GetSection("K8sSettings").
               Get<K8sSettings>();

         _loggerFactory = loggerFactory;
         _k8sController = k8sController;
         _logger = _loggerFactory.CreateLogger(typeof(VCenterRegistrationController));
      }

      [HttpPatch]
      [ProducesResponseType(typeof(RunspaceProviderSettings), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult<RunspaceProviderSettings> Patch([FromBody] RunspaceProviderSettings settings) {
         ActionResult<RunspaceProviderSettings> result = null;

         try {
            var configProxy = new K8sConfigRepository(_loggerFactory, _k8sSettings);
            var providers = new AdminEngine.RunspaceProviders.RunspaceProviders(_loggerFactory, configProxy, configProxy);

            var updatedSettings = providers.UpdateSettings(
               settings.MaxNumberOfRunspaces,
               settings.MaxRunspaceIdleTimeMinutes,
               settings.MaxRunspaceActiveTimeMinutes);

            // --- Restart SRS API Gateway ---
            _k8sController.WithUpdateK8sSettings(_k8sSettings).RestartSrsService();
            // --- Restart SRS API Gateway ---

            result = Ok(new RunspaceProviderSettings() {
               MaxNumberOfRunspaces = updatedSettings.MaxNumberOfRunspaces,
               MaxRunspaceActiveTimeMinutes = updatedSettings.MaxRunspaceActiveTimeMinutes,
               MaxRunspaceIdleTimeMinutes = updatedSettings.MaxRunspaceIdleTimeMinutes
            });
         } catch (Exception ex) {
            result = StatusCode(500, new ErrorDetails(ex));
         }

         return result;
      }

      [HttpGet]
      [ProducesResponseType(typeof(RunspaceProviderSettings), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult<RunspaceProviderSettings> Get() {
         ActionResult<RunspaceProviderSettings> result;
         try {
            var configProxy = new K8sConfigRepository(_loggerFactory, _k8sSettings);
            var providers = new AdminEngine.RunspaceProviders.RunspaceProviders(_loggerFactory, configProxy, configProxy);

            var updatedSettings = providers.GetSettings();

            result = Ok(new RunspaceProviderSettings() {
               MaxNumberOfRunspaces = updatedSettings.MaxNumberOfRunspaces,
               MaxRunspaceActiveTimeMinutes = updatedSettings.MaxRunspaceActiveTimeMinutes,
               MaxRunspaceIdleTimeMinutes = updatedSettings.MaxRunspaceIdleTimeMinutes
            });
         } catch (SrsNotRegisteredException ex) {
            result = StatusCode(404, new ErrorDetails(ex) { Code = 404 });
         } catch (Exception ex) {
            result = StatusCode(500, new ErrorDetails(ex));
         }

         return result;
      }
   }
}
