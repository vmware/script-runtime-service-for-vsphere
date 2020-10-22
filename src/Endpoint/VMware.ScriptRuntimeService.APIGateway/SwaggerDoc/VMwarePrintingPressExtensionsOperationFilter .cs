// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc {
   public class VMwarePrintingPressExtensionsOperationFilter : IOperationFilter {
      public void Apply(OpenApiOperation operation, OperationFilterContext context) {
         operation.Extensions.Add("x-vmw-doc-operation", new OpenApiString(operation.OperationId));
      }

      public static void Configure(SwaggerGenOptions options) {
         options.OperationFilter<VMwarePrintingPressExtensionsOperationFilter>();
      }
   }
}
