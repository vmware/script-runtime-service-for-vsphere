// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

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
         CreateHostBuilder(args).Build().Run();
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
