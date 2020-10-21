// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc {
   public class TagsOperationFilter : IOperationFilter {
      public void Apply(OpenApiOperation operation, OperationFilterContext context) {
         if (operation.Tags[0].Name == "ScriptExecutionOutput" ||
             operation.Tags[0].Name == "ScriptExecutionStreams") {
            operation.Tags[0].Name = "scriptExecutions";
         }

         if (operation.Tags[0].Name == "ArgumentTransformationScripts") {
            operation.Tags[0].Name = "argTransformations";
         }

         operation.Tags[0].Name = operation.Tags[0].Name.ToLower();
      }

      public static void Configure(SwaggerGenOptions options) {
         options.OperationFilter<TagsOperationFilter>();
      }
   }
}
