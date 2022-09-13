// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VMware.ScriptRuntimeService.AdminEngine.K8sClient;

namespace VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters {
   public class K8sConfigRepository : IConfigWriter, IConfigReader {
      private readonly K8sClient.K8sClient _k8sClient;
      private readonly ILogger _logger;
      public K8sConfigRepository(ILoggerFactory loggerFactory, K8sSettings k8sSettings) {
         _k8sClient = new K8sClient.K8sClient(
            loggerFactory,
            k8sSettings?.ClusterEndpoint,
            k8sSettings?.AccessToken,
            k8sSettings?.Namespace);
         _logger = loggerFactory.CreateLogger(typeof(K8sConfigRepository).FullName);
         _logger.LogDebug("K8s ConfigFileWriter created");
      }

      public void WriteBinaryFile(string name, string filePath) {
         _logger.LogInformation($"Writing binary file k8s opaque secret name: {name}, file: {filePath}");
         if (_k8sClient.CreateBinarySecret(name, filePath) == null) {
            var errorMessage = "_k8sClient.CreateBinarySecret didn't produce result";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
         }
      }

      public void WriteTlsCertificate(string name, string crtFilePath, string keyFilePath) {
         _logger.LogInformation($"Writing k8s Tls secret name: {name}, certificateFilePath: {crtFilePath}");
         if (_k8sClient.CreateTlsSecret(name, crtFilePath, keyFilePath) == null) {
            var errorMessage = "_k8sClient.CreateTlsSecret didn't produce result";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
         }
      }

      public void WriteTrustedCACertificates(IEnumerable<string> encodedCertificates) {
         _logger.LogInformation($"Writing k8s config map with Trusted CA certificates");
         var data = new Dictionary<string, string>();
         foreach (var encodedCert in encodedCertificates) {
            var cert = new X509Certificate2(Encoding.ASCII.GetBytes(encodedCert));
            var fileName = $"{cert.GetCertHashString()}.0";
            data[fileName] = encodedCert;
         }
         _k8sClient.RecreateConfigMap(Constants.TrustedCACertificatesConfigMapName, data);
      }

      public void DeleteTrustedCACertificates() {
         _logger.LogInformation($"Delete k8s config map with Trusted CA certificates");
         DeleteSettings(Constants.TrustedCACertificatesConfigMapName);
      }

      public void WriteSetupSettings(SetupServiceSettings setupServiceSettings) {
         _logger.LogInformation($"Writing k8s config map with setup settings");
         _k8sClient.RecreateConfigMap("setup-settings", new Dictionary<string, string> {
            { "setupsettings.json", JsonConvert.SerializeObject(setupServiceSettings.Memento, Formatting.Indented) }
         });
      }

      public void WriteServiceStsSettings(StsSettings stsSettings) {
         const string configMapName = Constants.StsSettingsConfigMapName;
         const string configMapDataKey = Constants.StsSettingsConfigMapDataKey;

         _logger.LogInformation($"Writing k8s config map with service settings");
         var settingsEditor = new SettingsEditor();
         settingsEditor.AddStsSettings(stsSettings);
         var settingsJson = settingsEditor.GetSettingsJsonContent();
         _k8sClient.RecreateConfigMap(configMapName, new Dictionary<string, string> {
            { configMapDataKey, settingsJson }
         });
      }

      public StsSettings ReadServiceStsSettings() {
         const string configMapName = Constants.StsSettingsConfigMapName;
         const string configMapDataKey = Constants.StsSettingsConfigMapDataKey;

         _logger.LogInformation($"Reading k8s config map with service settings");
         var settingsJson = _k8sClient.GetConfigMapData(configMapName, configMapDataKey);

         var result = JsonConvert.DeserializeObject(settingsJson, typeof(StsSettings));
         return (StsSettings) result;
      }

      public void WriteSettings(string settingsName, object settingsObject) {
         var configMapName = settingsName;
         var configMapDataKey = $"{settingsName}.json";

         _logger.LogInformation($"Writing k8s config map with settingsName settings");
         var settingsEditor = new SettingsEditor();
         settingsEditor.AddSettings(settingsObject);
         var settingsJson = settingsEditor.GetSettingsJsonContent();
         _k8sClient.RecreateConfigMap(configMapName, new Dictionary<string, string> {
            { configMapDataKey, settingsJson }
         });
      }

      public T ReadSettings<T>(string settingsName) {
         var configMapName = settingsName;
         var configMapDataKey = $"{settingsName}.json";

         try {
            _logger.LogInformation($"Reading k8s config map with settingsName settings");
            var rawCondifgMapData = _k8sClient.GetConfigMapData(configMapName, configMapDataKey);
            var result = JsonConvert.DeserializeObject(rawCondifgMapData, typeof(T));
            return (T) result;
         } catch (Exception exc) {
            _logger.LogError($"Reading k8s config map {settingsName} failed: {exc}");
         }

         return default(T);
      }

      public void DeleteSettings(string settingsName) {
         var configMapName = settingsName;

         try {
            _logger.LogInformation($"Deleting k8s config map with settingsName settings");
            _k8sClient.DeleteConfigMap(configMapName);
         } catch (Exception exc) {
            _logger.LogError($"Delete k8s config map {settingsName} failed: {exc.ToString()}");
         }
      }
   }
}
