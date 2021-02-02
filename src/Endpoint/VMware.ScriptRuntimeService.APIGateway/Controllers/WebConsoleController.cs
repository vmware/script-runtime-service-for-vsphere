// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.Runspace;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl;
using VMware.ScriptRuntimeService.APIGateway.Sts;
using VMware.ScriptRuntimeService.APIGateway.Sts.Impl;
using VMware.ScriptRuntimeService.Docker.Bindings.Model;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using ErrorDetails = VMware.ScriptRuntimeService.APIGateway.DataTypes.ErrorDetails;
using Task = System.Threading.Tasks.Task;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers
{
   [Route("api/webconsoles")]
   [Produces("application/json")]
   [Consumes("application/json")]
   [ApiController]
   public class WebConsoleController : ControllerBase {
      private StsSettings _stsSettings = new StsSettings();
      private ILoggerFactory _loggerFactory;
      private ILogger _logger;
      private IConfiguration _configuration;

      public WebConsoleController(IConfiguration Configuration, ILoggerFactory loggerFactory) {
         _configuration = Configuration;
         _configuration.Bind("StsSettings", _stsSettings);
         _loggerFactory = loggerFactory;
         _logger = _loggerFactory.CreateLogger(typeof(WebConsoleController));
      }

      // POST api/webconsole
      /// <summary>
      /// Creates a PowerShell console accessible as web page
      /// </summary>
      /// <remarks>      
      /// Web console is accessble on the SRS Url/web-console-Id      
      /// </remarks>      
      /// <returns>A Web Csonole resource.</returns>
      [HttpPost(Name = "create-webconsole")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(WebConsole), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult<WebConsole> Post() {         
         ActionResult<WebConsole> result = null;
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            if (RunspaceProviderSingleton.Instance.RunspaceProvider.CanCreateNewWebConsole()) {
               var webConsoleData = RunspaceProviderSingleton.Instance.RunspaceProvider.CreateWebConsole(
                     authzToken.UserName, 
                     authzToken,                      
                     new SolutionStsClient(_loggerFactory, _stsSettings),
                     // Assumes VC Address is same as STS Address
                     new Uri(_stsSettings.StsServiceEndpoint).Host);

               result = StatusCode(200, new WebConsole(webConsoleData));
            } else {
               _logger.LogInformation($"Runspace provider can't create new web console: {APIGatewayResources.RunspaceController_Post_MaxnumberOfRunspacesReached}");
               result = StatusCode(
                  500, 
                  new ErrorDetails(
                     ApiErrorCodes.GetErrorCode(nameof(APIGatewayResources.WebConsoleController_Post_MaxNumberOfWebConsolesReached)),
                     APIGatewayResources.WebConsoleController_Post_MaxNumberOfWebConsolesReached));
            }
            

         } catch (Exception exc) {
            result = StatusCode(500, new ErrorDetails(exc));
         }

         return result;
      }

   
      // DELETE api/webconsole/{id}
      /// <summary>
      /// Deletes a web console
      /// </summary>
      /// <param name="id">Unique identifier of the web console</param>
      /// <remarks>
      /// Deletes the PowerShell instance that is prepresented by this **webconsole** resource.
      /// Running script in the PowerShell instance won't prevent the operation.      
      /// </remarks>
      [HttpDelete("{id}", Name = "delete-webconsole")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult Delete([FromRoute] string id) {
         ActionResult result;

         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);
            RunspaceProviderSingleton.Instance.RunspaceProvider.KillWebConsole(authzToken.UserName, id);
            result = Ok();
         } catch (Exception exc) {
            _logger.LogError(exc, "Delete runspace operation failed.");
            result = StatusCode(
               500, 
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(
                     nameof(APIGatewayResources.RunspaceController_Kill_RunspaceProviderKillFailed)),
                  string.Format(
                     APIGatewayResources.RunspaceController_Kill_RunspaceProviderKillFailed,
                     id),
                  exc.ToString()));
         }

         return result;
      }
   }
}
