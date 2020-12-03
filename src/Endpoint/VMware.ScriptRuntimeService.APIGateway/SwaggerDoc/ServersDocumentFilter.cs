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
   public class ServersDocumentFilter : IDocumentFilter
   {
      public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) {
         swaggerDoc.Servers.Add(new OpenApiServer() {
            Url = "https://{srs address}"
         });
      }

      public static void Configure(SwaggerGenOptions options) {
         options.DocumentFilter<ServersDocumentFilter>();
      }
   }
}
