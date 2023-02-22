// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.ScriptRuntimeService.AdminEngine.VCRegistration.Exceptions {
   public class TrustedCertificateRetrievalException : Exception {
      public TrustedCertificateRetrievalException(string message) : base(message) {
      }

      public TrustedCertificateRetrievalException(string message, Exception innerException) : base(message, innerException) {
      }
   }
}
