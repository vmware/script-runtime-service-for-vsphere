﻿// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.RetentionPolicy;
using VMware.ScriptRuntimeService.APIGateway.Runspace.Impl.Statistics;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl
{
   public class RunspacesStatsMonitor : IRunspacesStatsMonitor {
      List<IRunspaceStats> _runspaceStats = new List<IRunspaceStats>();
      private RunspaceRetentionPolicy _retentionPolicy;
      private int _maxNumberOfRunspaces;
      private IRunspaceStatsFactory _runspaceStatsFactory;
      private IRemoveExpiredIdleRunspaceRuleFactory _removeExpiredIdleRuleFactory;
      private IRemoveExpiredActiveRunspaceRuleFactory _removeExpiredActiveRuleFactory;
      private int _lastConfiguredMaxRunspaceIdleTimeMinutes;
      private int _lastConfiguredMaxRunspaceActiveTimeMinutes;

      public RunspacesStatsMonitor(
         int maxNumberOfRunspaces,
         int maxRunspaceIdleTimeMinutes,
         int maxRunspaceActiveTimeMinutes) : 
         this(maxNumberOfRunspaces, 
            maxRunspaceIdleTimeMinutes, 
            maxRunspaceActiveTimeMinutes,
            new RunspaceStatsFactory(),
            new RemoveExpiredIdleRunspaceRuleFactory(),
            new RemoveExpiredActiveRunspaceRuleFactory(),
            null) {
      }

      public RunspacesStatsMonitor(
         int maxNumberOfRunspaces,
         int maxRunspaceIdleTimeMinutes,
         int maxRunspaceActiveTimeMinutes,
         IRunspaceStatsFactory runspaceStatsFactory,
         IRemoveExpiredIdleRunspaceRuleFactory removeExpiredIdleRuleFactory,
         IRemoveExpiredActiveRunspaceRuleFactory removeExpiredActiveRuleFactory,
         RunspaceRetentionPolicy runspaceRetentionPolicy) {

         if (removeExpiredIdleRuleFactory == null) throw new ArgumentNullException(nameof(removeExpiredIdleRuleFactory));
         if (removeExpiredActiveRuleFactory == null) throw new ArgumentNullException(nameof(removeExpiredActiveRuleFactory));

         _maxNumberOfRunspaces = maxNumberOfRunspaces;

         _removeExpiredIdleRuleFactory = removeExpiredIdleRuleFactory;
         _removeExpiredActiveRuleFactory = removeExpiredActiveRuleFactory;

         if (runspaceRetentionPolicy == null) {
            // Create retention policy with rules

            _lastConfiguredMaxRunspaceIdleTimeMinutes = maxRunspaceIdleTimeMinutes;
            _lastConfiguredMaxRunspaceActiveTimeMinutes = maxRunspaceActiveTimeMinutes;

            _retentionPolicy = new RunspaceRetentionPolicy(
               new IRunspaceRetentionRule[] {
                  new KeepActiveRunspaceOnInactiveSessionRule(),
                  _removeExpiredIdleRuleFactory.Create(maxRunspaceIdleTimeMinutes),
                  _removeExpiredActiveRuleFactory.Create(maxRunspaceActiveTimeMinutes),
               });
         } else {
            _retentionPolicy = runspaceRetentionPolicy;
         }

         _runspaceStatsFactory = runspaceStatsFactory;
      }

      public bool IsCreateNewRunspaceAllowed() {
         bool result = false;

         lock (this) {
            result = _runspaceStats.Count < _maxNumberOfRunspaces;
         }

         return result;
      }

      public void Register(IRunspaceInfo runspaceInfo, string sessionId) {
         lock (this) {
            _runspaceStats.Add(
               _runspaceStatsFactory.Create(
                  runspaceInfo.Id, 
                  new RunspaceSessionInfoProvider(sessionId), 
                  new ActiveIdleInfoProvider(runspaceInfo)));
         }
      }

      public void Unregister(string runspaceId) {
         lock (this) {
            var runspaceStats = _runspaceStats.Find(r => r.RunspaceId == runspaceId);
            if (runspaceStats != null) {
               _runspaceStats.Remove(runspaceStats);
            }
         }
      }

      public string[] GetRegisteredRunspaces() {
         string[] result = null;
         lock (this) {

            result = _runspaceStats.ToArray().Select(a => a.RunspaceId).ToArray();
         }

         return result;
      }

      public string[] EvaluateRunspacesToRemove() {
         IRunspaceStats[] runspacesToEvaluate = new IRunspaceStats[]{};
         lock (this) {
            runspacesToEvaluate = _runspaceStats.ToArray();
         }

         List<string> result = new List<string>();

         foreach (var runspaceStats in runspacesToEvaluate) {
            runspaceStats.Refresh();
            if (_retentionPolicy.ShouldRemove(runspaceStats)) {
               result.Add(runspaceStats.RunspaceId);
            }
         }

         return result.ToArray();
      }

      public void UpdateConfiguration(
         int maxNumberOfRunspaces,
         int maxRunspaceIdleTimeMinutes,
         int maxRunspaceActiveTimeMinutes) {

         _maxNumberOfRunspaces = maxNumberOfRunspaces;

         if (ShouldUpdateRetentionPolicy(maxRunspaceIdleTimeMinutes, maxRunspaceActiveTimeMinutes)) {
            // Create new retention policy with updated settings
            _lastConfiguredMaxRunspaceIdleTimeMinutes = maxRunspaceIdleTimeMinutes;
            _lastConfiguredMaxRunspaceActiveTimeMinutes = maxRunspaceActiveTimeMinutes;

            _retentionPolicy = new RunspaceRetentionPolicy(
               new IRunspaceRetentionRule[] {
               new KeepActiveRunspaceOnInactiveSessionRule(),
               _removeExpiredIdleRuleFactory.Create(maxRunspaceIdleTimeMinutes),
               _removeExpiredActiveRuleFactory.Create(maxRunspaceActiveTimeMinutes),
               });
         }
      }

      private bool ShouldUpdateRetentionPolicy(int maxRunspaceIdleTimeMinutes, int maxRunspaceActiveTimeMinute) {
         return 
            _lastConfiguredMaxRunspaceIdleTimeMinutes != maxRunspaceIdleTimeMinutes ||
            _lastConfiguredMaxRunspaceActiveTimeMinutes != maxRunspaceActiveTimeMinute;       
      }
   }
}
