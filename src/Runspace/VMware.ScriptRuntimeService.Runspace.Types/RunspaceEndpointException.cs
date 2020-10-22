// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.ScriptRuntimeService.Runspace.Types {
   /// <summary>
   /// Represents exceptions thrown by the Runspace Endpoint
   /// </summary>
   public class RunspaceEndpointException : Exception {
      private const int ERROR_CODE_NOT_SPECIFIED = 500;

      public RunspaceEndpointException(
         int httpErrorCode, 
         int errorCode, 
         string message) 
         : this(httpErrorCode, errorCode, message, null) {
      }

      public RunspaceEndpointException(
         int httpErrorCode, 
         int errorCode, 
         string message, 
         Exception innerException) 
         : this(errorCode, message, innerException) {
         HttpErrorCode = httpErrorCode;
      }

      public RunspaceEndpointException(
         int errorCode, 
         string message, 
         Exception innerException) 
         : this(message, innerException) {
         ErrorCode = errorCode;
      }

      public RunspaceEndpointException(
         int errorCode, 
         string message) 
         : this(errorCode, message, null) {
      }

      public RunspaceEndpointException(string message) : this(message, null) {
      }

      public RunspaceEndpointException(string message, Exception innerException)
         : base(message, innerException) {
         ErrorCode = ERROR_CODE_NOT_SPECIFIED;
         HttpErrorCode = 500; // Internal Server Error
      }

      public int ErrorCode { get; }
      public int HttpErrorCode { get; }
   }
}