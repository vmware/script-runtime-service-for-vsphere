// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters;
using VMware.ScriptRuntimeService.AdminEngine.K8sClient;

namespace VMware.ScriptRuntimeService.AdminEngine.SetupFlows {
   /// <summary>
   /// Base class to encapsulate complete specific setup flow implementation
   /// </summary>
   public abstract class BaseSetupFlow {
      protected readonly ILoggerFactory _loggerFactory;
      protected ILogger _logger;
      protected abstract SetupFlowType Type { get; }

      internal BaseSetupFlow(ILoggerFactory loggerFactory) {
         _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
      }

      public int Run(UserInput userInput) {
         try {
            userInput.EnsureIsValid(Type);

            K8sSettings k8sSettings = null;
            if (userInput.K8sSettings != null && File.Exists(userInput.K8sSettings)) {
               k8sSettings = JsonConvert.DeserializeObject<K8sSettings>(File.ReadAllText(userInput.K8sSettings));
            }

            var configProxy = new K8sConfigRepository(_loggerFactory, k8sSettings);
            var vcRegistrator = new VCRegistration.VCRegistrator(_loggerFactory, configProxy, configProxy);
            RunInternal(vcRegistrator, userInput);
         } catch (InvalidUserInputException exc) {
            _logger.LogError(exc, exc.Message);
            return 1;
         } catch (Exception exc) {
            _logger.LogError(exc, exc.Message);
            return 2;
         }

         _logger.LogInformation("Success");
         return 0;
      }

      protected abstract void RunInternal(VCRegistration.VCRegistrator vcRegistrator, UserInput userInput);
   }
}
