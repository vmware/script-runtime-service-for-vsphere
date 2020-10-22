// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using VMware.ScriptRuntimeService.Setup;

namespace VMware.ScriptRuntimeService.Setup.Tests
{
   /// <summary>
   /// Contains integrations tests interacting with K8S API
   /// To run those configure K8s cluster with API available on constant K8S_API_ENDPOINT
   /// and K8S API KEY on constant K8S_API_KEY, then uncomment [Test] attributes
   /// </summary>
   public class K8sClientIntegrationTests {
      private const string K8S_API_ENDPOINT = "https://10.23.80.72:5443";
      private const string K8S_API_KEY = "eyJhbGciOiJSUzI1NiIsImtpZCI6IlAxUHloOHlTYW1XeGVUUXhHaTE5QUQ1NHRqQXBSbGJmWUJ3OGstdDRVNTgifQ.eyJpc3MiOiJrdWJlcm5ldGVzL3NlcnZpY2VhY2NvdW50Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9uYW1lc3BhY2UiOiJzZXMtc2VydmljZSIsImt1YmVybmV0ZXMuaW8vc2VydmljZWFjY291bnQvc2VjcmV0Lm5hbWUiOiJzZXMtcnVuc3BhY2UtcHJvdmlkZXItdG9rZW4tYm40Y3oiLCJrdWJlcm5ldGVzLmlvL3NlcnZpY2VhY2NvdW50L3NlcnZpY2UtYWNjb3VudC5uYW1lIjoic2VzLXJ1bnNwYWNlLXByb3ZpZGVyIiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9zZXJ2aWNlLWFjY291bnQudWlkIjoiODkyMzMyNTAtYzIwOS00YTBiLThhYzEtOGVmNWIwMTMzOTJmIiwic3ViIjoic3lzdGVtOnNlcnZpY2VhY2NvdW50OnNlcy1zZXJ2aWNlOnNlcy1ydW5zcGFjZS1wcm92aWRlciJ9.eN0mevVMURTegRYZS4eTYGXkVoEReOogGO1gZbhABj6A_0uw4IcXI2QNlJXBq6caZRLmjl6giMHlST8wmg3myPH8YihzhZB1UDhvtPWCW_Nfn1t4iU9M4KWFByWrn4D2nxDcKpWIroZjk2rMXpMei8AaRDJBCKk9KUFuMJBTGOvX1qkK2Xj8Sd5SAr8DRILSDMgt31b2n_QUiMRNus0kV6Qhj_5JIuSg6PA9ZZO4Xj5kLjgytoDmXQxIOLf_tfk4h5mMmeLEApN1c2c9KB8lvxt-BUV9ceGVICn_h79OlPRKhqjsLevHgULZV1so-D8zLIwEHU2YcD7jDLACGCmy1A";
      K8sClient.K8sClient _k8sClient;
      [SetUp]
      public void Setup() {

         var loggerFactoryMock = new Mock<ILoggerFactory>();
         loggerFactoryMock.Setup(
            x => x.CreateLogger(typeof(K8sClient.K8sClient).ToString()))
            .Returns(new Mock<ILogger>().Object);

         _k8sClient = new K8sClient.K8sClient(
            loggerFactoryMock.Object,
            K8S_API_ENDPOINT,
            K8S_API_KEY,
            "default");
      }


      //[Test]
      public void CreateConfigMap() {
         // Arrange
         var configMapName = "test-cmap";
         var data = new Dictionary<string, string>() {
            { "settings.json", "{\"RunspaceProviderSettings\": {\"LocalhostDebug\": false}}" }
         };

         // Act
         var actual = _k8sClient.CreateConfigMap(configMapName, data);

         // Assert
         Assert.NotNull(actual);
         Assert.AreEqual(configMapName, actual.Metadata.Name);

         // Clean
         var status = _k8sClient.DeleteConfigMap(configMapName);
         Assert.AreEqual("Success", status);
      }

      //[Test]
      public void DeleteExistingConfigMap() {
         // Arrange
         var configMapName = "test-cmap";
         var data = new Dictionary<string, string>() {
            { "settings.json", "{\"RunspaceProviderSettings\": {\"LocalhostDebug\": false}}" }
         };
         _k8sClient.CreateConfigMap(configMapName, data);

         // Act
         var actual = _k8sClient.DeleteConfigMap(configMapName);

         // Assert
         Assert.NotNull(actual);         
         Assert.AreEqual("Success", actual);
      }


      //[Test]
      public void RecreateConfigMap() {
         // Arrange
         var configMapName = "test-cmap";
         var data = new Dictionary<string, string>() {
            { "settings.json", "{\"RunspaceProviderSettings\": {\"LocalhostDebug\": false}}" }
         };
         _k8sClient.CreateConfigMap(configMapName, data);

         var updateData = new Dictionary<string, string>() {
            { "settings.json", "{\"RunspaceProviderSettings\": {\"LocalhostDebug\": true}}" }
         };

         // Act
         _k8sClient.RecreateConfigMap(configMapName, updateData);

         // Assert
         var actual = _k8sClient.GetConfigMapData(configMapName, "settings.json");
         Assert.NotNull(actual);
         Assert.IsTrue(actual.Contains("{\"LocalhostDebug\": true}"));

         // Clean
         var status = _k8sClient.DeleteConfigMap(configMapName);
         Assert.AreEqual("Success", status);
      }

      //[Test]
      public void CreateBinarySecretFromP12CertFile() {
         // Arrange
         var secretName = "test-cert-secret";
         var certFile = Path.Combine("resources", "testcert.p12");

         // Act
         var actual = _k8sClient.CreateBinarySecret(secretName, certFile);

         // Assert
         Assert.NotNull(actual);
         Assert.AreEqual(secretName, actual.Metadata.Name);

         // Clean
         var status = _k8sClient.DeleteSecret(secretName);
         Assert.AreEqual("Success", status);
      }

      //[Test]
      public void CreateTlsSecret() {
         // Arrange
         var secretName = "test-tls-secret";
         var crtFile = Path.Combine("resources", "testtls.crt");
         var keyFile = Path.Combine("resources", "testtls.key");

         // Act
         var actual = _k8sClient.CreateTlsSecret(secretName, crtFile, keyFile);

         // Assert
         Assert.NotNull(actual);
         Assert.AreEqual(secretName, actual.Metadata.Name);

         // Clean
         var status = _k8sClient.DeleteSecret(secretName);
         Assert.AreEqual("Success", status);
      }

      //[Test]
      public void ListConfigMap() {
         // Act
         var configMaps = _k8sClient.ListConfigMap();

         // Assert
         Assert.NotNull(configMaps);
      }

      //[Test]
      public void ListSecrets() {
         // Act
         var secrets = _k8sClient.ListSecrets();

         // Assert
         Assert.NotNull(secrets);
      }
   }
}
