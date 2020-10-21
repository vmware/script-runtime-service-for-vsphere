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
using VMware.ScriptRuntimeService.APIGateway.DataTypes;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc {
   public class ScriptExecutionParameterSchemaFilter : ISchemaFilter {
      public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
         if (context.MemberInfo?.DeclaringType == typeof(DataTypes.ScriptExecution)
             && context.MemberInfo?.Name == "ScriptParameters") {

            var baseType = typeof(ScriptParameter);
            var derivedTypes = baseType.Assembly.GetTypes()
               .Where(x => baseType != x && baseType.IsAssignableFrom(x));

            // Update AnyOf
            var anyOfTypeList = new List<OpenApiSchema>();
            foreach (var parameterType in derivedTypes) {
               anyOfTypeList.Add(new OpenApiSchema {
                  Reference = new OpenApiReference {
                     Id = parameterType.Name,
                     Type = ReferenceType.Schema
                  }
               });
            }

            schema.Items.AnyOf = anyOfTypeList;
            schema.Items.Reference = null;
         }
      }

      public static void Configure(SwaggerGenOptions options) {
         options.SchemaFilter<ScriptExecutionParameterSchemaFilter>();
      }
   }
}
