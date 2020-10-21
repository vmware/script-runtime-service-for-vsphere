// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.K8sRunspaceProvider;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.K8sRunspaceProvider.Tests {
   /// <summary>
   /// Contains integrations tests interacting with K8S API
   /// To run those configure K8s cluster with API available on constant K8S_API_ENDPOINT
   /// and K8S API KEY on constant K8S_API_KEY, then uncomment [Test] attributes
   /// </summary>
   public class Itnegrationtests {
      private const string K8S_API_ENDPOINT = "https://10.23.80.72:5443";
      private const string K8S_API_KEY = "eyJhbGciOiJSUzI1NiIsImtpZCI6IlAxUHloOHlTYW1XeGVUUXhHaTE5QUQ1NHRqQXBSbGJmWUJ3OGstdDRVNTgifQ.eyJpc3MiOiJrdWJlcm5ldGVzL3NlcnZpY2VhY2NvdW50Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9uYW1lc3BhY2UiOiJzZXMtc2VydmljZSIsImt1YmVybmV0ZXMuaW8vc2VydmljZWFjY291bnQvc2VjcmV0Lm5hbWUiOiJzZXMtcnVuc3BhY2UtcHJvdmlkZXItdG9rZW4tYm40Y3oiLCJrdWJlcm5ldGVzLmlvL3NlcnZpY2VhY2NvdW50L3NlcnZpY2UtYWNjb3VudC5uYW1lIjoic2VzLXJ1bnNwYWNlLXByb3ZpZGVyIiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9zZXJ2aWNlLWFjY291bnQudWlkIjoiODkyMzMyNTAtYzIwOS00YTBiLThhYzEtOGVmNWIwMTMzOTJmIiwic3ViIjoic3lzdGVtOnNlcnZpY2VhY2NvdW50OnNlcy1zZXJ2aWNlOnNlcy1ydW5zcGFjZS1wcm92aWRlciJ9.eN0mevVMURTegRYZS4eTYGXkVoEReOogGO1gZbhABj6A_0uw4IcXI2QNlJXBq6caZRLmjl6giMHlST8wmg3myPH8YihzhZB1UDhvtPWCW_Nfn1t4iU9M4KWFByWrn4D2nxDcKpWIroZjk2rMXpMei8AaRDJBCKk9KUFuMJBTGOvX1qkK2Xj8Sd5SAr8DRILSDMgt31b2n_QUiMRNus0kV6Qhj_5JIuSg6PA9ZZO4Xj5kLjgytoDmXQxIOLf_tfk4h5mMmeLEApN1c2c9KB8lvxt-BUV9ceGVICn_h79OlPRKhqjsLevHgULZV1so-D8zLIwEHU2YcD7jDLACGCmy1A";
      private K8sRunspaceProvider _k8sProvider;
      [SetUp]
      public void Setup() {

         var loggerFactoryMock = new Mock<ILoggerFactory>();
         loggerFactoryMock.Setup(
            x => x.CreateLogger(typeof(K8sRunspaceProvider).ToString()))
            .Returns(new Mock<ILogger>().Object);

         _k8sProvider = new K8sRunspaceProvider(
            loggerFactoryMock.Object,
            K8S_API_ENDPOINT,
            K8S_API_KEY,
            "script-runtime-service",
            "pclirunspace:latest",
            5555,
            "regcred",
            false,
            "trusted-ca-certificates");
      }

      //[Test]
      public void TestCreate() {
         var runspaceInfo = _k8sProvider.StartCreate();
         runspaceInfo = _k8sProvider.WaitCreateCompletion(runspaceInfo);
         Assert.NotNull(runspaceInfo);
         Assert.NotNull(runspaceInfo.Id);
         Assert.NotNull(runspaceInfo.Endpoint);
         Assert.NotNull(runspaceInfo.Endpoint.Address);
         Assert.NotNull(runspaceInfo.Endpoint.Port);
         Assert.AreEqual(RunspaceCreationState.Ready, runspaceInfo.CreationState);
      }

      //[Test]
      public void TestList() {
         var runspaceInfo = _k8sProvider.List().FirstOrDefault();
         Assert.NotNull(runspaceInfo);
         Assert.NotNull(runspaceInfo.Id);
         Assert.NotNull(runspaceInfo.Endpoint);
         Assert.NotNull(runspaceInfo.Endpoint.Address);
         Assert.NotNull(runspaceInfo.Endpoint.Port);
      }

      //[Test]
      public void TestKill() {
         var runspaceInfo = _k8sProvider.List().FirstOrDefault();
         _k8sProvider.Kill(runspaceInfo.Id);         
         runspaceInfo = _k8sProvider.List().FirstOrDefault();
         Assert.IsNull(runspaceInfo);
      }
   }
}