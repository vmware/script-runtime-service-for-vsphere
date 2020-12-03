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
using VMware.ScriptRuntimeService.APIGateway.Properties;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc
{
   public class GlobalTagsSchemeFilter : IDocumentFilter
   {
      public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) {
         var aboutTag = new OpenApiTag();
         aboutTag.Name = "about";
         aboutTag.Description = APIGatewayResources.AboutTagDescription;

         var authenticationTag = new OpenApiTag();
         authenticationTag.Name = "authentication";
         authenticationTag.Description = APIGatewayResources.AuthenticationTagDescription;
         
         var argumentScriptsTag = new OpenApiTag();
         argumentScriptsTag.Name = "argument_scripts";
         argumentScriptsTag.Description = APIGatewayResources.ArgumentScriptsTagDescription;

         var runspacesTag = new OpenApiTag();
         runspacesTag.Name = "runspaces";
         runspacesTag.Description = APIGatewayResources.RunspaceTagDescription;

         var scriptExecutionsTag = new OpenApiTag();
         scriptExecutionsTag.Name = "script_executions";
         scriptExecutionsTag.Description = APIGatewayResources.ScriptExecutionsTagDescription;

         swaggerDoc.Tags.Add(aboutTag);
         swaggerDoc.Tags.Add(authenticationTag);         
         swaggerDoc.Tags.Add(argumentScriptsTag);
         swaggerDoc.Tags.Add(runspacesTag);
         swaggerDoc.Tags.Add(scriptExecutionsTag);
      }

      public static void Configure(SwaggerGenOptions options) {
         options.DocumentFilter<GlobalTagsSchemeFilter>();       
      }
   }
}
