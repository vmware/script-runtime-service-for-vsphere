// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.SystemScripts;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers {
   [Route("api/argument-scripts")]
   [ApiController]
   [Produces("application/json")]
   [Consumes("application/json")]
   public class ArgumentScriptsController : ControllerBase
   {
      /// <summary>
      /// List available argument script templates
      /// </summary>
      /// <remarks>
      /// Argument script templates are scripts with placeholders. When values replace the placeholders, the script can run in a given script runtime.
      /// Argument script templates help to convert simple type values to objects of types that can only be produced in a given script runtime. Those objects can be used as arguments to scripts' parameters.
      /// 
      /// This operation retrieves the available argument script templates.
      /// </remarks>
      /// <returns>List of names of the available PowerCLI argument transformation scripts</returns>
      [HttpGet("templates", Name = "list-argument-scripts-templates")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(ArgumentScriptTemplate[]), StatusCodes.Status200OK)]
      public ActionResult<ArgumentScriptTemplate[]> List() {
         return Ok(new List<ArgumentScriptTemplate>(PCLIScriptsReader.ListArgumentTransformationScriptNames()).ToArray());
      }

      /// <summary>
      /// Retrieves argument script template by given unique template identifier
      /// </summary>
      /// <remarks>
      /// This operation returns argument script template for the specified template id.
      /// </remarks>
      /// <param name="id">The Id of the argument script template</param>
      /// <returns></returns>
      [HttpGet("templates/{id}", Name = "get-argument-scripts-template")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(ArgumentScriptTemplate), StatusCodes.Status200OK)]
      public ActionResult<ArgumentScriptTemplate> Get([FromRoute]string id) {
         var result = PCLIScriptsReader.GetArgumentTransformationScript(id);
         return result == null
            ? NotFound(
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(nameof(APIGatewayResources.ArgumentScriptsController_ArgumentTransformationScriptNotFound)),
                  string.Format(APIGatewayResources.ArgumentScriptsController_ArgumentTransformationScriptNotFound, id)))
            : (ActionResult<ArgumentScriptTemplate>)Ok(result);
      }

      /// <summary>
      /// Creates scripts for a given script template id and placeholder values
      /// </summary>
      /// <remarks>
      /// Replaces the placeholders in a given argument transformation script template with given values on the placeholder_value_list field
      /// The resulting script can be provided to a **script execution** parameter that expects a specific script runtime type
      /// 
      /// **Example**
      /// 
      /// If the template argument transformation script is
      /// 
      /// Get-VM -Id &lt;vm-id&gt; -Server &lt;server&gt;
      /// 
      /// The result of this operation with given Id 'vm-1' and Server 'server-1' is
      /// 
      /// Get-VM -Id 'vm-1' -Server 'server-1'
      /// </remarks>
      /// <param name="argumentScript">The argument script create request</param>
      /// <returns></returns>
      [HttpPost("script", Name = "create-argument-scripts-script")]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      [ProducesResponseType(typeof(ArgumentScript), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      public ActionResult<ArgumentScript> Create([FromBody]ArgumentScript argumentScript) {
         var template = PCLIScriptsReader.GetArgumentTransformationScript(argumentScript.TemplateId);
         if (template == null) {
            return NotFound(
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(nameof(APIGatewayResources.ArgumentScriptsController_ArgumentTransformationScriptNotFound)),
                  string.Format(APIGatewayResources.ArgumentScriptsController_ArgumentTransformationScriptNotFound, argumentScript.TemplateId)));
         }
         
         return Ok(
            new PCLIScriptsGenerator(
               template.ScriptTemplate,
               argumentScript.PlaceholderValueList).Generate());
      }
   }
}
