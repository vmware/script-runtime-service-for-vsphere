// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using VMware.Http.Sso.Authentication;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.Authentication.Basic;
using VMware.ScriptRuntimeService.APIGateway.Authentication.Sign;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Sts;
using VMware.ScriptRuntimeService.APIGateway.SwaggerDoc;
using VMware.ScriptRuntimeService.APIGateway.Runspace;
using Microsoft.Extensions.Primitives;

namespace VMware.ScriptRuntimeService.APIGateway
{
   public class Startup {
      private ILogger _logger;
      public Startup(IConfiguration configuration) {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }


      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services) {
         services
            .AddMvc()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson(options => {
               options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
               options.SerializerSettings.MaxDepth = int.MaxValue;
            });


         services.AddAuthentication(Constants.SignAuthenticationScheme).
            AddScheme<StsSettings, SignAuthenticationHandler>(Constants.SignAuthenticationScheme,
               options => Configuration.Bind("StsSettings", options));

         services.AddAuthentication(BasicAuthenticationHandler.AuthenticationScheme).
            AddScheme<StsSettings, BasicAuthenticationHandler>(BasicAuthenticationHandler.AuthenticationScheme,
               options => Configuration.Bind("StsSettings", options));

         services.AddAuthentication(SrsAuthenticationScheme.SessionAuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(SrsAuthenticationScheme.SessionAuthenticationScheme, null);

         var packageNameExtension = new OpenApiObject();
         packageNameExtension.Add("package-name", new OpenApiString("com.vmware.srs"));
         services.AddSwaggerGen(
            c => {
               c.SwaggerDoc(
                  "srs",
                  new OpenApiInfo {
                     Description = APIGatewayResources.ProductApiDescription,
                     Title = APIGatewayResources.ProductName,
                     Version = APIGatewayResources.ProductVersion,
                     Contact = new OpenApiContact() {
                        Name = "Script Runtime Service for vSphere",                        
                        Url = new Uri(@"https://github.com/vmware/script-runtime-service-for-vsphere"),
                     },
                     Extensions = {
                        { "x-vmw-vapi-codegenconfig", packageNameExtension }
                     }
               });

               var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
               var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
               c.IncludeXmlComments(xmlPath);
               GlobalTagsSchemeFilter.Configure(c);
               TagsOperationFilter.Configure(c);
               VMwareVapiVendorExtensionsOperationFilter.Configure(c);
               SecurityRequirementsOperationFilter.Configure(c);
               ScriptExecutionParameterDocumentFilter.Configure(c);
               ScriptExecutionParameterSchemaFilter.Configure(c);
               //ServersDocumentFilter.Configure(c);
               //VMwarePrintingPressExtensionsOperationFilter.Configure(c);
               //VMwarePrintingPressPathExtensionsDocumentFilter.Configure(c);
               ReadOnlySchemaFilter.Configure(c);
            });
         services.AddSwaggerGenNewtonsoftSupport();
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory) {
         // Get logger for Startup class
         _logger = loggerFactory.CreateLogger(typeof(Startup));

         // Override Newtonsoft.Json MaxDepth de/serialization because
         // as of version 13.0.1 the default MaxDepth is 64
         // this keeps pre 13.0.1 behaviour
         Func<Newtonsoft.Json.JsonSerializerSettings> defaultSettingsFunc =
            Newtonsoft.Json.JsonConvert.DefaultSettings;
         if (null == Newtonsoft.Json.JsonConvert.DefaultSettings) {
            defaultSettingsFunc =
               () => new Newtonsoft.Json.JsonSerializerSettings() {
                  MaxDepth = int.MaxValue,
                  NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
               };
         } else {
            defaultSettingsFunc =
               () => {
                  var settings = defaultSettingsFunc();
                  settings.MaxDepth = int.MaxValue;

                  return settings;
               };
         }

         Newtonsoft.Json.JsonConvert.DefaultSettings = defaultSettingsFunc;

         // Get from app settings
         RunspaceProviderSingleton.Instance.CreateRunspaceProvider(
            loggerFactory,
            Configuration.
               GetSection("RunspaceProviderSettings").
               Get<RunspaceProviderSettings>());

         ScriptExecutionMediatorSingleton.Instance.CreateScriptExecutionStorage(
            loggerFactory,
            Configuration.
               GetSection("ScriptExecutionStorageSettings").
               Get<ScriptExecutionStorageSettings>());

         // Enable middleware to serve generated Swagger as a JSON endpoint.
         app.UseSwagger();

         // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
         // specifying the Swagger JSON endpoint.
         app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/srs/swagger.json", "Script Runtime Service for vSphere API");
         });

         ChangeToken.OnChange(
            () => Configuration.GetReloadToken(),
            () => ServiceConfigurationChanged());

         app.UseStaticFiles();

         app.UseRouting();
         app.UseCors();

         app.UseAuthentication();
         app.UseAuthorization();

         app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
         });

      }

      private void ServiceConfigurationChanged() {
         try {
            // Update runspace provider configuration. Users can update
            // service settings runspace provider configuration is updated
            // before creating new runspaces.
            var providerSettings = new RunspaceProviderSettings();
            Configuration.Bind("RunspaceProviderSettings", providerSettings);
            if (!providerSettings.IsDefault()) {
               _logger.LogDebug("RunspaceProviderSettings update:");
               _logger.LogDebug($"    K8sRunspaceImageName: {providerSettings.K8sRunspaceImageName}");
               _logger.LogDebug($"    MaxNumberOfRunspaces: {providerSettings.MaxNumberOfRunspaces}");
               _logger.LogDebug($"    MaxRunspaceActiveTimeMinutes: {providerSettings.MaxRunspaceActiveTimeMinutes}");
               _logger.LogDebug($"    MaxRunspaceIdleTimeMinutes: {providerSettings.MaxRunspaceIdleTimeMinutes}");
               RunspaceProviderSingleton.Instance.RunspaceProvider.UpdateConfiguration(providerSettings);
            }
         } catch (Exception configUpdateException) {
            _logger.LogError(configUpdateException, "An error occured on updating runspace provider settings");
         }

         try {
            // Update scripts storage configuration. Users can update
            // service settings for persisting scripts. We update the
            // scripts store settings on run script request.
            var scriptStorageSettings = new ScriptExecutionStorageSettings();
            Configuration.Bind("ScriptExecutionStorageSettings", scriptStorageSettings);
            if (!scriptStorageSettings.IsDefault()) {
               ScriptExecutionMediatorSingleton.Instance.ScriptExecutionMediator.UpdateConfiguration(scriptStorageSettings);
            }
         } catch (Exception configUpdateException) {
            _logger.LogError(configUpdateException, "An error occured on updating script storage settings");
         }
      }
   }
}
