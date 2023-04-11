// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IdentityModel.OidcClient;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.K8sRunspaceProvider {
   public class K8sRunspaceProvider : IRunspaceProvider {
      private readonly ILogger _logger;
      private readonly string _namespace;
      private string _imageName;
      private string _imagePullSecret;
      private bool _verifyRunspaceApiIsAccessibleOnCreate;
      private readonly string _runspaceTrustedCertsConfigMapName;
      private string[] _trustedCertsFileNames;
      private readonly IKubernetes _client;
      private int _runspaceApiPort;
      private int _webConsoleApiPort;
      private int _webConsoleCreationTimeoutMs;
      private const string LABEL_KEY = "runspace";
      private const string RUNSPACE_TYPE = "pcli";
      private const string WEB_CONSOLE_LABEL_KEY = "webconsole";
      private const string WEB_CONSOLE_LABEL_VALUE = "webconsole";
      private const string TYPE_LABEL_KEY = "type";
      private const string TYPE_WORKER_LABEL_VALUE = "worker";

      public K8sRunspaceProvider(
         ILoggerFactory loggerFactory,
         string k8sClusterEndpoint,
         string accessToken,
         string @namespace,
         string imageName,
         int runspaceApiPort,
         string imagePullSecret,
         bool verifyRunspaceApiIsAccessibleOnCreate,
         string runspaceTrustedCertsConfigMapName) : this(
               loggerFactory,
               k8sClusterEndpoint,
               accessToken,
               @namespace,
               imageName,
               runspaceApiPort,
               8086,
               0,
               imagePullSecret,
               verifyRunspaceApiIsAccessibleOnCreate,
               runspaceTrustedCertsConfigMapName) { }

      public K8sRunspaceProvider(
         ILoggerFactory loggerFactory,
         string k8sClusterEndpoint,
         string accessToken,
         string @namespace,
         string imageName,
         int runspaceApiPort,
         int webConsoleApiPort,
         int webConsoleCreationTimeoutMs,
         string imagePullSecret,
         bool verifyRunspaceApiIsAccessibleOnCreate,
         string runspaceTrustedCertsConfigMapName) {

         _logger = loggerFactory.CreateLogger<K8sRunspaceProvider>();

         var config = new KubernetesClientConfiguration() {
            Host = k8sClusterEndpoint ?? @"https://kubernetes.default.svc",
            AccessToken = accessToken ?? File.ReadAllText("/run/secrets/kubernetes.io/serviceaccount/token"),
            SkipTlsVerify = true
         };

         _logger.LogDebug("KubernetesClientConfiguration");
         _logger.LogDebug($"  Host: {config.Host}");
         _logger.LogDebug($"  AccessToken: {config.AccessToken}");
         _logger.LogDebug($"  SkipTlsVerify: {config.SkipTlsVerify}");

         _client = new Kubernetes(config);

         _imageName = imageName;
         _runspaceApiPort = runspaceApiPort;
         _webConsoleApiPort = webConsoleApiPort;
         _webConsoleCreationTimeoutMs = webConsoleCreationTimeoutMs;
         _imagePullSecret = imagePullSecret;
         _namespace = @namespace;
         if (string.IsNullOrEmpty(_namespace)) {
            // read namesplace from service account
            _namespace = File.ReadAllText("/run/secrets/kubernetes.io/serviceaccount/namespace");
         }
         _verifyRunspaceApiIsAccessibleOnCreate = verifyRunspaceApiIsAccessibleOnCreate;
         _runspaceTrustedCertsConfigMapName = runspaceTrustedCertsConfigMapName;

         _logger.LogDebug("Runspace Settings");
         _logger.LogDebug($"  ImageName: {_imageName}");
         _logger.LogDebug($"  RunspaceApiPort: {_runspaceApiPort}");
         _logger.LogDebug($"  ImagePullSecret: {_imagePullSecret}");
         _logger.LogDebug($"  Namespace: {_namespace}");
         _logger.LogDebug($"  VerifyRunspaceApiIsAccessibleOnCreate: {_verifyRunspaceApiIsAccessibleOnCreate}");
         _logger.LogDebug($"  RunspaceTrustedCertsConfigMap: {_runspaceTrustedCertsConfigMapName}");

         ReadTrustedCertsKeys();
      }

      internal K8sRunspaceProvider() {
      }

      #region Private helpers

      private void ReadTrustedCertsKeys() {
         if (!string.IsNullOrEmpty(_runspaceTrustedCertsConfigMapName)) {
            try {
               var configMaps = _client.CoreV1.ListNamespacedConfigMap(_namespace);
               var trustedCertsConfigMap = configMaps.Items
                  .Where<V1ConfigMap>(c => c.Metadata.Name == _runspaceTrustedCertsConfigMapName)
                  .FirstOrDefault<V1ConfigMap>();
               if (trustedCertsConfigMap != null) {
                  _trustedCertsFileNames = trustedCertsConfigMap?.Data?.Keys?.ToArray();
               }
            } catch (Exception exc) {
               _logger.LogError(exc, "Failed to get trusted certificates config map keys");
            }
         }
      }

      private List<V1Volume> CreateRunspacePodVolumes() {
         List<V1Volume> result = null;
         if (_trustedCertsFileNames != null) {
            List<V1KeyToPath> items = new List<V1KeyToPath>();
            foreach (string certFileName in _trustedCertsFileNames) {
               items.Add(new V1KeyToPath(certFileName, certFileName));
            }
            result = new List<V1Volume> {
               {
                  new V1Volume(
                     name: "trusted-certs",
                     configMap: new V1ConfigMapVolumeSource(
                        name: _runspaceTrustedCertsConfigMapName,
                        items: items
                        )
                     )
               }
            };
         }
         return result;
      }

      private List<V1VolumeMount> CreateRunspacePodVolumeMounts() {
         List<V1VolumeMount> result = null;
         if (_trustedCertsFileNames != null) {
            result = new List<V1VolumeMount>();
            foreach (string certFileName in _trustedCertsFileNames) {
               result.Add(new V1VolumeMount(
                  $"/etc/ssl/certs/{certFileName}",
                  "trusted-certs",
                  readOnlyProperty: true,
                  subPath: certFileName));
            }
         }
         return result;
      }

      private string GenerateRunspaceId() {
         var uid = Guid.NewGuid().ToString();
         return $"{RUNSPACE_TYPE}-{uid}";
      }

      private string GenerateWebconsoleId() {
         var uid = Guid.NewGuid().ToString();
         var parts = uid.Split("-");
         return "wc" + parts[parts.Length - 1];
      }

      private V1Pod CreateK8sPod(string podName) {
         _logger.LogDebug($"CreateK8sPod: {podName}");
         var body = new V1Pod(
            "v1",
            "Pod",
            new V1ObjectMeta(
               name: podName,
               labels: new Dictionary<string, string> {
                  { LABEL_KEY, RUNSPACE_TYPE },
                  { TYPE_LABEL_KEY, TYPE_WORKER_LABEL_VALUE }
               }),
            new V1PodSpec(
               new List<V1Container> {
                  {
                     new V1Container(
                        podName,
                        image:_imageName,
                        imagePullPolicy:"IfNotPresent",
                        env: new []{
                           new V1EnvVar("ASPNETCORE_URLS", "http://+:5550")
                        },
                        ports: new [] {
                           new V1ContainerPort(5550)
                        },
                        command: new [] { "/app/service/VMware.ScriptRuntimeService.RunspaceEndpoint" },
                        volumeMounts: CreateRunspacePodVolumeMounts())
                  }
               },
               volumes: CreateRunspacePodVolumes(),
               restartPolicy: "Never"));

         if (!string.IsNullOrEmpty(_imagePullSecret)) {
            body.Spec.ImagePullSecrets = new List<V1LocalObjectReference> (new[] {
               new V1LocalObjectReference(_imagePullSecret)
            });
         }

         _logger.LogDebug($"K8s API Call CreateNamespacedPod: {body}");
         var createdPod = _client.CoreV1.CreateNamespacedPod(body, _namespace);

         return createdPod;
      }

      private V1Deployment CreateK8sApp(string appName, string vc, string token, bool allLinked) {
         _logger.LogDebug($"CreateK8sApp: {appName}");

         var deploymentBody = new V1Deployment(
            "apps/v1",
            "Deployment",
            new V1ObjectMeta(
               labels: new Dictionary<string, string> {
                  { "app", appName },
                  { WEB_CONSOLE_LABEL_KEY, WEB_CONSOLE_LABEL_VALUE } },
               name: appName),
            new V1DeploymentSpec(
               replicas:1,
               selector: new V1LabelSelector(
                  matchLabels: new Dictionary<string, string> { { "app", appName } }
                  ),
               template: new V1PodTemplateSpec(
                  metadata: new V1ObjectMeta(
                     labels: new Dictionary<string, string> {
                        { "app", appName },
                        { WEB_CONSOLE_LABEL_KEY, WEB_CONSOLE_LABEL_VALUE },
                        { TYPE_LABEL_KEY, TYPE_WORKER_LABEL_VALUE } }
                     ),
                  spec: new V1PodSpec(
                     new List<V1Container> {
                        {
                           new V1Container(
                              appName,
                              image: _imageName,
                              env: new []{
                                 new V1EnvVar("vc", vc),
                                 new V1EnvVar("token", token),
                                 new V1EnvVar("allLinked", allLinked.ToString()),
                              },
                              command: new [] { "ttyd" },
                              args: new [] {"-p", "8086", "-b", $"/{appName}", "-T", "linux", "-P", "30", "-m", "1", "pwsh", "-NoExit", "/app/scripts/connect.ps1" },
                              imagePullPolicy:"IfNotPresent",
                              volumeMounts: CreateRunspacePodVolumeMounts())
                        }
                     },
                     volumes: CreateRunspacePodVolumes(),
                     restartPolicy: "Always")
                  )
               )
         );

         var serviceBody = new V1Service(
            "v1",
            "Service",
            new V1ObjectMeta(
               labels: new Dictionary<string, string> { { WEB_CONSOLE_LABEL_KEY, WEB_CONSOLE_LABEL_VALUE } },
               name: appName),
            spec: new V1ServiceSpec(
               type:"ClusterIP",
               sessionAffinity: "None",
               selector: new Dictionary<string, string> { { "app", appName } },
               ports: new List<V1ServicePort> {
                  new V1ServicePort(
                     port: 8086,
                     protocol: "TCP",
                     targetPort: 8086
                     )
                  }
               )
         );

         var deployment = _client.AppsV1.CreateNamespacedDeployment(deploymentBody, _namespace);
         _client.CoreV1.CreateNamespacedService(serviceBody, _namespace);

         AddSrsIngressWebConsolePath(appName);

         return deployment;
      }

      public void AddSrsIngressWebConsolePath(string id) {
         _logger.LogInformation($"Adding ingress rule: /{id}");
         V1Ingress ingress = _client.NetworkingV1.ReadNamespacedIngress("srs-ingress", _namespace);

         // Path to add
         dynamic pathRule = new ExpandoObject();
         pathRule.path = $"/({id}/?)";
         pathRule.pathType = "ImplementationSpecific";
         dynamic backend = new ExpandoObject();
         backend.service = new ExpandoObject() as dynamic;
         backend.service.name = id;
         backend.service.port = new ExpandoObject() as dynamic;
         backend.service.port.number = 8086;
         pathRule.backend = backend;

         // Patch Json Spec
         dynamic ingressToMerge = new ExpandoObject();
         dynamic ingressSpec = new ExpandoObject();
         dynamic ingressSpecRulesHttp = new ExpandoObject();
         dynamic ingressSpecRule = new ExpandoObject();
         ingressSpecRulesHttp.paths = new List<dynamic>();

         // Existing paths
         foreach (var path in ingress.Spec.Rules.Where(r => r.Http != null).First().Http.Paths) {
            dynamic dPath = new ExpandoObject();
            dPath.path = path.Path;
            dPath.pathType = "ImplementationSpecific";
            dynamic dBackend = new ExpandoObject();
            dBackend.service = new ExpandoObject() as dynamic;
            dBackend.service.name = path.Backend.Service.Name;
            dBackend.service.port = new ExpandoObject() as dynamic;
            dBackend.service.port.number = path.Backend.Service.Port.Number;
            dPath.backend = dBackend;
            ingressSpecRulesHttp.paths.Add(dPath);
         }

         // Add the new path
         ingressSpecRulesHttp.paths.Add(pathRule);

         ingressSpecRule.http = ingressSpecRulesHttp;
         dynamic ingressSpecRules = new[] { ingressSpecRule };
         ingressSpec.rules = ingressSpecRules;
         ingressToMerge.spec = ingressSpec;

         _client.NetworkingV1.PatchNamespacedIngress(new V1Patch(
            JsonConvert.SerializeObject(ingressToMerge), V1Patch.PatchType.MergePatch
            ), "srs-ingress", _namespace);
      }

      public void RemoveSrsIngressWebConsolePath(string id) {
         _logger.LogInformation($"Removing ingress rule: {id}");
         V1Ingress ingress = _client.NetworkingV1.ReadNamespacedIngress("srs-ingress", _namespace);

         // Patch Json Spec
         dynamic ingressToMerge = new ExpandoObject();
         dynamic ingressSpec = new ExpandoObject();
         dynamic ingressSpecRulesHttp = new ExpandoObject();
         dynamic ingressSpecRule = new ExpandoObject();
         ingressSpecRulesHttp.paths = new List<dynamic>();

         bool ruleFound = false;
         // Existing paths
         foreach (var path in ingress.Spec.Rules[0].Http.Paths) {
            // Exclude the path to remove
            if (path.Path == $"/({id}/?)") {
               ruleFound = true;
               _logger.LogDebug($"Rule matching path /{id} found");
               continue;
            }

            dynamic dPath = new ExpandoObject();
            dPath.path = path.Path;
            dPath.pathType = "ImplementationSpecific";
            dynamic dBackend = new ExpandoObject();
            dBackend.service = new ExpandoObject() as dynamic;
            dBackend.service.name = path.Backend.Service.Name;
            dBackend.service.port = new ExpandoObject() as dynamic;
            dBackend.service.port.number = path.Backend.Service.Port.Number;
            dPath.backend = dBackend;
            ingressSpecRulesHttp.paths.Add(dPath);
         }

         if (!ruleFound) {
            _logger.LogDebug($"Rule matching path /{id} NOT found!");
         }

         ingressSpecRule.http = ingressSpecRulesHttp;
         dynamic ingressSpecRules = new[] { ingressSpecRule };
         ingressSpec.rules = ingressSpecRules;
         ingressToMerge.spec = ingressSpec;

         _client.NetworkingV1.PatchNamespacedIngress(new V1Patch(
            JsonConvert.SerializeObject(ingressToMerge), V1Patch.PatchType.MergePatch
            ), "srs-ingress", _namespace);
      }

      private static void EnsureRunspaceEndpointIsAccessible(IPEndPoint endPoint) {
         bool ready = false;
         var retryNum = 0;
         var maxRetryCount = 40; // 10 seconds max
         var retryDelayMs = 250;
         while (retryNum < maxRetryCount) {
            using (TcpClient tcpClient = new TcpClient()) {
               try {
                  tcpClient.Connect(endPoint.Address, endPoint.Port);
                  ready = true;
                  break;
               } catch {
                  // ignored
               }
            }

            Thread.Sleep(retryDelayMs);
            retryNum++;
         }

         if (!ready) {
            throw new RunspaceProviderException(Resources.K8sRunspaceProvider_Create_K8sServiceIsNotAccessible);
         }
      }
      #endregion

      public IRunspaceInfo StartCreate() {
         _logger.LogInformation("Create Runspace");
         K8sRunspaceInfo result = null;
         try {
            _logger.LogDebug("GenerateRunspaceId");
            var runspaceId = GenerateRunspaceId();
            _logger.LogDebug($"RunspaceId: {runspaceId}");
            var runspacePod = CreateK8sPod(runspaceId);

            result = new K8sRunspaceInfo {
               Id = runspaceId,
               CreationState = RunspaceCreationState.Creating
            };
            _logger.LogDebug($"RunspaceInfo.Id: {result.Id}");
         } catch (Exception e) {
            LogException(e);

            var error = new RunspaceProviderException(
               Resources.K8sRunspaceProvider_Create_K8sRunspaceCreateFail,
               e);

            result = new K8sRunspaceInfo {
               Id = result?.Id,
               CreationState = RunspaceCreationState.Error,
               CreationError = error
            };
         }

         return result;
      }

      public IWebConsoleInfo CreateWebConsole(string vc, string token, bool allLinked) {
         _logger.LogInformation("Create Web Console Runspace");
         K8sWebConsoleInfo result = null;
         try {
            _logger.LogDebug("GenerateWebconsoleId");
            var webConsoleId = GenerateWebconsoleId();
            _logger.LogDebug($"RunspaceId: {webConsoleId}");
            var runspacePod = CreateK8sApp(webConsoleId, vc, token, allLinked);

            result = new K8sWebConsoleInfo {
               Id = webConsoleId,
               CreationState = RunspaceCreationState.Creating
            };
            _logger.LogDebug($"RunspaceInfo.Id: {result.Id}");
         } catch (Exception e) {
            LogException(e);

            var error = new RunspaceProviderException(
               Resources.K8sRunspaceProvider_Create_K8sRunspaceCreateFail,
               e);

            result = new K8sWebConsoleInfo {
               Id = result?.Id,
               CreationState = RunspaceCreationState.Error,
               CreationError = error
            };
         }

         return result;
      }

      public IWebConsoleInfo GetWebConsole(string id) {
         _logger.LogInformation($"GetWebConsole: {id}");
         return ListWebConsole().FirstOrDefault(r => r.Id == id);
      }

      public IWebConsoleInfo[] ListWebConsole() {
         _logger.LogInformation("List WebConsoles");
         List<IWebConsoleInfo> result = new List<IWebConsoleInfo>();

         try {
            var webConsoleServices = _client.CoreV1.ListNamespacedService(
               _namespace,
               labelSelector: $"{WEB_CONSOLE_LABEL_KEY}={WEB_CONSOLE_LABEL_KEY}");

            foreach (var webConsoleService in webConsoleServices.Items) {
               var webConsoleInfo = new K8sWebConsoleInfo {
                  Id = webConsoleService.Metadata.Name
               };

               webConsoleInfo.CreationState = RunspaceCreationState.Ready;
               result.Add(webConsoleInfo);
            }
         } catch (Exception e) {
            LogException(e);
            throw new RunspaceProviderException(
               Resources.K8sRunspaceProvider_ListWebConsole_K8sWebConsoleListFail,
               e);
         }

         return result.ToArray();
      }

      public void KillWebConsole(string id) {
         _logger.LogInformation($"Kill Runspace: {id}");
         try {
            RemoveSrsIngressWebConsolePath(id);
            _client.AppsV1.DeleteNamespacedDeployment(id, _namespace);
            _client.CoreV1.DeleteNamespacedService(id, _namespace);
            // Wait pod to be deleted
            int maxRetry = 20;
            int retryCount = 1;
            V1Deployment deployment = null;
            V1Service service = null;
            do {
               deployment = null;
               service = null;
               try {
                  deployment = _client.AppsV1.ReadNamespacedDeployment(id, _namespace);
                  service = _client.CoreV1.ReadNamespacedService(id, _namespace);
                  Thread.Sleep(100);
               } catch (Exception ex) {
                  LogExpectedException(ex);
               }
               retryCount++;
            } while (deployment != null && service != null && retryCount < maxRetry);
         } catch (Exception e) {
            LogException(e);
            throw new RunspaceProviderException(
               Resources.K8sRunspaceProvider_Create_K8sRunspaceCreateFail,
               e);
         }
      }

      public IRunspaceInfo WaitCreateCompletion(IRunspaceInfo runspaceInfo) {

         if (runspaceInfo == null) {
            return runspaceInfo;
         }

         V1Pod pod = null;
         try {
            pod = WaitForPodCreation(runspaceInfo.Id);
         } catch (RunspaceProviderException ex) {
            return new K8sRunspaceInfo {
               Id = runspaceInfo.Id,
               CreationState = RunspaceCreationState.Error,
               CreationError = ex
            };
         }

         if (pod == null) {
            return new K8sRunspaceInfo {
               Id = runspaceInfo.Id,
               CreationState = RunspaceCreationState.Error,
               CreationError = new RunspaceProviderException(
                  string.Format(
                     Resources.K8sRunspaceProvider_WaitCreateComplation_PodNotFound, runspaceInfo.Id))
            };
         }

         var result = new K8sRunspaceInfo {
            Id = runspaceInfo.Id,
            Endpoint =
                  new IPEndPoint(
                     IPAddress.Parse(pod.Status.PodIP),
                     _runspaceApiPort),
            CreationState = RunspaceCreationState.Ready
         };

         if (_verifyRunspaceApiIsAccessibleOnCreate) {
            try {
               _logger.LogDebug($"EnsureRunspaceEndpointIsAccessible: Start");
               // Ensure Container is accessible over the network after creation
               EnsureRunspaceEndpointIsAccessible(result.Endpoint);
               _logger.LogDebug($"EnsureRunspaceEndpointIsAccessible: Success");

               return result;
            } catch (RunspaceProviderException exc) {
               LogException(exc);
               // Kill the container that is not accessible, otherwise it will leak
               try {
                  Kill(result.Id);
               } catch (RunspaceProviderException rexc) {
                  LogException(rexc);
               }

               return new K8sRunspaceInfo {
                  Id = result.Id,
                  CreationState = RunspaceCreationState.Error,
                  CreationError = exc
               };
            }
         } else {
            return result;
         }
      }

      public IWebConsoleInfo WaitCreateCompletion(IWebConsoleInfo webConsoleInfo) {
         V1PodList podList = null;
         try {
            _logger.LogDebug($"Waiting k8s Pod 'app:{webConsoleInfo.Id}' to become ready");
            _logger.LogDebug($"K8s API Call ListNamespacedPod: {webConsoleInfo.Id}");
            podList = _client.CoreV1.ListNamespacedPod(_namespace, labelSelector: $"app={webConsoleInfo.Id}");
         } catch (Exception exc) {
            LogException(exc);
            return new K8sWebConsoleInfo {
               Id = webConsoleInfo.Id,
               CreationState = RunspaceCreationState.Error,
               CreationError = new RunspaceProviderException(
                  string.Format(
                     Resources.K8sRunspaceProvider_WaitCreateComplation_PodNotFound, webConsoleInfo.Id),
                  exc)
            };
         }

         if (null == podList || podList.Items.Count == 0) {
            _logger.LogError($"Web console pod {webConsoleInfo.Id} not found.");
            return new K8sWebConsoleInfo {
               Id = webConsoleInfo.Id,
               CreationState = RunspaceCreationState.Error,
               CreationError = new RunspaceProviderException(
                  string.Format(
                     Resources.K8sRunspaceProvider_WaitCreateComplation_PodNotFound, webConsoleInfo.Id))
            };
         } else if (podList.Items.Count > 1) {
            _logger.LogError($"Too many web console pods for {webConsoleInfo.Id} found.");
            return new K8sWebConsoleInfo {
               Id = webConsoleInfo.Id,
               CreationState = RunspaceCreationState.Error,
               CreationError = new RunspaceProviderException(
                  string.Format(
                     Resources.K8sRunspaceProvider_WaitCreateComplation_ManyPodFound, webConsoleInfo.Id))
            };
         } else {
            _logger.LogDebug($"Web console pod for {webConsoleInfo.Id} found.");
            IWebConsoleInfo podWaitResult = null;
            try {
               var pod = WaitForPodCreation(podList.Items[0].Name());
               podWaitResult = new K8sWebConsoleInfo {
                  Id = webConsoleInfo.Id,
                  CreationState = RunspaceCreationState.Ready,
                  Endpoint = new IPEndPoint(
                     IPAddress.Parse(pod.Status.PodIP),
                     _webConsoleApiPort)
               };
            } catch (RunspaceProviderException ex) {
               return new K8sWebConsoleInfo {
                  Id = webConsoleInfo.Id,
                  CreationState = RunspaceCreationState.Error,
                  CreationError = ex
               };
            }

            if (podWaitResult?.CreationState == RunspaceCreationState.Ready) {
               // The pod is up and running we not wait for UPDATE event from the ingress controller
               // Corev1EventList eventList = null;
               V1Ingress ingress = null;

               // Set 10 minutes timeout for container creation.
               // Worst case would be image pulling from server.
               int maxRetryCount = 6000;
               int retryIntervalMs = 100;
               int retryCount = 1;

               // Wait Pod to become running and obtain IP Address
               // _logger.LogDebug($"Start waiting k8s for nginx ingress controller to update after the rule change on {creationTime}");

               do {
                  try {
                     _logger.LogTrace($"K8s API Call ReadNamespacedIngress: \"srs-ingress\"");
                     ingress = _client.NetworkingV1.ReadNamespacedIngress("srs-ingress", _namespace);
                  } catch (Exception exc) {
                     LogException(exc);
                     return new K8sWebConsoleInfo {
                        Id = webConsoleInfo.Id,
                        CreationState = RunspaceCreationState.Error,
                        CreationError = new RunspaceProviderException(
                           string.Format(
                              Resources.K8sRunspaceProvider_WaitCreateComplation_ListEvents, webConsoleInfo.Id),
                           exc)
                     };
                  }

                  if (ingress?.Spec.Rules[0].Http.Paths.Any(p => p.Path == $"/({webConsoleInfo.Id}/?)") ?? false) {
                     _logger.LogDebug($"srs-ingress updated. Web console {webConsoleInfo.Id} can be accessed.");
                     break;
                  }

                  Thread.Sleep(retryIntervalMs);

                  retryCount++;
               } while (retryCount < maxRetryCount);

               if (retryCount >= maxRetryCount) {
                  // Timeout
                  return new K8sWebConsoleInfo {
                     Id = webConsoleInfo.Id,
                     CreationState = RunspaceCreationState.Error,
                     CreationError = new RunspaceProviderException(Resources.K8sRunspaceProvider_WaitCreateComplition_TimeOut)
                  };
               } else {
                  if (_webConsoleCreationTimeoutMs > 0) {
                     _logger.LogDebug($"Web console creation additional timeout {_webConsoleCreationTimeoutMs}ms.");
                     Thread.Sleep(_webConsoleCreationTimeoutMs);
                  }

                  try {
                     _logger.LogDebug($"EnsureRunspaceEndpointIsAccessible: Start IP:{webConsoleInfo.Endpoint?.Address}, Port:{webConsoleInfo.Endpoint?.Port}");
                     // Ensure Container is accessible over the network after creation
                     EnsureRunspaceEndpointIsAccessible(webConsoleInfo.Endpoint);
                     _logger.LogDebug($"EnsureRunspaceEndpointIsAccessible: Success");

                     // Success, everything should be in place
                     return new K8sWebConsoleInfo {
                        Id = webConsoleInfo.Id,
                        CreationState = RunspaceCreationState.Ready,
                        Endpoint = webConsoleInfo.Endpoint
                     };
                  } catch (RunspaceProviderException exc) {
                     LogException(exc);
                     // Kill the container that is not accessible, otherwise it will leak
                     try {
                        KillWebConsole(webConsoleInfo.Id);
                     } catch (RunspaceProviderException rexc) {
                        LogException(rexc);
                     }

                     return new K8sRunspaceInfo {
                        Id = webConsoleInfo.Id,
                        CreationState = RunspaceCreationState.Error,
                        CreationError = exc
                     };
                  }
               }
            } else {
               return podWaitResult;
            }
         }
      }

      private static bool IsNginxReloadEventAfter(Corev1Event e, DateTime since) {
         return e.LastTimestamp?.CompareTo(since) > 0 &&
            e.Type.Equals("Normal", StringComparison.InvariantCultureIgnoreCase) &&
            e.Reason.Equals("RELOAD", StringComparison.InvariantCultureIgnoreCase) &&
            e.Message.Equals("NGINX reload triggered due to a change in configuration");
      }

      private V1Pod WaitForPodCreation(string name) {
         V1Pod pod = null;
         try {
            _logger.LogDebug($"Waiting k8s Pod '{name}' to become ready");
            _logger.LogDebug($"K8s API Call ReadNamespacedPod: {name}");
            pod = _client.CoreV1.ReadNamespacedPod(name, _namespace);
         } catch (Exception exc) {
            LogException(exc);
            throw new RunspaceProviderException(
                  string.Format(
                     Resources.K8sRunspaceProvider_WaitCreateComplation_PodNotFound, name),
                  exc);
         }

         if (pod == null) {
            throw new RunspaceProviderException(
                  string.Format(
                     Resources.K8sRunspaceProvider_WaitCreateComplation_PodNotFound, name));
         }

         // Set 10 minutes timeout for container creation.
         // Worst case would be image pulling from server.
         int maxRetryCount = 6000;
         int retryIntervalMs = 100;
         int retryCount = 1;

         // Wait Pod to become running and obtain IP Address
         _logger.LogDebug($"Start wating K8s Pod to become running: {pod.Metadata.Name}");
         // There are three possible phases of a POD
         // Pending - awaiting containers to start
         // Running - Pod is initialized and all containers in the Pod are running or completed successfully
         // Terminating - Pod is terminating

         // The Pending phase could last forever when container image pull error occurred or some other error
         // in the container initialization happens. In order to stop waiting below we first monitor for Pod status to
         // phase to switch from pending to running. While Pod is pending phase we monitor the container
         // creation for errors and if such occur we break the waiting with error.
         //
         // The Container creation errors that are related to image pulling failura are stored in the
         // pod.Status.ContainerStatuses[0].State.Waiting propery is not null which is instance of V1ContainerStateWaiting
         // The errors are returned as strings in Reason property of the V1ContainerStateWaiting
         // The strings that represent errors are:
         //
         // ImagePullBackOff - Container image pull failed, kubelet is backing off image pull
         // ImageInspectError - Unable to inspect image
         // ErrImagePull - General image pull error
         // ErrImageNeverPull - Required Image is absent on host and PullPolicy is NeverPullImage
         // RegistryUnavailable - Get http error when pulling image from registry
         // InvalidImageName - Unable to parse the image name.

         while (
            pod != null &&
            string.IsNullOrEmpty(pod.Status?.PodIP) &&
            (pod.Status?.Phase != "Running" || (pod.Status?.Phase == "Pending" && !HasErrrorInContainerStatus(pod.Status, out var _))) &&
            retryCount < maxRetryCount) {

            Thread.Sleep(retryIntervalMs);
            _logger.LogDebug($"K8s API Call ReadNamespacedPod: {pod.Metadata.Name}");
            pod = _client.CoreV1.ReadNamespacedPod(pod.Metadata.Name, _namespace);
            retryCount++;
         }

         if (retryCount >= maxRetryCount) {
            // Timeout
            throw new RunspaceProviderException(Resources.K8sRunspaceProvider_WaitCreateComplition_TimeOut);
         } else
         if (HasErrrorInContainerStatus(pod.Status, out var errorMessage)) {
            // Container Creation Error
            throw new RunspaceProviderException(errorMessage);
         } else {
            // Success, everything should be in place
         }

         return pod;
      }

      /// Returns true if container creation error is identified in PodStatus, otherwise false
      private bool HasErrrorInContainerStatus(V1PodStatus status, out string errorMessage) {
         var result = false;
         errorMessage = string.Empty;

         var possibleWaitingErrorReasons = new string[] {
            "IMAGEPULLBACKOFF",
            "IMAGEINSPECTERROR",
            "ERRIMAGEPULL",
            "ERRIMAGENEVERPULL",
            "REGISTRYUNAVAILABLE",
            "INVALIDIMAGENAME"
         };

         var containerStatus = status.ContainerStatuses.FirstOrDefault<V1ContainerStatus>();

         if (!string.IsNullOrEmpty(containerStatus?.State?.Waiting?.Reason)) {
            _logger.LogDebug($"HasErrrorInContainerStatus -> Container State is 'Waiting' with reason: {containerStatus?.State?.Waiting?.Reason}");

            if (possibleWaitingErrorReasons.
                  Contains(containerStatus.
                  State.
                  Waiting.
                  Reason.
                  ToUpper())) {

               result = true;
               errorMessage = containerStatus.
                  State.
                  Waiting.
                  Message;
            }
         }

         return result;
      }

      public IRunspaceInfo Get(string id) {
         _logger.LogInformation($"Get Runspace: {id}");
         return List().FirstOrDefault(r => r.Id == id);
      }

      public void Kill(string id) {
         _logger.LogInformation($"Kill Runspace: {id}");
         try {
            _client.CoreV1.DeleteNamespacedPod(id, _namespace);
            // Wait pod to be deleted
            int maxRetry = 20;
            int retryCount = 1;
            V1Pod pod = null;
            do {
               pod = null;
               try {
                  pod = _client.CoreV1.ReadNamespacedPod(id, _namespace);
                  Thread.Sleep(100);
               } catch (Exception ex) {
                  LogExpectedException(ex);
               }
               retryCount++;
            } while (pod != null && pod.Status?.Phase == "Running" && retryCount < maxRetry);
         } catch (Exception e) {
            LogException(e);
            throw new RunspaceProviderException(
               Resources.K8sRunspaceProvider_Create_K8sRunspaceCreateFail,
               e);
         }
      }

      public IRunspaceInfo[] List() {
         _logger.LogInformation("List Runspaces");
         List<IRunspaceInfo> result = new List<IRunspaceInfo>();

         try {
            var runspacePods = _client.CoreV1.ListNamespacedPod(
               _namespace,
               labelSelector : $"{LABEL_KEY}={RUNSPACE_TYPE}");

            foreach (var runspacePod in runspacePods.Items) {
               if (runspacePod?.Status?.Phase != "Running" &&
                  runspacePod?.Status?.Phase != "Pending") {
                  continue;
               }

               var runspaceInfo = new K8sRunspaceInfo {
                  Id = runspacePod.Metadata.Name
               };

               if (runspacePod?.Status?.Phase == "Pending") {
                  runspaceInfo.CreationState = RunspaceCreationState.Creating;
               } else {
                  runspaceInfo.Endpoint = new IPEndPoint(
                     IPAddress.Parse(runspacePod.Status.PodIP),
                     _runspaceApiPort);
                  runspaceInfo.CreationState = RunspaceCreationState.Ready;
               }

               result.Add(runspaceInfo);
            }
         } catch (Exception e) {
            LogException(e);
            throw new RunspaceProviderException(
               Resources.K8sRunspaceProvider_List_K8sRunspaceListFail,
               e);
         }

         return result.ToArray();
      }

      public void UpdateConfiguration(IRunspaceProviderSettings runspaceProviderSettings) {
         var newConfig = runspaceProviderSettings as K8sRunspaceProviderSettings;
         if (newConfig != null) {
            _imagePullSecret = newConfig.ImagePullSecret;
            _imageName = newConfig.RunspaceImageName;
            _runspaceApiPort = newConfig.RunspacePort;
            _verifyRunspaceApiIsAccessibleOnCreate = newConfig.VerifyRunspaceApiIsAccessibleOnCreate;
         }
      }

      private void LogException(Exception ex) {
         _logger.LogError(ex.ToString());
         _logger.LogTrace(ex, ex.ToString());
      }

      private void LogExpectedException(Exception ex) {
         _logger.LogDebug("[EXPECTED EXCEPTION] " + ex.ToString());
         _logger.LogTrace(ex, "[EXPECTED EXCEPTION] " + ex.ToString());
      }
   }
}
