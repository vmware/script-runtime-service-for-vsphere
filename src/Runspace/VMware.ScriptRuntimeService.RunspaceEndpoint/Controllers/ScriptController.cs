// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.Controllers
{
   [Route("api/script")]
   [ApiController]
   public class ScriptController : ControllerBase
   {
      // GET api/scripts
      [HttpGet]
      [ProducesResponseType(typeof(ScriptExecutionResponse), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
      public ActionResult<ScriptExecutionResponse> GetLastScript() {
         ActionResult<ScriptExecutionResponse> result;
         try {
            result = Ok(ScriptExecutionEngineSingleton.Instance.GetLastScriptStatus());
         } catch (RunspaceEndpointException exc) {
            result = StatusCode(
               exc.HttpErrorCode,
               new ErrorResponse(exc.ErrorCode, exc.Message, exc.ToString()));
         }
         return result;
      }

      // GET api/scripts/<script-id>
      [HttpGet("{id}")]
      [ProducesResponseType(typeof(ScriptExecutionResponse), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
      public ActionResult<ScriptExecutionResponse> Get(string id) {
         ActionResult<ScriptExecutionResponse> result;
         try {
            result = Ok(ScriptExecutionEngineSingleton.Instance.GetScriptStatus(id));
         } catch (RunspaceEndpointException exc) {
            result = StatusCode(
               exc.HttpErrorCode,
               new ErrorResponse(exc.ErrorCode, exc.Message, exc.ToString()));
         }
         return result;
      }

      // POST api/scripts
      [HttpPost]
      [ProducesResponseType(typeof(ScriptExecutionResponse), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public ActionResult<ScriptExecutionResponse> Post([FromBody] ScriptExecutionRequest value) {
         ActionResult<ScriptExecutionResponse> result;
         try {
            IScriptParameter[] parameters = null;

            // Convert Parameters from request to IScriptParameters
            if (value.Parameters?.Length > 0) {
               List<IScriptParameter> parametersList = new List<IScriptParameter>();
               foreach (var scriptParameter in value.Parameters) {
                  if (scriptParameter != null) {
                     parametersList.Add(
                        new Runspace.PSScriptExecutionEngine.ScriptParameter {
                           Name = scriptParameter.Name,
                           Script = scriptParameter.Script,
                           Value =
                              ScriptExecutionEngineSingleton.
                                 Instance.
                                 ConvertToEngineNativeObject(scriptParameter.Value)
                        });
                  }
               }

               if (parametersList.Count > 0) {
                  parameters = parametersList.ToArray();
               }
            }

            result = StatusCode(201, 
               ScriptExecutionEngineSingleton.
                  Instance.
                  StartScriptExecution(
                     value.Script,
                     parameters,
                     OutputObjectsFormatEnumConverter.ToRunspaceTypes(value.OutputObjectsFormat),
                     value.Name));
         } catch (RunspaceEndpointException exc) {
            result = StatusCode(
               exc.HttpErrorCode,
               new ErrorResponse(exc.ErrorCode, exc.Message, exc.ToString()));
         }
         return result;
      }

      // DELETE api/scripts/<script-id>
      [HttpDelete("{id}")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
      public void Delete(string id) {
         try {
            ScriptExecutionEngineSingleton.Instance.CancelScriptExecution(id);
            Ok();
         } catch (RunspaceEndpointException exc) {
            StatusCode(
               exc.HttpErrorCode,
               new ErrorResponse(exc.ErrorCode, exc.Message, exc.ToString()));
         }
      }
   }
}
