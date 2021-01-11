﻿// **************************************************************************
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
      /// Starts a runspace creation
      /// </summary>
      /// <remarks>      
      /// Runspace creation and preparation time depends on the requested runspace details.
      /// If a connection to the vCenter Servers is requested, the operation creates a PowerShell instance, loads PowerCLI modules, and connects PowerCLI to the vCenter Servers.
      /// 
      /// Returns
      /// 
      /// When request is accepted **202 Accepted** - response code, with **Location** header is returned in the response that leads you to the **runspace** resource. The **runspace** resource is in **creation** state initially.
      /// </remarks>
      /// <param name="runspace">Desired runspace resource.</param>
      /// <returns>Runspace resource to monitor the requested runspace. Once runspace becomes in **ready** state you will be able to run scripts in it.</returns>
      [HttpPost(Name = "create-webconsole")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(DataTypes.Runspace), StatusCodes.Status202Accepted)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult<DataTypes.Runspace> Post([FromBody]DataTypes.Runspace runspace) {         
         ActionResult<DataTypes.Runspace> result = null;
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            if (RunspaceProviderSingleton.Instance.RunspaceProvider.CanCreateNewRunspace()) {
               var runspaceData = RunspaceProviderSingleton.Instance.RunspaceProvider.StartCreateWebConsole(
                     authzToken.UserName, 
                     authzToken, 
                     runspace.Name, 
                     runspace.RunVcConnectionScript,
                     new SolutionStsClient(_loggerFactory, _stsSettings),
                     // Assumes VC Address is same as STS Address
                     new Uri(_stsSettings.StsServiceEndpoint).Host);

               result = StatusCode(202, new DataTypes.Runspace(runspaceData));
            } else {
               _logger.LogInformation($"Runspace provider can't create new runspaces: {APIGatewayResources.RunspaceController_Post_MaxnumberOfRunspacesReached}");
               result = StatusCode(
                  500, 
                  new ErrorDetails(
                     ApiErrorCodes.GetErrorCode(nameof(APIGatewayResources.RunspaceController_Post_MaxnumberOfRunspacesReached)),
                     APIGatewayResources.RunspaceController_Post_MaxnumberOfRunspacesReached));
            }
            

         } catch (Exception exc) {
            result = StatusCode(500, new ErrorDetails(exc));
         }

         return result;
      }

   
      // DELETE api/webconsole/{id}
      /// <summary>
      /// Deletes a runspace
      /// </summary>
      /// <param name="id">Unique identifier of the runspace</param>
      /// <remarks>
      /// Deletes the PowerShell instance that is prepresented by this **runspace** resource.
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