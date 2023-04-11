// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Net;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl {
   public class WebConsoleData : IWebConsoleData {
      private readonly object _threadSafeLock = new object();

      private WebConsoleState _state;
      private ErrorDetails _errorDetails;
      private DateTime _creationTime;
      private string _id;
      private RunspaceCreationState _creationState;
      private RunspaceProviderException _creationError;
      private IPEndPoint _endpoint;

      public WebConsoleData(IWebConsoleInfo webConsoleInfo) {
         Id = webConsoleInfo.Id;
         CreationState = webConsoleInfo.CreationState;
         CreationError = webConsoleInfo.CreationError;
      }

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



      public DateTime CreationTime {
         get {
            lock (_threadSafeLock) {
               return _creationTime;
            }
         }
         set {
            lock (_threadSafeLock) {
               _creationTime = value;
            }
         }
      }

      public string Id {
         get {
            lock (_threadSafeLock) {
               return _id;
            }
         }
         set {
            lock (_threadSafeLock) {
               _id = value;
            }
         }
      }

      public RunspaceCreationState CreationState {
         get {
            lock (_threadSafeLock) {
               return _creationState;
            }
         }
         set {
            lock (_threadSafeLock) {
               _creationState = value;
            }
         }
      }

      public RunspaceProviderException CreationError {
         get {
            lock (_threadSafeLock) {
               return _creationError;
            }
         }
         set {
            lock (_threadSafeLock) {
               _creationError = value;
            }
         }
      }

      public IPEndPoint Endpoint {
         get {
            lock (_threadSafeLock) {
               return _endpoint;
            }
         }
         set {
            lock (_threadSafeLock) {
               _endpoint = value;
            }
         }
      }

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
