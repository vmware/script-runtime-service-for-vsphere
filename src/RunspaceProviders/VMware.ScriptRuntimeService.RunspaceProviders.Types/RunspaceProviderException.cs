// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.ScriptRuntimeService.RunspaceProviders.Types
{
   public class RunspaceProviderException : Exception
   {
      public RunspaceProviderException(string message)
         : base(message) {

      }

      public RunspaceProviderException(string message, Exception innerException)
         : base(message, innerException) {

      }
   }
}
