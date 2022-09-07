// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using k8s;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using k8s.Models;

namespace VMware.ScriptRuntimeService.AdminEngine.K8sClient {
   public class K8sClient {
      private readonly ILogger _logger;
      private readonly string _namespace;
      private readonly IKubernetes _k8sClient;

      public K8sClient(
         ILoggerFactory loggerFactory,
         string k8sClusterEndpoint,
         string accessToken,
         string @namespace) {

         _logger = loggerFactory.CreateLogger(typeof(K8sClient).FullName);

         var config = new KubernetesClientConfiguration() {
            Host = k8sClusterEndpoint ?? @"https://kubernetes.default.svc",
            AccessToken = accessToken ?? File.ReadAllText("/run/secrets/kubernetes.io/serviceaccount/token"),
            SkipTlsVerify = true
         };

         _logger.LogDebug("KubernetesClientConfiguration");
         _logger.LogDebug($"  Host: {config.Host}");
         _logger.LogDebug($"  AccessToken: {config.AccessToken}");
         _logger.LogDebug($"  SkipTlsVerify: {config.SkipTlsVerify}");

         _k8sClient = new Kubernetes(config);

         _namespace = @namespace;
         if (string.IsNullOrEmpty(_namespace)) {
            _namespace = File.ReadAllText("/run/secrets/kubernetes.io/serviceaccount/namespace");
         }
      }

      public k8s.Models.V1ConfigMap CreateConfigMap(string name, Dictionary<string, string> data) {
         var result = _k8sClient.CoreV1.CreateNamespacedConfigMap(
            new k8s.Models.V1ConfigMap(
                  data: data,
                  metadata: new k8s.Models.V1ObjectMeta(name: name)
               ),
               _namespace
            );
         return result;
      }

      public k8s.Models.V1ConfigMap GetConfigMap(string name) {
         var allConfigMaps = _k8sClient.CoreV1.ListNamespacedConfigMap(_namespace);
         return allConfigMaps?.Items?.Where<k8s.Models.V1ConfigMap>(c => c.Metadata.Name == name).FirstOrDefault();
      }

      /// <summary>
      /// Retrieves data for specified key from specified config map.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="key"></param>
      /// <returns>Value represeting the data for specified parameters. In case configmap or key doesn't exists returns null.</returns>
      public string GetConfigMapData(string name, string key) {
         string result = null;
         var allConfigMaps = _k8sClient.CoreV1.ListNamespacedConfigMap(_namespace);
         var cmap = allConfigMaps?.Items?.Where<k8s.Models.V1ConfigMap>(c => c.Metadata.Name == name).FirstOrDefault();
         if (cmap != null && !string.IsNullOrEmpty(key)) {
            cmap.Data.TryGetValue(key, out result);
         }
         return result;
      }

      /// <summary>
      /// If config map with specified name exists it is deleted and new one is created
      /// If config map with specified name doesn't exists creates new config map.
      /// <see cref="GetConfigMap(string)"/> method is used to check whether config map with specified name exists.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="data"></param>
      /// <returns>The newly created <see cref="k8s.Models.V1ConfigMap"/></returns>
      public k8s.Models.V1ConfigMap RecreateConfigMap(string name, Dictionary<string, string> data) {
         // Delete current if exists
         if (GetConfigMap(name) != null) {
            DeleteConfigMap(name);
         }

         // Create config map
         return CreateConfigMap(name, data);
      }


      public string DeleteConfigMap(string name) {
         var status = _k8sClient.CoreV1.DeleteNamespacedConfigMap(
               name,
               _namespace
            );
         return status.Status;
      }

      public k8s.Models.V1ConfigMapList ListConfigMap() {
         return _k8sClient.CoreV1.ListNamespacedConfigMap(_namespace);
      }

      public k8s.Models.V1Secret CreateTlsSecret(string name, string crtFilePath, string keyFilePath) {
         return CreateSecret(name, GetTlsSecretData(crtFilePath, keyFilePath), "kubernetes.io/tls");
      }

