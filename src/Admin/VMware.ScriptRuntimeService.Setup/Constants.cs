// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMware.ScriptRuntimeService.Setup
{
   public static class Constants {
      public const string SignCertificateSecretName = "srs-sign";
      public const string TlsCertificateSecretName = "srs-tls";
      public static string StsSettings_SolutionUserSigningCertificatePath = Path.Combine(
                     "/app",
                     "service",
                     "settings",
                     "certs",
                     $"{SignCertificateSecretName}.p12");
      public const string TrustedCACertificatesConfigMapName = "trusted-ca-certs";
      public const string StsSettingsConfigMapName = "sts-settings";
      public const string StsSettingsConfigMapDataKey = "sts-settings.json";
   }
}
