// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using k8s.KubeConfigModels;
using Microsoft.Extensions.Logging;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.ScriptExecution.Impl;
using VMware.ScriptRuntimeService.APIGateway.Sts;
using VMware.ScriptRuntimeService.APIGateway.Sts.Impl;
using VMware.ScriptRuntimeService.APIGateway.SystemScripts;
using VMware.ScriptRuntimeService.K8sRunspaceProvider;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl
{
   internal class MultiTenantRunspaceProvider : IMultiTenantRunspaceProvider, IDisposable {
      private ILogger _logger;
      private IRunspaceProvider _runspaceProvider;
      private IRunspacesStatsMonitor _runspacesStatsMonitor;
      private UserToIdentifiableData<IRunspaceData> _userRunspaces = new UserToIdentifiableData<IRunspaceData>();
      private Timer _runspacesCleanupTimer;

      public MultiTenantRunspaceProvider(ILoggerFactory loggerFactory, IRunspaceProvider runspaceProvider) : 
         this(loggerFactory, runspaceProvider, Int32.MaxValue, 24 * 60, 24 * 60) {}

      public MultiTenantRunspaceProvider(
         ILoggerFactory loggerFactory,
         IRunspaceProvider runspaceProvider, 
         int maxNumberOfRunspaces,
         int maxRunspaceIdleTimeMinutes,
         int maxRunspaceActiveTimeMinutes,
         IRunspacesStatsMonitor runspacesStatsMonitor = null) {
         if (runspaceProvider == null) throw new ArgumentNullException(nameof(runspaceProvider));

         _runspaceProvider = runspaceProvider;
         _logger = loggerFactory.CreateLogger(typeof(MultiTenantRunspaceProvider).FullName);

         _logger.LogDebug("Runspace Provider Settings:");
         _logger.LogDebug($"   MaxNumberOfRunspaces: {maxNumberOfRunspaces}");
         _logger.LogDebug($"   MaxRunspaceIdleTimeMinutes: {maxRunspaceIdleTimeMinutes}");
         _logger.LogDebug($"   MaxRunspaceActiveTimeMinutes: {maxRunspaceActiveTimeMinutes}");

         if (runspacesStatsMonitor == null) {
            _runspacesStatsMonitor = new RunspacesStatsMonitor(
               maxNumberOfRunspaces,
               maxRunspaceIdleTimeMinutes,
               maxRunspaceActiveTimeMinutes);
         } else {
            _runspacesStatsMonitor = runspacesStatsMonitor;
         }

         _runspacesCleanupTimer = new Timer(
            CleanupTimerCallback, 
            null, 
            30 * 1000, 
            60 * 1000);
      }

      private void CleanupTimerCallback(Object stateInfo) {
         try {
            Cleanup();
         } catch (Exception exc) {
            _logger.Log(LogLevel.Error, exc.ToString());
         }
      }

      public bool CanCreateNewRunspace() {
         return _runspacesStatsMonitor.IsCreateNewRunspaceAllowed();
      }

      public IRunspaceData StartCreate(
         string userId,
         ISessionToken sessionToken, 
         string name, 
         bool runVcConnectionScript,
         ISolutionStsClient stsClient,
         string vcEndpoint) {

         IRunspaceData result = null;

         _logger.LogInformation("StartCreate");

         try {
            Sessions.Instance.EnsureValidUser(userId);
            _logger.LogDebug("RunspaceProvider -> StartCreate call");
            var runspaceInfo = _runspaceProvider.StartCreate();
            _logger.LogDebug($"Runspace provider result: {runspaceInfo.Id}, {runspaceInfo.CreationState}, {runspaceInfo.CreationError}");
            result = new RunspaceData(runspaceInfo);
            result.CreationTime = DateTime.Now;
            result.Name = name;
            result.RunVcConnectionScript = runVcConnectionScript;
            result.State = DataTypes.RunspaceState.Creating;

            _userRunspaces.Add(userId, result.Id, result);
            _runspacesStatsMonitor.Register(result, sessionToken.SessionId);
            
            Task.Run(() => {
               _logger.LogDebug("RunspaceProvider -> WaitCreateCompletion call");
               var waitResult = _runspaceProvider.WaitCreateCompletion(result);
               _logger.LogDebug($"Runspace provider WaitCreateCompletion result: {waitResult.Id}, {waitResult.CreationState}, {waitResult.CreationError}");
               if (waitResult.CreationState == RunspaceCreationState.Error) {
                  ((RunspaceData)result).ErrorDetails = new DataTypes.ErrorDetails(waitResult.CreationError);
                  ((RunspaceData)result).State = DataTypes.RunspaceState.Error;
               } else {
                  // Update endpoint info
                  ((RunspaceData)result).Endpoint = waitResult.Endpoint;
               }

               if (waitResult.CreationState == RunspaceCreationState.Ready &&
                     !runVcConnectionScript) {
                  ((RunspaceData)result).State = DataTypes.RunspaceState.Ready;                              
               }

               _logger.LogDebug($"Connect VC requested: {runVcConnectionScript}");               
               if (runVcConnectionScript && waitResult.CreationState == RunspaceCreationState.Ready) {
                  string bearerSamlToken = null;

                  try {
                     _logger.LogDebug($"HoK Saml Token availble: {sessionToken.HoKSamlToken != null}");
                     if (sessionToken.HoKSamlToken == null) {
                        throw new Exception(APIGatewayResources.PowerCLIVCloginController_NoRefreshTokenAvailable_For_Session);
                     }

                     _logger.LogDebug($"STSClient -> IssueBearerTokenBySolutionToken call");
                     bearerSamlToken = stsClient
                        .IssueBearerTokenBySolutionToken(sessionToken.HoKSamlToken.RawXmlElement)
                        .OuterXml;
                  } catch (Exception exc) {
                     _logger.LogError(exc, "Issue Bearer Token failed");
                     result.State = DataTypes.RunspaceState.Error;
                     result.ErrorDetails = new DataTypes.ErrorDetails(exc);
                  }

                  if (bearerSamlToken != null) {
                     // Connect PowerCLI
                     try {
                        var scriptExecutionRequest = new DataTypes.ScriptExecution {
                           OutputObjectsFormat = OutputObjectsFormat.Json,
                           Name = "powerclivclogin",
                           Script = PCLIScriptsReader.ConnectByStringSamlToken,
                           ScriptParameters = new DataTypes.ScriptParameter[] {
                              new DataTypes.ScriptParameter {
                                 Name = "server",
                                 Value = vcEndpoint
                              },
                              new DataTypes.ScriptParameter {
                                 Name = "samlToken",
                                 Value = bearerSamlToken
                              },
                              new DataTypes.ScriptParameter {
                                 Name = "allLinked",
                                 Value = true
                              }
                           }
                        };

                        _logger.LogDebug($"Start Connect VC script");
                        var scriptResult = ScriptExecutionMediatorSingleton.
                           Instance.
                           ScriptExecutionMediator.
                           StartScriptExecution(sessionToken.UserName, result, scriptExecutionRequest).Result;
                        result.VcConnectionScriptId = scriptResult.Id;

                        _logger.LogDebug($"Wait Connect VC script to complete");
                        while (scriptResult.State == ScriptState.Running) {
                           var intermediateResult = ScriptExecutionMediatorSingleton.Instance.ScriptExecutionMediator.GetScriptExecution(
                              sessionToken.UserName,
                              scriptResult.Id);
                           if (intermediateResult != null) {
                              scriptResult = intermediateResult;
                           }
                           Thread.Sleep(200);
                        }
                     } catch (RunspaceEndpointException runspaceEndointException) {
                        _logger.LogError(runspaceEndointException, "Runspace endpoint exception while waiting connect VC script");
                        result.ErrorDetails = new DataTypes.ErrorDetails(runspaceEndointException);
                        result.State = DataTypes.RunspaceState.Error;
                     } catch (Exception exc) { 
                        _logger.LogError(exc, "Wait Connect VC script failed");
                        result.ErrorDetails = new DataTypes.ErrorDetails(exc);
                        result.State = DataTypes.RunspaceState.Error;
                     }
                  }

                  if (result.State != DataTypes.RunspaceState.Error) {
                     result.State = DataTypes.RunspaceState.Ready;
                  }
               }               
            });            
         } catch (RunspaceProviderException runspaceProviderException) {
            _logger.LogError(runspaceProviderException, "Runspace provider exception was thrown");
            throw;
         }
         catch (Exception ex) {
            throw new RunspaceProviderException(
               string.Format(
                  APIGatewayResources.MultiTenantRunspaceProvider_CreateFailed,
                  userId, 
                  ex.Message), 
               ex);
         }

         return result;
      }

      public IRunspaceData Get(string userId, string runspaceId) {
         _logger.LogInformation($"Get runspace with id: {runspaceId}");
         IRunspaceData result = null;
         try {
            Sessions.Instance.EnsureValidUser(userId);

            if (!_userRunspaces.Contains(userId)) {
               throw new RunspaceProviderException(
                  string.Format(APIGatewayResources.MultiTenantRunspaceProvider_UserHasNoRunspaces, userId));
            }

            if (!_userRunspaces.Contains(userId, runspaceId)) {
               throw new RunspaceProviderException(
                  string.Format(APIGatewayResources.MultiTenantRunspaceProvider_UserHasNoRunspaceWithId, userId, runspaceId));
            }

            var runspaceInfo = _runspaceProvider.Get(runspaceId);
            var runspaceData = _userRunspaces.GetData(userId, runspaceId);
            if (runspaceInfo == null && runspaceData != null) {               
               _userRunspaces.RemoveData(userId, runspaceId);
            } else {
               result = runspaceData;
            }
         } catch (Exception ex) {
            throw new RunspaceProviderException(
               string.Format(
                  APIGatewayResources.MultiTenantRunspaceProvider_GetFailed,
                  userId,
                  ex.Message),
               ex);
         }

         return result;
      }

      public void Kill(string userId, string runspaceId) {
         _logger.LogInformation($"Kill runspace {runspaceId}");
         try {
            Sessions.Instance.EnsureValidUser(userId);

            if (!_userRunspaces.Contains(userId)) {
               throw new RunspaceProviderException(
                  string.Format(
                     APIGatewayResources.MultiTenantRunspaceProvider_UserHasNoRunspaces, 
                     userId));
            }

            if (!_userRunspaces.Contains(userId, runspaceId)) {
               throw new RunspaceProviderException(
                  string.Format(
                     APIGatewayResources.MultiTenantRunspaceProvider_UserHasNoRunspaceWithId, 
                     userId, 
                     runspaceId));
            }

            _userRunspaces.RemoveData(userId, runspaceId);
            _runspaceProvider.Kill(runspaceId);
            _runspacesStatsMonitor.Unregister(runspaceId);
            if (_userRunspaces.List(userId) == null) {
               _userRunspaces.RemoveUser(userId);
            }
         } catch (Exception ex) {
            throw new RunspaceProviderException(
               string.Format(
                  APIGatewayResources.MultiTenantRunspaceProvider_KillFailed,
                  userId,
                  ex.Message),
               ex);
         }
      }

      public void Cleanup() {
         _logger.LogInformation($"Cleanup");
         // NB: The below operation could be slow because interacts with
         // every running container to get stats.
         var runspaceIdsToRemove = _runspacesStatsMonitor.EvaluateRunspacesToRemove();

         foreach (var runspaceId in runspaceIdsToRemove) {
            var userId = _userRunspaces.GetUser(runspaceId);

            // Kill Runspace Container
            Kill(userId, runspaceId);

            // Unregister from stats
            _runspacesStatsMonitor.Unregister(runspaceId);
         }

         // Cleanup Local Data for containers that are not running
         // Get all running containers and if there are such for which
         // local data exists but they are not available, remove local data
         var runningRunspaces = _runspaceProvider.List().Select(a => a.Id);

         // Clean up user to runspace data
         var userIds = _userRunspaces.ListUsers();
         foreach (var userId in userIds ?? Enumerable.Empty<string>()) {
            var userRunspaces = _userRunspaces.List(userId);
            foreach (var runspaceData in userRunspaces ?? Enumerable.Empty<IRunspaceData>()) {
               if (!runningRunspaces.Contains(runspaceData.Id)) {
                  _userRunspaces.RemoveData(userId, runspaceData.Id);
               }
            }
         }

         // Clean up statistics data
         var monitoredRunspaces = _runspacesStatsMonitor.GetRegisteredRunspaces();
         foreach (var runspaceId in monitoredRunspaces ?? Enumerable.Empty<string>()) {
            if (!runningRunspaces.Contains(runspaceId)) {
               _runspacesStatsMonitor.Unregister(runspaceId);
            }
         }
      }

      public IEnumerable<IRunspaceData> List(string userId) {
         _logger.LogInformation($"List");
         var result = new List<IRunspaceData>();
         try {
            Sessions.Instance.EnsureValidUser(userId);

            if (_userRunspaces.Contains(userId)) {
               var runspaces = _userRunspaces.List(userId);

               if (runspaces != null) {
                  foreach (var runspaceData in runspaces) {
                     var runspaceInfo = _runspaceProvider.Get(runspaceData.Id);
                     if  (runspaceInfo != null) {
                        result.Add(runspaceData);
                     } else {
                        _userRunspaces.RemoveData(userId, runspaceData.Id);
                     }
                  }
               }
            }
         } catch (Exception ex) {
            throw new RunspaceProviderException(
               string.Format(
                  APIGatewayResources.MultiTenantRunspaceProvider_ListFailed,
                  userId,
                  ex.Message),
               ex);
         }

         return result;
      }

      public void Dispose() {
         _runspacesCleanupTimer?.Dispose();
      }

      public void UpdateConfiguration(RunspaceProviderSettings runspaceProviderSettings) {
         _logger.LogInformation($"UpdateConfiguration");
         if (runspaceProviderSettings != null) {
            if (_runspaceProvider is K8sRunspaceProvider.K8sRunspaceProvider) {
               var newSettings = new K8sRunspaceProviderSettings {
                  ImagePullSecret = runspaceProviderSettings.K8sImagePullSecret,
                  RunspaceImageName = runspaceProviderSettings.K8sRunspaceImageName,
                  RunspacePort = runspaceProviderSettings.K8sRunspacePort,
                  VerifyRunspaceApiIsAccessibleOnCreate = runspaceProviderSettings.K8sVerifyRunspaceApiIsAccessibleOnCreate
               };
               _runspaceProvider.UpdateConfiguration(newSettings);
            }

            if (_runspacesStatsMonitor is RunspacesStatsMonitor rsMonitor) {
               rsMonitor.UpdateConfiguration(
                  runspaceProviderSettings.MaxNumberOfRunspaces,
                  runspaceProviderSettings.MaxRunspaceIdleTimeMinutes,
                  runspaceProviderSettings.MaxRunspaceActiveTimeMinutes);
            }
         }         
      }
   }
}
