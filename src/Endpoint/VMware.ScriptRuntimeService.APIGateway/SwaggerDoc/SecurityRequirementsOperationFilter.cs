// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VMware.ScriptRuntimeService.APIGateway.SwaggerDoc {
   public class SecurityRequirementsOperationFilter : IOperationFilter {
      public void Apply(OpenApiOperation operation, OperationFilterContext context) {
         // Policy names map to scopes
         var requiredScopes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .Distinct();

         if (requiredScopes.Any()) {
            if (operation.OperationId != "get.about") {
               if (operation.OperationId == "login") {
                  var basic = new OpenApiSecurityScheme {
                     Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basicAuth" }
                  };
                  var sign = new OpenApiSecurityScheme {
                     Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "signAuth" }
                  };

                  operation.Security = new List<OpenApiSecurityRequirement> {
                     new OpenApiSecurityRequirement {
                        [ basic ] = Array.Empty<string>()
                     },
                     new OpenApiSecurityRequirement {
                        [ sign ] = Array.Empty<string>()
                     }
                  };
               } else {
                  var apiKey = new OpenApiSecurityScheme {
                     Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "apiKeyAuth" }
                  };

                  operation.Security = new List<OpenApiSecurityRequirement> {
                     new OpenApiSecurityRequirement {
                        [ apiKey ] = Array.Empty<string>()
                     }
                  };
               }
            }
         }
      }

      public static void Configure(SwaggerGenOptions options) {
         options.AddSecurityDefinition("apiKeyAuth", new OpenApiSecurityScheme {
            Name = "X-SRS-API-KEY",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,

         });

         options.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Scheme = "basic"

         });

         options.AddSecurityDefinition("signAuth", new OpenApiSecurityScheme {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Scheme = "SIGN"

         });

         options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
               new OpenApiSecurityScheme {
                  Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "apiKeyAuth" }
               },
               Array.Empty<string>()
            }
         });

         options.AddSecurityRequirement(new OpenApiSecurityRequirement {
               {
                  new OpenApiSecurityScheme {
                     Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basicAuth" }
                  },
                  Array.Empty<string>()
               }
            });
         options.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
               new OpenApiSecurityScheme {
                  Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "signAuth" }
               },
               Array.Empty<string>()
            }
         });

         options.OperationFilter<SecurityRequirementsOperationFilter>();
      }
   }
}
