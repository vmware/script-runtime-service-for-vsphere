// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Net;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl {
   public class RunspaceData : IRunspaceData
   {
      object _threadSafeLock = new object();
      public RunspaceData(IRunspaceInfo runspaceInfo) {
         Id = runspaceInfo.Id;
         Endpoint = runspaceInfo.Endpoint;
         CreationState = runspaceInfo.CreationState;
         CreationError = runspaceInfo.CreationError;
      }
      public string Name { get; set; }

      private RunspaceState _state;
      public RunspaceState State { 
         get {
            lock (_threadSafeLock) {
               return _state;
            }
         }
         set {
            lock (_threadSafeLock) {
               _state = value;
            }
         }
      }
      
      public bool RunVcConnectionScript { get; set; }

      private string _vcConnectionScriptId;
      public string VcConnectionScriptId { 
         get {
            lock (_threadSafeLock) {
               return _vcConnectionScriptId;
            }
         }
         set {
            lock (_threadSafeLock) {
               _vcConnectionScriptId = value;
            }
         }
      }

      private ErrorDetails _errorDetails;
      public ErrorDetails ErrorDetails {
         get {
            lock (_threadSafeLock) {
               return _errorDetails;
            }
         }
         set {
            lock (_threadSafeLock) {
               _errorDetails = value;
            }
         }
      }

      public DateTime CreationTime { get; set; }

      public IPEndPoint Endpoint { get; set; }

      public string Id { get; set;  }

      public RunspaceCreationState CreationState { get; set; }

      public RunspaceProviderException CreationError { get; set; }

      public override bool Equals(object obj) {
         var result = false;

         if (obj is RunspaceData target) {
            result = Id.Equals(target.Id);
         }

         return result;
      }

      public override int GetHashCode() {
         return Id.GetHashCode();
      }

      public override string ToString() {
         return Id;
      }
   }
}
