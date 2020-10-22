// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Properties;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication {
   public class InvalidSessionException : Exception {
      private InvalidSessionException(string sessionId, string invalidSessionMessage) :
         base(string.Format(invalidSessionMessage, sessionId)){

      }

      public static InvalidSessionException SessionHasExpired(string sessionId) {
         return new InvalidSessionException(sessionId, APIGatewayResources.InvalidSessionException_SessionExpired);
      }

      public static InvalidSessionException SessionDoesntExist(string sessionId) {
         return new InvalidSessionException(sessionId, APIGatewayResources.InvalidSessionException_SessionDoesntExist);
      }
   }
}
