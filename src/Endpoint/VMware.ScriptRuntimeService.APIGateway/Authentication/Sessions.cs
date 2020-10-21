// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Http;
using VMware.ScriptRuntimeService.APIGateway.Properties;

[assembly: InternalsVisibleTo("VMware.ScriptRuntimeService.APIGateway.Tests")]
namespace VMware.ScriptRuntimeService.APIGateway.Authentication {
   public class Sessions : IDisposable {
      private ConcurrentDictionary<string, DateTime> _sessionValidTo = 
         new ConcurrentDictionary<string, DateTime>();
      private ConcurrentDictionary<string, SessionToken> _activeSessions  =
         new ConcurrentDictionary<string, SessionToken>();
      private bool _disposed;
      private Timer _releaseIdleSessionsTimer;

      private static readonly Sessions _sessions = new Sessions();
      private Sessions() {
         // Default Idle Session Period is 30 minutes
         IdleSessionValidPeriodSeconds = 30 * 60;

         // Idle Sessions cleanup period time is 5 minutes
         _releaseIdleSessionsTimer = new Timer(5 * 60 * 1000);

         // Cleanup expired session handler
         _releaseIdleSessionsTimer.Elapsed += (sender, args) => {
            var now = DateTime.Now;
            var sessions = _sessionValidTo.Keys;
            foreach (var session in sessions) {
               if (_sessionValidTo.TryGetValue(session, out var sessionEndTime) &&
                   now > sessionEndTime) {
                  _sessionValidTo.Remove(session, out _);
                  _activeSessions.Remove(session, out _);
               }
            }
         };

         // Timer settings
         _releaseIdleSessionsTimer.AutoReset = true;
         _releaseIdleSessionsTimer.Enabled = true;
      }

      internal static Sessions Instance => _sessions;

      internal void EnsureValidUser(string userId) {
         if (string.IsNullOrEmpty(userId)) {
            throw new ArgumentNullException(nameof(userId));
         }
      }
      
      internal int IdleSessionValidPeriodSeconds { get; set; }

      internal void RegisterSession(SessionToken sessionToken) {
         _sessionValidTo[sessionToken.SessionId] = GetValidPeriodEndTime();
         _activeSessions[sessionToken.SessionId] = sessionToken;
      }

      internal void UnregisterSession(SessionToken sessionToken) {
         _sessionValidTo.TryRemove(sessionToken.SessionId, out _);
         _activeSessions.TryRemove(sessionToken.SessionId, out _);
      }

      internal SessionToken GetSessionToken(string sessionId) {
         SessionToken result = null;

         if (!string.IsNullOrEmpty(sessionId) && _activeSessions.TryGetValue(sessionId, out var resultValue)) {
            result = resultValue;
         }

         return result;
      }

      internal bool IsActive(string sessionId) {
         var result = false;

         if (!string.IsNullOrEmpty(sessionId)) {
            if (_sessionValidTo.TryGetValue(sessionId, out var sessionEndTime) &&
                DateTime.Now < sessionEndTime) {
               result = true;
            }
         }

         return result;
      }

      internal void ValidateAndUpdateSession(SessionToken sessionToken) {
         if (_sessionValidTo.TryGetValue(sessionToken.SessionId, out var validTo)) {
            if (validTo > DateTime.Now) {
               // Extend session lifetime
               _sessionValidTo[sessionToken.SessionId] = GetValidPeriodEndTime();
            } else {
               // Expired session
               throw InvalidSessionException.SessionHasExpired(sessionToken.SessionId);
            }
         } else {
            // Session doesn't exist
            throw InvalidSessionException.SessionDoesntExist(sessionToken.SessionId);
         }
      }

      private DateTime GetValidPeriodEndTime() {
         return DateTime.Now + new TimeSpan(0, 0, IdleSessionValidPeriodSeconds);
      }

      protected virtual void Dispose(bool disposing) {
         if (!_disposed) {
            if (disposing) {
               _releaseIdleSessionsTimer.Dispose();
            }

            _disposed = true;
         }
      }

      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }
   }
}
