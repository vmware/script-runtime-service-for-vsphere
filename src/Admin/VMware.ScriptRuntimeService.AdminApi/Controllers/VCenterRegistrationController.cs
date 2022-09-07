// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;
using VMware.ScriptRuntimeService.AdminEngine.K8sClient;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;

namespace VMware.ScriptRuntimeService.AdminApi.Controllers {
   [ApiController]
   [Route("admin/vc-registration")]
   [Produces("application/json")]
   [Consumes("application/json")]
   public class VCenterRegistrationController : ControllerBase {
      private readonly ILoggerFactory _loggerFactory;
      private readonly IK8sController _k8sController;
      private readonly ILogger _logger;
      private readonly IConfiguration _configuration;
      private readonly Settings _settings;
      private readonly K8sSettings _k8sSettings;

      public VCenterRegistrationController(IConfiguration Configuration, ILoggerFactory loggerFactory, IK8sController k8sController) {
         _configuration = Configuration;
         _settings = _configuration.
               GetSection("Settings").
               Get<Settings>();
         _k8sSettings = _configuration.
               GetSection("K8sSettings").
               Get<K8sSettings>();

         _loggerFactory = loggerFactory;
         _k8sController = k8sController;
         _logger = _loggerFactory.CreateLogger(typeof(VCenterRegistrationController));
      }

      [HttpPost]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult Post([FromBody] VCInfo vcInfo) {
         ActionResult result = null;

         try {

            var configWriter = new K8sConfigWriter(_loggerFactory, _k8sSettings);
            var vcRegistrator = new VCRegistrator(_loggerFactory, configWriter);

            vcRegistrator.Register(
               vcInfo.Address,
               vcInfo.UserName,
               SecurePassword(vcInfo.Password),
               vcInfo.Thumbprint,
               false);

            // --- Restart SRS API Gateway ---
            _k8sController.WithUpdateK8sSettings(_k8sSettings).RestartSrsService();
            // --- Restart SRS API Gateway ---

            result = Ok();
         } catch (Exception ex) {
            result = StatusCode(500, new ErrorDetails(ex));
         }

         return result;
      }

      [HttpGet]
      [ProducesResponseType(typeof(VCInfo), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult<VCInfo> Get() {
         ActionResult<VCInfo> result;
         try {
            var configWriter = new K8sConfigWriter(_loggerFactory, _k8sSettings);
            var vcRegSettings = configWriter.ReadSettings<VCenterStsSettings>(_settings.ConfigMap);
            result = Ok(new VCInfo() {
               Address = vcRegSettings?.VCenterAddress
            });
         } catch (Exception ex) {
            result = StatusCode(500, new ErrorDetails(ex));
         }

         return result;
      }

      [HttpDelete]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult Delete([FromBody] VCInfo vcInfo, [FromQuery] bool clean) {
         ActionResult result = null;
         try {
            _logger.LogDebug($"User Input VC: {vcInfo.Address}");
            _logger.LogDebug($"User Input VC User: {vcInfo.UserName}");
            _logger.LogDebug($"User Input VC Thumbprint: {vcInfo.Thumbprint}");

            var secureVcPassword = SecurePassword(vcInfo.Password);

            var configWriter = new K8sConfigWriter(_loggerFactory, _k8sSettings);
            var vcRegistrator = new VCRegistrator(_loggerFactory, configWriter);

            var vcSettings = configWriter.ReadSettings<VCenterStsSettings>(_settings.ConfigMap);
            if (vcSettings != null &&
                vcSettings.VCenterAddress == vcInfo.Address) {

               if (clean) {
                  vcRegistrator.Clean(vcInfo.Address, vcInfo.UserName, secureVcPassword, vcInfo.Thumbprint, false);
               } else {
                  vcRegistrator.Unregister(vcInfo.Address, vcInfo.UserName, secureVcPassword, vcInfo.Thumbprint, false);
               }

               // --- Restart SRS API Gateway ---
               _k8sController.WithUpdateK8sSettings(_k8sSettings).RestartSrsService();
               // --- Restart SRS API Gateway ---
               result = Ok();
            } else {
               result = StatusCode(StatusCodes.Status404NotFound, new ErrorDetails(new Exception($"No SRS registration found for vCenter Server: {vcInfo.Address}")));
            }
         } catch (Exception ex) {
            result = StatusCode(500, new ErrorDetails(ex));
         }
         return result;
      }

      private SecureString SecurePassword(string password) {
         SecureString result = null;
         if (!string.IsNullOrEmpty(password)) {
            var securePassword = new SecureString();
            foreach (char c in password) {
               securePassword.AppendChar(c);
            }
            result = securePassword;
         }

         return result;
      }
   }
}
