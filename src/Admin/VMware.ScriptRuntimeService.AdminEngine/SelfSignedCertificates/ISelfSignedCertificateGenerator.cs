// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Security.Cryptography.X509Certificates;

namespace VMware.ScriptRuntimeService.AdminEngine.SelfSignedCertificates {
   /// <summary>
   /// Inteface with ability to generate self signed certificate
   /// </summary>
   public interface ISelfSignedCertificateGenerator {
      /// <summary>
      /// Generates self signed certificate
      /// </summary>      
      /// <returns><see cref="X509Certificate2"/> instance if certficate is generated successfully, otherwise null</returns>
      X509Certificate2 Generate(string certName);
   }
}
