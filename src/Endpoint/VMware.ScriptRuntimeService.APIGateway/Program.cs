// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Web;
using Swashbuckle.AspNetCore.Swagger;

namespace VMware.ScriptRuntimeService.APIGateway
{
   public class Program
   {
      public static void Main(string[] args) {
         var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
         try {
            // Copy attached trsuted CA certificates to system trusted certificates if avaiable
            // Works for photon OS only assuming it will run a photon OS container
            var expectedAttachedTrustedCADir = "/app/service/trusted-ca-certs";
            var expectedOSTrustedCADir = "/etc/ssl/certs";
            if (Directory.Exists(expectedAttachedTrustedCADir) &&
               Directory.Exists(expectedOSTrustedCADir)) {
               foreach (var f in Directory.GetFiles(expectedAttachedTrustedCADir)) {
                  var fi = new FileInfo(f);
                  File.Copy(f, Path.Combine(expectedOSTrustedCADir, fi.Name));
               }
            }
            CreateWebHostBuilder(args).Build().Run();
         } catch (Exception ex) {
            logger.Error(ex, "Stopped program because of exception");
            throw;
         } finally {
            NLog.LogManager.Shutdown();
         }
      }

      public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
         return WebHost.CreateDefaultBuilder(args)
               .UseKestrel()
               .UseStartup<Startup>()
               .ConfigureAppConfiguration((hostingContext, config) =>
               {
                  var settingsPath = Path.Combine(AssemblyDirectory, "settings", "settings.json");
                  if (File.Exists(settingsPath)) {
                     config.AddContentBasedUpdateJsonFileConfiguration(
                        settingsPath);
                  }

                  var stsSettingsPath = Path.Combine(AssemblyDirectory, "settings", "sts-settings.json");
                  if (File.Exists(stsSettingsPath)) {
                     config.AddContentBasedUpdateJsonFileConfiguration(
                        stsSettingsPath);
                  }                  
               })
               .ConfigureLogging(logging => {
                  logging.ClearProviders();
                  logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
               })
               .UseNLog();
      }

      public static string AssemblyDirectory {
         get {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
         }
      }
   }
}
