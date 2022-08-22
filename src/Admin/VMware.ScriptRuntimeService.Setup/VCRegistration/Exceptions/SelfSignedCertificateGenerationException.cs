// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.Setup.VCRegistration.Exceptions {
   [Serializable]
   internal class SelfSignedCertificateGenerationException : Exception {
      public SelfSignedCertificateGenerationException() {
      }

      public SelfSignedCertificateGenerationException(string message) : base(message) {
      }

      public SelfSignedCertificateGenerationException(string message, Exception innerException) : base(message, innerException) {
      }

      protected SelfSignedCertificateGenerationException(SerializationInfo info, StreamingContext context) : base(info, context) {
      }
   }
}