      public k8s.Models.V1Secret CreateBinarySecret(string name, string filePath) {
         return CreateSecret(name, GetBinarySecretData(filePath), "Opaque");
      }

      private Dictionary<string, byte[]> GetBinarySecretData(string file) {
         var secretData = new Dictionary<string, byte[]>();

         if (!string.IsNullOrEmpty(file) && File.Exists(file)) {
            secretData.Add(
               new FileInfo(file).Name,
               File.ReadAllBytes(file));
         }
         return secretData;
      }

      private Dictionary<string, string> GetTlsSecretData(string crtFile, string keyFile) {
         var secretData = new Dictionary<string, string>();

         if (!string.IsNullOrEmpty(crtFile) && File.Exists(crtFile)) {
            secretData.Add(
               "tls.crt",
               File.ReadAllText(crtFile));
         }

         if (!string.IsNullOrEmpty(keyFile) && File.Exists(keyFile)) {
            secretData.Add(
               "tls.key",
               File.ReadAllText(keyFile));
         }

         return secretData;
      }

      private k8s.Models.V1Secret CreateSecret(string name, IDictionary<string, string> secretData, string secretType) {
         k8s.Models.V1Secret result = null;

         if (secretData.Count > 0) {
            result = _k8sClient.CoreV1.CreateNamespacedSecret(
            new k8s.Models.V1Secret(
                  stringData: secretData,
                  metadata: new k8s.Models.V1ObjectMeta(name: name),
                  type: secretType
               ),
               _namespace
            );
         }
         return result;
      }

      private k8s.Models.V1Secret CreateSecret(string name, IDictionary<string, byte[]> secretData, string secretType) {
         k8s.Models.V1Secret result = null;

         if (secretData.Count > 0) {
            result = _k8sClient.CoreV1.CreateNamespacedSecret(
            new k8s.Models.V1Secret(
                  data: secretData,
                  metadata: new k8s.Models.V1ObjectMeta(name: name),
                  type: secretType
               ),
               _namespace
            );
         }
         return result;
      }

      public string DeleteSecret(string name) {
         var status = _k8sClient.CoreV1.DeleteNamespacedSecret(
               name,
               _namespace
            );
         return status.Status;
      }

      public V1SecretList ListSecrets() {
         return _k8sClient.CoreV1.ListNamespacedSecret(_namespace);
      }
      public V1Pod GetPod(string label) {
         var podList = _k8sClient.CoreV1.ListNamespacedPod(_namespace, labelSelector: label);

         return podList?.Items?.FirstOrDefault<V1Pod>();
      }

      public void DeletePod(V1Pod pod) {
         _k8sClient.CoreV1.DeleteNamespacedPodAsync(pod.Metadata.Name, _namespace);
      }

      public string ReadPodLog(
         V1Pod pod,
         int? sinceSeconds = null) {

         using (var stream = _k8sClient.CoreV1.ReadNamespacedPodLog(
            pod.Metadata.Name,
            _namespace,
            follow: false,
            sinceSeconds: sinceSeconds)) {
            using (var reader = new StreamReader(stream)) {
               return reader
                  .ReadToEnd()
                  .Trim()
                  .Replace("\\n", System.Environment.NewLine)
                  .Replace("\u001b[40m\u001b[37mtrce\u001b[39m\u001b[22m\u001b[49m", "trce")
                  .Replace("\u001b[40m\u001b[37mdbug\u001b[39m\u001b[22m\u001b[49m", "dbug")
                  .Replace("\u001b[40m\u001b[32minfo\u001b[39m\u001b[22m\u001b[49m", "info")
                  .Replace("\u001b[40m\u001b[1m\u001b[33mwarn\u001b[39m\u001b[22m\u001b[49m", "warn")
                  .Replace("\u001b[41m\u001b[30mfail\u001b[39m\u001b[22m\u001b[49m", "fail")
                  .Replace("\u001b[41m\u001b[1m\u001b[37mcrit\u001b[39m\u001b[22m\u001b[49m", "crit");
            }
         }
      }
   }
}
