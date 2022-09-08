// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.AdminEngine.VCRegistration {
   [Serializable]
   public class SrsNotRegisteredException : Exception {
      public SrsNotRegisteredException() {
      }

      public SrsNotRegisteredException(string message) : base(message) {
      }

      public SrsNotRegisteredException(string message, Exception innerException) : base(message, innerException) {
      }

      protected SrsNotRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context) {
      }
   }
}
