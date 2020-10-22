// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace {
   /// <summary>
   /// Represents Excetions thrown by communication with the Runspace Endpoint
   /// </summary>
   public class RunspaceException : Exception {
      public RunspaceException(string message)
         : base(message) {

      }

      public RunspaceException(string message, Exception innerException)
         : base(message, innerException) {

      }
   }
}
