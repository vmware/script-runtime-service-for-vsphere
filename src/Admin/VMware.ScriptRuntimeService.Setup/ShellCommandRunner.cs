// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace VMware.ScriptRuntimeService.Setup {
   public class ShellCommandRunner {
      string _shellFileName;
      string _arguments;
      ILogger _logger;
      const int MAX_EXECUTION_TIME = 60 * 60 * 1000; // 1 minute
      int _maxExecutionTime;

      public ShellCommandRunner(
         ILoggerFactory loggerFactory,
         string shell,
         string arguments) : 
         this(loggerFactory, shell, arguments, MAX_EXECUTION_TIME) { }

         public ShellCommandRunner(
         ILoggerFactory loggerFactory, 
         string shell, 
         string arguments,
         int maxExecutionTimeMs) {

         _logger = loggerFactory.CreateLogger(typeof(ShellCommandRunner).FullName);
         _shellFileName = shell;
         _arguments = arguments;
         _maxExecutionTime = maxExecutionTimeMs;

         _logger.LogDebug("ShellCommandRunner constructler");
         _logger.LogDebug($"  Shell: {_shellFileName}");
         _logger.LogDebug($"  Arguments: {_arguments}");         
      }

      public ShellCommandResult Run() {
         ShellCommandResult result = null;

         try {
            using (Process process = new Process()) {

               process.StartInfo = new ProcessStartInfo {
                  WindowStyle = ProcessWindowStyle.Hidden,
                  FileName = _shellFileName,
                  Arguments = _arguments,
                  RedirectStandardOutput = true,
                  RedirectStandardError = true,
                  UseShellExecute = false
               };

               StringBuilder output = new StringBuilder();
               StringBuilder error = new StringBuilder();

               using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
               using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false)) {
                  process.OutputDataReceived += (sender, e) => {
                     if (e.Data == null) {
                        outputWaitHandle.Set();
                     } else {
                        output.AppendLine(e.Data);
                     }
                  };
                  process.ErrorDataReceived += (sender, e) =>
                  {
                     if (e.Data == null) {
                        errorWaitHandle.Set();
                     } else {
                        error.AppendLine(e.Data);
                     }
                  };

                  process.Start();

                  process.BeginOutputReadLine();
                  process.BeginErrorReadLine();

                  if (process.WaitForExit(_maxExecutionTime) &&
                      outputWaitHandle.WaitOne(_maxExecutionTime) &&
                      errorWaitHandle.WaitOne(_maxExecutionTime)) {                     
                     result = new ShellCommandResult {
                        ExitCode = process.ExitCode,
                        OutputStream = output.ToString(),
                        ErrorStream = error.ToString()
                     };
                  } else {                     
                     throw new Exception($"ShellCommandRunner command didn't finished within maximum allowed execution time: {MAX_EXECUTION_TIME}");
                  }                  
               }
            }
         } catch(Exception exc) {
            _logger.LogError(exc, "ShellCommandRunner produced an exception");
         }

         return result;
      }
   }
}
