// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc {
   public class ScriptExecutionParameterDocumentFilter : IDocumentFilter {
      public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) {
         // Retrieve all derive types of ScriptParameter
         var baseType = typeof(ScriptParameter);
         var derivedTypes = baseType.Assembly.GetTypes()
            .Where(x => baseType != x && baseType.IsAssignableFrom(x));

         // Generate schema for all derived types 
         foreach (var item in derivedTypes) {
            context.SchemaRepository.GetOrAdd(item, item.Name, () => {
               var resultSchema = context.SchemaGenerator.GenerateSchema(
                  item,
                  context.SchemaRepository);
               resultSchema.Properties = new Dictionary<string, OpenApiSchema>();

               foreach (var mi in item.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                  if (mi.GetCustomAttribute(typeof(DataMemberAttribute)) is DataMemberAttribute dataMemberAttribute) {
                     var propertySchema = context.SchemaGenerator.GenerateSchema(mi.PropertyType, context.SchemaRepository, mi);
                     if (mi.GetCustomAttribute(typeof(RequiredAttribute)) is RequiredAttribute) {
                        if (resultSchema.Required == null) {
                           resultSchema.Required = new HashSet<string>();
                        }

                        resultSchema.Required.Add(dataMemberAttribute.Name);
                     }

                     resultSchema.Properties[dataMemberAttribute.Name] = propertySchema;
                     
                  }
               }

               return resultSchema;
            });
         }

         // Remove base type from schema
         context.SchemaRepository.Schemas.Remove(baseType.Name);
      }

      public static void Configure(SwaggerGenOptions options) {
         options.DocumentFilter<ScriptExecutionParameterDocumentFilter>();
      }
   }
}
