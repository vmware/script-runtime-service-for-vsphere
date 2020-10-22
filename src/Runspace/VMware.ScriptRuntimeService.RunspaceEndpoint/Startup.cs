// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddMvc()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)            
            .AddNewtonsoftJson();

         services.AddSwaggerGen(c =>
         {

            c.SwaggerDoc(
                "v1",
                new OpenApiInfo {
                     Title = "Runspace API",
                     Version = "v1",
                }
            );
         });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime) {
         applicationLifetime.ApplicationStopping.Register(OnShutdown);

         // Enable middleware to serve generated Swagger as a JSON endpoint.
         app.UseSwagger();

         // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
         // specifying the Swagger JSON endpoint.
         app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Runspace API V1");
         });

         app.UseStaticFiles();

         app.UseRouting();
         app.UseCors();

         app.UseAuthentication();
         app.UseAuthorization();

         app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
         });
      }
      private void OnShutdown() {
         ScriptExecutionEngineSingleton.Instance.Close();
      }
   }
}
