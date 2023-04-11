// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl;
using VMware.ScriptRuntimeService.APIGateway.Sts;
using VMware.ScriptRuntimeService.APIGateway.Sts.Impl;
using ErrorDetails = VMware.ScriptRuntimeService.APIGateway.DataTypes.ErrorDetails;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers
{
   [Route("api/webconsoles")]
   [Produces("application/json")]
   [Consumes("application/json")]
   [ApiController]
   public class WebConsoleController : ControllerBase {
      private readonly StsSettings _stsSettings = new StsSettings();
      private readonly ILoggerFactory _loggerFactory;
      private readonly ILogger _logger;
      private readonly IConfiguration _configuration;

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


         } catch (Exception e) {
            _logger.LogError(e, "Creating web console operation failed.");
            result = StatusCode(500, new ErrorDetails(e));
         }

         return result;
      }

      // GET api/webconsole
      /// <summary>
      /// List all webconsoles
      /// </summary>
      /// <remarks>
      /// </remarks>
      [HttpGet(Name = "list-webconsoles")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(DataTypes.WebConsole[]), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult<WebConsole[]> List() {
         ActionResult<WebConsole[]> result = null;

         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);
            var webConsoleDataList =
               RunspaceProviderSingleton.
                  Instance.
                  RunspaceProvider.
                  ListWebConsole(authzToken.UserName);
            if (webConsoleDataList != null) {
               List<WebConsole> webConsoleResponseList = new List<WebConsole>();
               foreach (var webConsoleData in webConsoleDataList) {
                  webConsoleResponseList.Add(new WebConsole(webConsoleData));
               }

               result = Ok(webConsoleResponseList.ToArray());
            } else {
               result = Ok(new DataTypes.Runspace[] { });
            }

         } catch (Exception e) {
            _logger.LogError(e, "List web consoles operation failed.");
            result = StatusCode(
               500,
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(
                     nameof(APIGatewayResources.WebConsoleController_List_RunspaceProviderListWebConsoleFailed)),
                  APIGatewayResources.WebConsoleController_List_RunspaceProviderListWebConsoleFailed,
                  e.ToString()));
         }
         return result;
      }


      // GET api/webconsole/{id}
      /// <summary>
      /// Retrieve a web console
      /// </summary>
      /// <param name="id">Unique identifier of the web console</param>
      /// <remarks>
      /// Retrieves the details of a web console. One only needs to supply the unique web console identifier returned on the web console creation to retrieve the web console details.
      ///
      /// Returns a **webconsole** resource instance if a valid identifier was provided.
      /// When requesting the Id of a web console that has been deleted or doesn't exist **404 NotFound** is returned.
      /// </remarks>
      [HttpGet("{id}", Name = "get-webconsole")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(WebConsole), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      public ActionResult<WebConsole> Get([FromRoute] string id) {
         ActionResult<WebConsole> result = null;

         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);
            var webConsoleData =
               RunspaceProviderSingleton.
                  Instance.
                  RunspaceProvider.
                  GetWebConsole(authzToken.UserName, id);
            result = Ok(new WebConsole(webConsoleData));
         } catch (Exception e) {
            _logger.LogError(e, "Get web console operation failed.");
            result = NotFound(
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(nameof(APIGatewayResources.WebConsoleNotFound)),
                  string.Format(
                     APIGatewayResources.WebConsoleNotFound,
                     id)));
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
                     nameof(APIGatewayResources.WebConsoleController_Kill_RunspaceProviderKillWebConsoleFailed)),
                  string.Format(
                     APIGatewayResources.WebConsoleController_Kill_RunspaceProviderKillWebConsoleFailed,
                     id),
                  exc.ToString()));
         }

         return result;
      }
   }
}
