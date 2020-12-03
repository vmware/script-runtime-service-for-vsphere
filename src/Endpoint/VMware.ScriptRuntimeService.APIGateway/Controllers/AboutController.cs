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
using VMware.ScriptRuntimeService.APIGateway.Runspace;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers {
   [Route("api/about")]
   [Produces("application/json")]
   [ApiController]
   public class AboutController : ControllerBase {
      /// <summary>
      /// Retrieves about information for the product
      /// </summary>     
      /// <returns>Instance of <see cref="About"/></returns>
      [HttpGet(Name = "get-product-about-info")]
      [ProducesResponseType(typeof(About), StatusCodes.Status200OK)]
      public ActionResult<About> Get() {
         return Ok(new About());
      }
   }
}