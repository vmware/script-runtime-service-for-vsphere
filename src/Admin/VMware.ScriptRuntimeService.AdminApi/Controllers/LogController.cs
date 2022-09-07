// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;
using VMware.ScriptRuntimeService.AdminApi.Exceptions;

namespace VMware.ScriptRuntimeService.AdminApi.Controllers {
   [ApiController]
   [Route("[controller]")]
   public class LogController : ControllerBase {
      private readonly ILoggerFactory _loggerFactory;
      private readonly IK8sController _k8sController;
      private readonly ILogger _logger;

      public LogController(ILoggerFactory loggerFactory, IK8sController k8sController) {
         _loggerFactory = loggerFactory;
         _k8sController = k8sController;
         _logger = _loggerFactory.CreateLogger(typeof(LogController));
      }

      [HttpGet("{type}", Name = "get-podlog")]
      [ProducesResponseType(typeof(VCInfo), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      public ActionResult<IDictionary<LogType, IEnumerable<string>>> Get([FromQuery] LogType type) {
         ActionResult<IDictionary<LogType, IEnumerable<string>>> result;
         _logger.LogDebug($"Getting logs for {type}");
         try {
            result = Ok(_k8sController.GetPodLog(type));
         } catch (PodNotFoundException ex) {
            result = StatusCode(404, new ErrorDetails(ex));
         } catch (Exception ex) {
            result = StatusCode(500, new ErrorDetails(ex));
         }

         return result;
      }
   }
}
