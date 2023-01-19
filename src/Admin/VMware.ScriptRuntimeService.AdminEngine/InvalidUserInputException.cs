// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.ScriptRuntimeService.AdminEngine {
   public class InvalidUserInputException : Exception {
      public InvalidUserInputException(string message) : base(message) {

      }
   }
}
