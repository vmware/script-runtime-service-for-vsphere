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
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.Runspace;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;
using VMware.ScriptRuntimeService.Runspace.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers
{
   [Route("api/script-executions")]
   [Produces("application/json")]
   [Consumes("application/json")]
   [ApiController]
   public class ScriptExecutionsController : Controller  {

      private ILoggerFactory _loggerFactory;
      private ILogger _logger;
      private IConfiguration _configuration;

      public ScriptExecutionsController(IConfiguration Configuration, ILoggerFactory loggerFactory) {
         _configuration = Configuration;
         _loggerFactory = loggerFactory;
         _logger = _loggerFactory.CreateLogger(typeof(ScriptExecutionsController));
      }


      // POST api/script-executions
      /// <summary>
      /// Creates a script execution
      /// </summary>
      /// <remarks>
      /// **Script execution** represents asynchronous execution of a script in a specified **runspace**
      /// When created, the **script execution** starts running in the **runspace**. To monitor the script execution progress, poll the resource by id.
      ///       
      /// When the request is accepted **Location** header is returned in the response that leads you to the **script execution** resource.      
      /// </remarks>
      /// <param name="script_execution">Desired script execution resource.</param>
      /// <returns>
      /// Script execution resource to monitor the requested script execution.
      /// </returns>
      [HttpPost(Name= "create-script-execution")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(DataTypes.ScriptExecution), StatusCodes.Status202Accepted)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public async Task<ActionResult<DataTypes.ScriptExecution>> Post([FromBody] DataTypes.ScriptExecution script_execution)
      {
         ActionResult<DataTypes.ScriptExecution> result = null;

         // Get Runspace Endpoint or throw Runspace Not Found
         IRunspaceData runspaceInfo = null;
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            runspaceInfo = 
               RunspaceProviderSingleton.
                  Instance.
                  RunspaceProvider.
                  Get(authzToken.UserName, script_execution.RunspaceId);

         } catch (Exception) {
         }

         if (runspaceInfo == null) {
            result = NotFound(
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(
                     nameof(APIGatewayResources.RunspaceNotFound)),
                  string.Format(
                        APIGatewayResources.RunspaceNotFound, script_execution.RunspaceId)));
         } else if (runspaceInfo.State != RunspaceState.Ready) {
            result = StatusCode(
                  500,
                  new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(
                     nameof(APIGatewayResources.RunspaceNotReady)),
                  string.Format(
                        APIGatewayResources.RunspaceNotReady, 
                        script_execution.RunspaceId, 
                        runspaceInfo.State)));
         } else {
            // Add Script Execution in the ScriptExecutionMediator
            try {
               var authzToken = SessionToken.FromHeaders(Request.Headers);

              var scriptResult =
                  await ScriptExecutionMediatorSingleton.Instance.ScriptExecutionMediator.StartScriptExecution(
                     authzToken.UserName,
                     runspaceInfo,
                     script_execution);

               result = StatusCode(
                  202,
                  new DataTypes.ScriptExecution(scriptResult));
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
                        nameof(APIGatewayResources.ScriptsController_RunspaceFailedToProcessScriptRequest)),
                     APIGatewayResources.ScriptsController_RunspaceFailedToProcessScriptRequest,
                     exc.ToString()));
            }
         }

        
         return result;
      }

      /// <summary>
      /// Cancels a script execution
      /// </summary>
      /// <remarks>
      /// This operation is equivalent to pressing Ctrl+C in the PowerShell console. If the script is cancellable it will be canceled.
      /// The state of the **script execution** will become canceled after this operation. The operation is asynchronous. Cancel request
      /// is sent to the runtime.
      /// </remarks>
      /// <param name="id">The id of the script execution</param>
      /// <returns></returns>
      // POST api/script-executions/{id}/cancel
      [HttpPost("{id}/cancel", Name = "cancel-script-execution")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult Cancel([FromRoute] string id) {
         ActionResult result;
         // Get Script Execution from ScriptExecutionMediator
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            var scriptResult = ScriptExecutionMediatorSingleton.
               Instance.
               ScriptExecutionMediator.
               GetScriptExecution(authzToken.UserName, id);

            if (scriptResult != null) {
               // Cancel Script Execution
               ScriptExecutionMediatorSingleton.
                  Instance.
                  ScriptExecutionMediator.
                  CancelScriptExecution(authzToken.UserName, id);

               result = Ok();
            } else {
               result = NotFound(
                  new ErrorDetails(
                     ApiErrorCodes.GetErrorCode(
                        APIGatewayResources.ScriptsController_ScriptNotFound),
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
                     nameof(APIGatewayResources.ScriptsController_RunspaceFailedToCancelScriptExecution)),
                  APIGatewayResources.ScriptsController_RunspaceFailedToCancelScriptExecution,
                  exc.ToString()));
         }

         return result;
      }

      // GET api/script-executions
      /// <summary>
      /// List all script executions
      /// </summary>
      /// <remarks>
      /// Retrieves all available **script execution** records
      /// </remarks>      
      /// <returns></returns>
      [HttpGet(Name = "list-script-executions")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(DataTypes.ScriptExecution[]), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      // Out of scope for MVP release
      // It is allowed to retrieve partial representation of the **script execution** resources giving desired fields on the **fields** query parameter.
      // Partial objects allow lowering traffic from server to client. You'll probably not need to get script content and parameters when list all script executions.
      // When fields are requested in the query parameter other fields will remain null.
      // <param name="fields"></param>
      //public ActionResult<DataTypes.ScriptExecution[]> List([FromQuery]string[] fields) {
      public ActionResult<DataTypes.ScriptExecution[]> List() {
            ActionResult<DataTypes.ScriptExecution[]> result = null;
         // Get Script Execution from ScriptExecutionMediator
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            var scriptExecutionResults = ScriptExecutionMediatorSingleton.
               Instance.
               ScriptExecutionMediator.
               ListScriptExecutions(authzToken.UserName);

            if (scriptExecutionResults != null) {
               var resultList = new List<DataTypes.ScriptExecution>();

               foreach (var scriptExecutionResult in scriptExecutionResults)
                  resultList.Add(new DataTypes.ScriptExecution(scriptExecutionResult));
               result = resultList.ToArray();
            } else {
               result = Ok(new DataTypes.ScriptExecution[]{});
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
                     nameof(APIGatewayResources.ScriptsController_ScriptStorageService_FailedToRetrieveScripts)),
                  APIGatewayResources.ScriptsController_ScriptStorageService_FailedToRetrieveScripts,
                  exc.ToString()));
         }
         return result;
      }

      /// <summary>
      /// Retrieve a script execution
      /// </summary>
      /// <param name="id">Unique identifier of the runspace</param>
      /// <remarks>
      /// Retrieves the details of a **script execution**. You need only supply the unique script execution identifier that was returned on the **script execution** creation.      
      /// </remarks>
      /// <param name="id"></param>      
      /// <returns></returns>
      // GET api/script-executions/{id}
      [HttpGet("{id}", Name = "get-script-execution")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(DataTypes.ScriptExecution), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      // Out of scope for MVP release
      // It is allowed to retrieve a partial representation of the **script execution** resources giving desired fields on the **fields** query parameter.
      // When fields are requested in the query parameter other fields will remain null.
      // <param name="fields"></param>
      //public ActionResult<DataTypes.ScriptExecution> Get([FromRoute] string id, [FromQuery]string[] fields)
      public ActionResult<DataTypes.ScriptExecution> Get([FromRoute] string id) {
         ActionResult<DataTypes.ScriptExecution> result = null;
         INamedScriptExecution scriptResult = null;
         // Get Script Execution from ScriptExecutionMediator
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            scriptResult = ScriptExecutionMediatorSingleton.
               Instance.
               ScriptExecutionMediator.
               GetScriptExecution(authzToken.UserName, id);

            if (scriptResult != null) {
               result = Ok(new DataTypes.ScriptExecution(scriptResult));
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
         } catch(Exception exc) {
            result = StatusCode(
               500,
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(
                     nameof(APIGatewayResources.ScriptsController_ScriptStorageService_FailedToRetrieveScripts)),
                  APIGatewayResources.ScriptsController_ScriptStorageService_FailedToRetrieveScripts,
                  exc.ToString()));
         }
         return result;
      }
   }
}
