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
   public class VMwareVapiVendorExtensionsOperationFilter : IOperationFilter {
      public void Apply(OpenApiOperation operation, OperationFilterContext context) {
         if (operation.Tags[0].Name == "about" &&
             operation.OperationId == "get-product-about-info") {
            operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("about"));
            operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("get"));
         }

         if (operation.Tags[0].Name == "runspaces") {
            if (operation.OperationId == "create-runspace") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("runspaces"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("create"));
            }
            if (operation.OperationId == "delete-runspace") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("runspaces"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("delete"));
            }
            if (operation.OperationId == "get-runspace") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("runspaces"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("get"));
            }
            if (operation.OperationId == "list-runspaces") {

               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("runspaces"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("list"));
            }
         }

         if (operation.Tags[0].Name == "scriptexecutions") {
            if (operation.OperationId == "create-script-execution") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("script_executions"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("create"));
            }
            if (operation.OperationId == "cancel-script-execution") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("script_executions"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("cancel"));
            }
            if (operation.OperationId == "get-script-execution") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("script_executions"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("get"));
            }
            if (operation.OperationId == "list-script-executions") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("script_executions"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("list"));
            }
            if (operation.OperationId == "get-script-execution-stream") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("script_executions"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("get_stream"));
            }
            if (operation.OperationId == "get-script-execution-output") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("script_executions"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("get_output"));
            }
         }

         if (operation.Tags[0].Name == "authentication") {
            if (operation.OperationId == "login") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("authentication"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("login"));
            }

            if (operation.OperationId == "logout") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("authentication"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("logout"));
            }
         }

         if (operation.Tags[0].Name == "argumentscripts") {
            if (operation.OperationId == "list-argument-scripts-templates") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("argument_scripts"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("list_templates"));
            }

            if (operation.OperationId == "gget-argument-scripts-template") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("argument_scripts"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("get_template"));
            }

            if (operation.OperationId == "create-argument-scripts-script") {
               operation.Extensions.Add("x-vmw-vapi-servicename", new OpenApiString("argument_scripts"));
               operation.Extensions.Add("x-vmw-vapi-methodname", new OpenApiString("create_script"));
            }
         }
      }

      public static void Configure(SwaggerGenOptions options) {
         options.OperationFilter<VMwareVapiVendorExtensionsOperationFilter>();
      }
   }
}
