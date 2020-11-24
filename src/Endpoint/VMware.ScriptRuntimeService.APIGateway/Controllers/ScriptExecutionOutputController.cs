// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers
{
   [Route("api/script-executions/{id}/output")]
   [Produces("application/json")]
   [Consumes("application/json")]
   [ApiController]
   public class ScriptExecutionOutputController : ControllerBase{
      // Get api/script-executions/{id}/output
      /// <summary>
      /// Retrieves output objects produced by a script execution.
      /// </summary>
      /// <remarks>
      /// ### Retrieves output objects produced by a script execution ###
      /// Output object could be in a different format depending on the requested output object format on **script execution** request.
      /// Text and JSON are currently supported.
      /// When Text format is requested, the objects produced by the script execution are formatted in a table, the same way Format-Table formats the objects in PowerShell.
      /// When JSON format is requested, a custom json formatting is used to serialize the objects produced by the script execution. The JSON objects contain the type name and the full name of the implementing interfaces.
      /// </remarks>
      /// <param name="id">Unique identifier of the script execution</param>      
      /// <returns></returns>
      [HttpGet(Name = "get.script.execution.output")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      // Out of scope for MVP release
      // Since the number of output object could be large the retrieval operation supports pagination.
      // There are two query parameters limit and start which can be used to move through the output objects list.
      // Start 0 means the last retrieved output object.
      // <param name="limit">Maximum number of records to be retrieved</param>
      // <param name="start">Position in the history of output records where the retrieval is requested from</param>
      //public ActionResult<string[]> Get([FromRoute] string id, [FromQuery] int limit = 20, [FromQuery] int start = 0) {
      public ActionResult<string[]> Get([FromRoute] string id) {
            ActionResult<string[]> result = null;
         IScriptExecutionOutputObjects scriptOutput = null;
         // Get Script Execution from ScriptExecutionMediator
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            scriptOutput = ScriptExecutionMediatorSingleton.
               Instance.
               ScriptExecutionMediator.
               GetScriptExecutionOutput(authzToken.UserName, id);

            if (scriptOutput != null) {
               result = Ok(scriptOutput.OutputObjects);
            } else {
               result = NotFound(
                  new ErrorDetails(
                     ApiErrorCodes.GetErrorCode(
                        nameof(APIGatewayResources.ScriptsController_ScriptNotFound)),
                     string.Format(APIGatewayResources.ScriptsController_ScriptNotFound, id)));
            }
         } catch (RunspaceEndpointException runspaceEndointException) {
            result = StatusCode(
               runspaceEndointException.HttpErrorCode,
               new ErrorDetails(
                  ApiErrorCodes.CalculateScriptsErrorCode(runspaceEndointException.ErrorCode),
                  runspaceEndointException.Message,
                  runspaceEndointException.ToString()));
         } catch (Exception exc) {
            result = StatusCode(
               500,
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(
                     nameof(APIGatewayResources.ScriptOutputController_ScriptStorageService_FailedToRetrieveScriptOutput)),
                  APIGatewayResources.ScriptOutputController_ScriptStorageService_FailedToRetrieveScriptOutput,
                  exc.ToString()));
         }
         return result;
      }
   }
}