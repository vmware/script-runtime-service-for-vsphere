// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VMware.ScriptRuntimeService.Ls
{
   public class VCEndpoint {
      public string Hostname { get; set; }
      public X509Certificate2[] SslTrustCertificates { get; set; } 
   }
}
