// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc {
   public class VMwarePrintingPressPathExtensionsDocumentFilter : IDocumentFilter {
      public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) {
         // Project eve doc generation tool requires the following vendor extensions on paths
         // x-vmw-doc-resource: <resource>
         // x-vmw-doc-domain: <domain>
         foreach (var path in swaggerDoc.Paths) {
            if (path.Key == "/api/about") {
               path.Value.Extensions["x-vmw-doc-resource"] = new OpenApiString(typeof(About).Name);
               path.Value.Extensions["x-vmw-doc-domain"] = new OpenApiString("about");
            }

            if (path.Key.StartsWith("/api/auth/login") ||
                path.Key.StartsWith("/api/auth/logout")) {
               path.Value.Extensions["x-vmw-doc-resource"] = new OpenApiString("session");
               path.Value.Extensions["x-vmw-doc-domain"] = new OpenApiString("authentication");
            }

            if (path.Key.StartsWith("/api/runspaces")) {
               path.Value.Extensions["x-vmw-doc-resource"] = new OpenApiString(typeof(DataTypes.Runspace).Name);
               path.Value.Extensions["x-vmw-doc-domain"] = new OpenApiString("runspaces");
            }

            if (path.Key.StartsWith("/api/script-executions")) {
               path.Value.Extensions["x-vmw-doc-resource"] = new OpenApiString(typeof(DataTypes.ScriptExecution).Name);
               if (path.Key.Contains("streams")) {
                  path.Value.Extensions["x-vmw-doc-resource"] = new OpenApiString(typeof(StreamRecord).Name);
               }
               path.Value.Extensions["x-vmw-doc-domain"] = new OpenApiString("scriptexecutions");
            }

            if (path.Key.StartsWith("/api/argument-scripts")) {
               path.Value.Extensions["x-vmw-doc-resource"] = new OpenApiString(typeof(ArgumentScript).Name);
               path.Value.Extensions["x-vmw-doc-domain"] = new OpenApiString("argumentscripts");
            }
         }

      }

      public static void Configure(SwaggerGenOptions options) {
         options.DocumentFilter<VMwarePrintingPressPathExtensionsDocumentFilter>();
      }
   }
}
