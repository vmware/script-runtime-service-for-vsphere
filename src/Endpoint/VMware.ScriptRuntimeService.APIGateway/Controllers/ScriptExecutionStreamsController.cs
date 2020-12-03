// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers
{
   [Route("api/script-executions/{id}/streams/{stream-type}")]
   [Produces("application/json")]
   [Consumes("application/json")]
   [ApiController]
   public class ScriptExecutionStreamsController : ControllerBase {
      // Get api/script-executions/{id}/streams/{stream-type}
      /// <summary>
      /// Retrieves list of stream records received during script execution.
      /// </summary>
      /// <remarks>
      /// During the execution of a script, the script execution engine collects streams that are produced by the running script.
      /// There are five stream types: information, error, warning, debug, verbose.
      /// </remarks>
      /// <param name="id">Unique identifier of the script execution</param>
      /// <param name="streamType">Type of the stream for which records to be rterieved</param>
      /// <returns></returns>
      [HttpGet(Name = "get-script-execution-stream")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(StreamRecord[]), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      // Out of scope for MVP release
      // Since the stream messages number could be large depending on the script the operation uses pagination.
      // There are two query parameters limit and start which can be used to move through the stream records list. Record at index 0 is the last registered stream records. Moving up increasing the start of the stream records moves you back in the history of stream records.
      // <param name="limit">Maximum number of records to be retrieved</param>
      // <param name="start">Position in the history of records where the records retrieval is requested from.</param>
      // public ActionResult<StreamRecord[]> Get([FromRoute] string id, [FromRoute(Name ="stream-type")] StreamType streamType, [FromQuery] int limit = 20, [FromQuery] int start = 0) {
      public ActionResult<StreamRecord[]> Get([FromRoute] string id, [FromRoute(Name = "stream-type")] StreamType streamType) {
         ActionResult<StreamRecord[]> result = null;
          IScriptExecutionDataStreams scriptResult = null;
          // Get Script Execution from ScriptExecutionMediator
          try {
             var authzToken = SessionToken.FromHeaders(Request.Headers);

             scriptResult = ScriptExecutionMediatorSingleton.
                Instance.
                ScriptExecutionMediator.
                GetScriptExecutionDataStreams(authzToken.UserName, id);

             if (scriptResult != null) {
                switch(streamType) {
                  case StreamType.debug:
                     result = Ok(                   
                      StreamRecord.FromStreamRecords(scriptResult.Streams.Debug));
                     break;
                  case StreamType.error:
                     result = Ok(
                      StreamRecord.FromStreamRecords(scriptResult.Streams.Error));
                     break;
                  case StreamType.information:
                     result = Ok(
                      StreamRecord.FromStreamRecords(scriptResult.Streams.Information));
                     break;
                  case StreamType.verbose:
                     result = Ok(
                      StreamRecord.FromStreamRecords(scriptResult.Streams.Verbose));
                     break;
                  case StreamType.warning:
                     result = Ok(
                      StreamRecord.FromStreamRecords(scriptResult.Streams.Warning));
                     break;
               }               
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
                     nameof(APIGatewayResources.ScriptStreamsController_ScriptStorageService_FailedToRetrieveScriptStreams)),
                  APIGatewayResources.ScriptStreamsController_ScriptStorageService_FailedToRetrieveScriptStreams,
                  exc.ToString()));
         }
          return result;
      }
    }
}