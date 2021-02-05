// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Net;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl {
   public class WebConsoleData : IWebConsoleData
   {
      object _threadSafeLock = new object();
      public WebConsoleData(IWebConsoleInfo webConsoleInfo) {
         Id = webConsoleInfo.Id;         
         CreationState = webConsoleInfo.CreationState;
         CreationError = webConsoleInfo.CreationError;
      }
      
      private WebConsoleState _state;
      public WebConsoleState State { 
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

      public string Id { get; set;  }

      public RunspaceCreationState CreationState { get; set; }

      public RunspaceProviderException CreationError { get; set; }

      public override bool Equals(object obj) {
         var result = false;

         if (obj is WebConsoleData target) {
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
