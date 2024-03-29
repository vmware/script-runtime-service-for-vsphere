// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Collections.Generic;

namespace VMware.ScriptRuntimeService.AdminEngine.ConfigFileWriters {
   public interface IConfigWriter {
      void WriteTlsCertificate(string name, string crtFilePath, string keyFilePath);
      void WriteBinaryFile(string name, string filePath);
      void WriteTrustedCACertificates(IEnumerable<string> encodedCertificates);
      void DeleteTrustedCACertificates();
      void WriteSetupSettings(SetupServiceSettings setupServiceSettings);
      void WriteServiceStsSettings(StsSettings stsSettings);
      void WriteSettings(string settingsName, object settingsObject);
      void WriteSettings(string settingsName, object settingsObject, string settingsDataKeyName);
      void DeleteSettings(string settingsName);
   }
}
