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
using VMware.ScriptRuntimeService.Setup.K8sClient;

namespace VMware.ScriptRuntimeService.Setup.ConfigFileWriters
{
   public class K8sConfigWriter : IConfigWriter
   {
      K8sClient.K8sClient _k8sClient;
      ILogger _logger;
      public K8sConfigWriter(ILoggerFactory loggerFactory, K8sSettings k8sSettings) {
         _k8sClient = new K8sClient.K8sClient(
            loggerFactory, 
            k8sSettings?.ClusterEndpoint,
            k8sSettings?.AccessToken,
            k8sSettings?.Namespace);
         _logger = loggerFactory.CreateLogger(typeof(K8sConfigWriter).FullName);
      }

      public void WriteBinaryFile(string name, string filePath) {
         if (_k8sClient.CreateBinarySecret(name, filePath) == null) {
            var errorMessage = "_k8sClient.CreateBinarySecret didn't produce result";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
         }
      }

      public void WriteTlsCertificate(string name, string crtFilePath, string keyFilePath) {
         if (_k8sClient.CreateTlsSecret(name, crtFilePath, keyFilePath) == null) {
            var errorMessage = "_k8sClient.CreateTlsSecret didn't produce result";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
         }         
      }

      public void WriteTrustedCACertificates(IEnumerable<string> encodedCertificates) {         
         var data = new Dictionary<string, string>();
         foreach (var encodedCert in encodedCertificates) {
            var cert = new X509Certificate2(Encoding.ASCII.GetBytes(encodedCert));
            var fileName = $"{cert.GetCertHashString()}.0";
            data[fileName] = encodedCert;
         }
         _k8sClient.RecreateConfigMap(Constants.TrustedCACertificatesConfigMapName, data);
      }

      public void WriteSetupSettings(SetupServiceSettings setupServiceSettings) {
         _k8sClient.RecreateConfigMap("setup-settings", new Dictionary<string, string> {
            { "setupsettings.json", JsonConvert.SerializeObject(setupServiceSettings.Memento, Formatting.Indented) }      
         });
      }

      public void WriteServiceStsSettings(StsSettings stsSettings) {
         const string configMapName = Constants.StsSettingsConfigMapName;
         const string configMapDataKey = Constants.StsSettingsConfigMapDataKey;

         var settingsEditor = new SettingsEditor();
         settingsEditor.AddStsSettings(stsSettings);
         var settingsJson = settingsEditor.GetSettingsJsonContent();
         _k8sClient.RecreateConfigMap(configMapName, new Dictionary<string, string> {
            { configMapDataKey, settingsJson }
         });
      }
   }
}
