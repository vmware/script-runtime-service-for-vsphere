// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
         try {
            var srsApiGatewayPod = _k8sClient.GetPod(label: "app=srs-apigateway");
            if (srsApiGatewayPod != null) {
               _k8sClient.DeletePod(srsApiGatewayPod);
            }
         } catch (Exception ex) {
            _logger.LogError($"RestartSrsService failed: {ex}");
         }
      }

      public IEnumerable<string> GetApiGatewayLog() {
         try {
            var srsApiGatewayPod = _k8sClient.GetPod(label: "app=srs-apigateway");
            if (srsApiGatewayPod != null) {
               return _k8sClient.ReadPodLog(srsApiGatewayPod);
            } else {
               throw new Exception("Pod with label srs-apigateway not found.");
            }
         } catch (Exception ex) {
            _logger.LogError($"RestartSrsService failed: {ex}");
            throw;
         }
      }

      private static readonly Dictionary<PodType, string> _podTypeToLableMap = new Dictionary<PodType, string>() {
         { PodType.ApiGateway, "app=srs-apigateway" },
         { PodType.AdminApi, "app=srs-adminapi" },
         { PodType.Setup, "job-name=srs-setup" },
      };

      public IDictionary<PodType, IEnumerable<string>> GetPodLog(PodType podType) {
         try {
            var result = new Dictionary<PodType, IEnumerable<string>>();
            foreach (var type in _podTypeToLableMap) {
               if ((podType & type.Key) == type.Key) {
                  var srsApiGatewayPod = _k8sClient.GetPod(label: type.Value);
                  if (srsApiGatewayPod != null) {
                     result.Add(type.Key, _k8sClient.ReadPodLog(srsApiGatewayPod));
                  } else {
                     throw new PodNotFoundException(type.Key, type.Value);
                  }
               }
            }

            return result;
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
