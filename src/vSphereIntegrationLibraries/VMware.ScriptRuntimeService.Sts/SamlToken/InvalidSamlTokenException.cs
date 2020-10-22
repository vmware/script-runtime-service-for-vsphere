// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.Sts.SamlToken {
   public class InvalidSamlTokenException : Exception {
      public InvalidSamlTokenException(string message)
         : base(message) { }

      public InvalidSamlTokenException(string message, Exception exception)
         : base(message, exception) { }
   }
}
