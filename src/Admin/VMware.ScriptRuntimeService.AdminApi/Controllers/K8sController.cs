// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;
using VMware.ScriptRuntimeService.AdminApi.Exceptions;
using VMware.ScriptRuntimeService.AdminEngine.K8sClient;

namespace VMware.ScriptRuntimeService.AdminApi.Controllers {
   public class K8sController : IK8sController {
      private K8sClient _k8sClient;

      private readonly ILoggerFactory _loggerFactory;
      private readonly ILogger _logger;
      private readonly IConfiguration _configuration;
      private K8sSettings _k8sSettings;

      public K8sController(IConfiguration Configuration, ILoggerFactory loggerFactory) {
         _configuration = Configuration;
         _k8sSettings = _configuration.
               GetSection("K8sSettings").
               Get<K8sSettings>();

         _loggerFactory = loggerFactory;
         _logger = loggerFactory.CreateLogger(typeof(K8sController).FullName);
         _logger.LogDebug("K8sServiceController created");

         _k8sClient = new K8sClient(
            loggerFactory,
            _k8sSettings?.ClusterEndpoint,
            _k8sSettings?.AccessToken,
            _k8sSettings?.Namespace);
      }

      public void RestartSrsService() {
         _logger.LogInformation("K8sServiceController restarting the SRS Api gateway.");
         try {
            var srsApiGatewayPod = _k8sClient.GetPod(label: _podTypeToLableMap[LogType.ApiGateway]);
            if (srsApiGatewayPod != null) {
               _k8sClient.DeletePod(srsApiGatewayPod);
            }
         } catch (Exception ex) {
            _logger.LogError($"RestartSrsService failed: {ex}");
         }
      }

      private static readonly Dictionary<LogType, string> _podTypeToLableMap = new Dictionary<LogType, string>() {
         { LogType.ApiGateway, "app=srs-apigateway" },
         { LogType.AdminApi, "app=srs-adminapi" },
         { LogType.Setup, "job-name=srs-setup" },
      };

      public Stream GetPodLogReader(LogType logType) {
         _logger.LogInformation($"Getting {logType} log");
         try {
            foreach (var type in _podTypeToLableMap) {
               if (logType == type.Key) {
                  var pod = _k8sClient.GetPod(label: type.Value);

                  if (pod != null) {
                     _logger.LogDebug($"Getting {logType} log for pod {pod.Metadata.Name}");
                     return _k8sClient.GetPodLogReader(pod);
                  } else {
                     throw new LogSourceNotFoundException(type.Key, type.Value);
                  }
               }
            }

            throw new UnknownLogTypeException(logType);
         } catch (Exception ex) {
            _logger.LogError($"RestartSrsService failed: {ex}");
            throw;
         }
      }

      public IK8sController WithUpdateK8sSettings(K8sSettings k8sSettings) {
         _k8sSettings = k8sSettings;

         _k8sClient = new K8sClient(
            _loggerFactory,
            _k8sSettings?.ClusterEndpoint,
            _k8sSettings?.AccessToken,
            _k8sSettings?.Namespace);

         return this;
      }
   }
}
