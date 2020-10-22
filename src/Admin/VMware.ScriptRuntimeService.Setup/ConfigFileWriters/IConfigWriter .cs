// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VMware.ScriptRuntimeService.Setup.ConfigFileWriters {
   public interface IConfigWriter {
      void WriteTlsCertificate(string name, string crtFilePath, string keyFilePath);
      void WriteBinaryFile(string name, string filePath);
      void WriteTrustedCACertificates(IEnumerable<string> encodedCertificates);
      void WriteSetupSettings(SetupServiceSettings setupServiceSettings);
      void WriteServiceStsSettings(StsSettings stsSettings);
   }
}
