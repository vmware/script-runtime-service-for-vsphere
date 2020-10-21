// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc {
   public class ReadOnlySchemaFilter : ISchemaFilter {
      public void Apply(OpenApiSchema model, SchemaFilterContext context) {

         // ReadOnly fields
         if (context.MemberInfo != null && context.MemberInfo.MemberType == MemberTypes.Property) {
            var pi = context.MemberInfo.DeclaringType.GetProperty(
               context.MemberInfo.Name,
               BindingFlags.Instance | BindingFlags.Public);
            if (pi.GetCustomAttributes(typeof(ReadOnlyAttribute), false).SingleOrDefault() is ReadOnlyAttribute propAttr &&
                propAttr.IsReadOnly) {
               model.ReadOnly = true;
            }
         }

         // ReadOnly Reference Types
         var type = context.Type;
         if (type.GetCustomAttributes(typeof(ReadOnlyAttribute), false).SingleOrDefault() is ReadOnlyAttribute typeReadOnlyAttr &&
             typeReadOnlyAttr.IsReadOnly) {
            model.ReadOnly = true;
         }
         
      }

      public static void Configure(SwaggerGenOptions options) {
         options.SchemaFilter<ReadOnlySchemaFilter>();
      }
   }
}
