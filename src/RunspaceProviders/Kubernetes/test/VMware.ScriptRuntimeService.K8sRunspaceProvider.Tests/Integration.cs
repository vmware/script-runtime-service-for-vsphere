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
      private const string K8S_API_ENDPOINT = "https://10.23.82.191:6443";
      private const string K8S_API_KEY = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ing5VGZpRVhqNnlRbk4xdWRyQzViaDg2NWxBb0dCR3ZhQ3lwRkV5em5QdVUifQ.eyJpc3MiOiJrdWJlcm5ldGVzL3NlcnZpY2VhY2NvdW50Iiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9uYW1lc3BhY2UiOiJzY3JpcHQtcnVudGltZS1zZXJ2aWNlIiwia3ViZXJuZXRlcy5pby9zZXJ2aWNlYWNjb3VudC9zZWNyZXQubmFtZSI6InNycy1ydW5zcGFjZS1wcm92aWRlci10b2tlbi1jcmJueiIsImt1YmVybmV0ZXMuaW8vc2VydmljZWFjY291bnQvc2VydmljZS1hY2NvdW50Lm5hbWUiOiJzcnMtcnVuc3BhY2UtcHJvdmlkZXIiLCJrdWJlcm5ldGVzLmlvL3NlcnZpY2VhY2NvdW50L3NlcnZpY2UtYWNjb3VudC51aWQiOiI4YjA3MmRlNC0yOTJlLTQwNDMtOGIyMC04NjJjZmFmMjA1ZGEiLCJzdWIiOiJzeXN0ZW06c2VydmljZWFjY291bnQ6c2NyaXB0LXJ1bnRpbWUtc2VydmljZTpzcnMtcnVuc3BhY2UtcHJvdmlkZXIifQ.vUzAth823V4qZYOcMTrSyBWa9H8wpleWV3u7Xbp7Z8WFzz6Q9zWKFqWBHYSBZOehhr9630E90BmgxOjiPyAfNhNSI4n0UmuH3rSua2DSvYj1SiTwy7n5DJ3MGpTHgcDcqzPV-krxK6gqD9Oe8WV7BPW1hJRoqWhfP8xwFBuNw9LrSD3qa5s0kaO03Y6V3sWe04Sij28XpO2Ia1IS-FhgZk5KzHnb7mBB__wUwh7kd2zP6722DXJdwJfFSctUH7eyCkIpAwjHITnxH97skqD3LnCbCqrp9d9B_Kum5aTxuZDsOqItDqb3jpvxjyuNpSZlbXp3Z8hqutrBWFuULeZZDw";
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
            "pclirunspace:12.1",
            5555,
            null,
            false,
            "trusted-ca-certificates");
      }

      [Test]
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

      [Test]
      public void TestCreateWebConsole() {
         var webConsole = _k8sProvider.CreateWebConsole("", "", false);
         Assert.NotNull(webConsole);
      }


      [Test]
      public void TestKillWebConsole() {         
         _k8sProvider.KillWebConsole("pcli-c0f80b2c-7ba4-4c4d-a2f6-b939a6ce3f63");
      }

      [Test]
      public void TestAddWebConsolePathToIngress() {
         _k8sProvider.AddSrsIngressWebConsolePath("pcli-e6d4663c-a755-455d-98e6-87dc7c7b8988");         
      }

      [Test]
      public void TestAddRemoveWebConsolePathToIngress() {
         _k8sProvider.RemoveSrsIngressWebConsolePath("pcli-e6d4663c-a755-455d-98e6-87dc7c7b8988");
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

      [Test]
      public void TestKill() {
         var runspaceInfo = _k8sProvider.List().FirstOrDefault();
         _k8sProvider.Kill(runspaceInfo.Id);         
         runspaceInfo = _k8sProvider.List().FirstOrDefault();
         Assert.IsNull(runspaceInfo);
      }
   }
}