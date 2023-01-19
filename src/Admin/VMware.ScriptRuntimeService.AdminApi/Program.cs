// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace VMware.ScriptRuntimeService.AdminApi
{
   public class Program
   {
      public static void Main(string[] args)
      {
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
            CreateHostBuilder(args).Build().Run();
         } catch (Exception ex) {
            logger.Error(ex, "Stopped program because of exception");
            throw;
         } finally {
            NLog.LogManager.Shutdown();
         }

      }

      public static IWebHostBuilder CreateHostBuilder(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
               .UseKestrel()
               .UseStartup<Startup>()
               .ConfigureLogging(logging => {
                  logging.ClearProviders();
                  logging.SetMinimumLevel(LogLevel.Trace);
               })
               .UseNLog();
   }
}
