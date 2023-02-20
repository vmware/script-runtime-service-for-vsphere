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
                  AddContentBasedUpdateJsonFileConfiguration(config, "SERVICE_SETTINGS_LOCATION",Path.Combine(AssemblyDirectory, "settings"), "settings.json");
                  AddContentBasedUpdateJsonFileConfiguration(config, "SERVICE_STS_SETTINGS_LOCATION",Path.Combine(AssemblyDirectory, "settings"), "sts-settings.json");
               })
               .ConfigureLogging(logging => {
                  logging.ClearProviders();
                  logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
               })
               .UseNLog();
      }

      private static void AddContentBasedUpdateJsonFileConfiguration(IConfigurationBuilder config, string environmentVariableKey, string defaultRootLocation, string defaultFileName) {
         string environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableKey);

         if (string.IsNullOrEmpty(environmentVariableValue)) {
            Console.WriteLine($"warn: The environment variable '{environmentVariableKey}' is not set. Trying to search for file.");
            if (string.IsNullOrEmpty(defaultRootLocation) || string.IsNullOrEmpty(defaultFileName)) {
               Console.WriteLine($"error: No root path or default file name specified for '{environmentVariableKey}'. Unable to search for configuration file.");
            } else {
               var foundConfigFile = SearchForConfigFile(environmentVariableKey, defaultRootLocation, defaultFileName);
               if (!string.IsNullOrEmpty(foundConfigFile)) {
                  if (File.Exists(foundConfigFile)) {
                     Console.WriteLine($"info: Applying config file for '{environmentVariableKey}' found at '{foundConfigFile}'.");
                     config.AddContentBasedUpdateJsonFileConfiguration(
                        foundConfigFile);
                  } else {
                     Console.WriteLine($"error: Unable to find config file for '{environmentVariableKey}'.");
                  }
               }
            }
         } else {
            if (File.Exists(environmentVariableValue)) {
               Console.WriteLine($"info: Applying config file for '{environmentVariableKey}' from environment variable '{environmentVariableValue}'.");
               config.AddContentBasedUpdateJsonFileConfiguration(
                  environmentVariableValue);
            } else {
               Console.WriteLine($"error: Unable to find config file '{environmentVariableValue}' specified in the environment variable '{environmentVariableKey}'.");
            }
         }
      }

      private static string SearchForConfigFile(string environmentVariableKey, string defaultRootLocation, string defaultFileName) {
         Console.WriteLine($"info: Searching for file '{defaultFileName}' into '{defaultRootLocation}'.");
         var foundFiles = Directory.GetFiles(defaultRootLocation, defaultFileName, SearchOption.AllDirectories);

         if (foundFiles?.Length == 0) {
            Console.WriteLine($"error: No configuration file '{defaultFileName}' found into '{defaultRootLocation}'.");
            return null;
         } else if (foundFiles?.Length > 1) {
            Console.WriteLine($"error: {foundFiles?.Length} configuration files '{defaultFileName}' found into '{defaultRootLocation}'. Unable to choose.");

            foreach(var file in foundFiles) {
               Console.WriteLine($"error:      {file}");
            }
            return null;
         } else {
            Console.WriteLine($"info: Applying config file for '{environmentVariableKey}' {foundFiles[0]}.");
            return foundFiles[0];
         }
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
