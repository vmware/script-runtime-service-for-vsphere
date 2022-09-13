// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Abstractions;
using Newtonsoft.Json.Linq;

namespace VMware.ScriptRuntimeService.AdminEngine {
   public class SettingsEditor {
      private IFileSystem _fileSystem;
      private string _settingsFilePath;
      private dynamic _settingsJson;

      public SettingsEditor(string settingsFilePath, IFileSystem fileSystem) {
         _settingsFilePath = settingsFilePath;
         _fileSystem = fileSystem;

         if (fileSystem.File.Exists(settingsFilePath)) {
            string json = _fileSystem.File.ReadAllText(_settingsFilePath);
            _settingsJson = JsonConvert.DeserializeObject(json);
         } else {
            _settingsJson = JsonConvert.DeserializeObject("{}");
         }
      }

      public SettingsEditor() : this(null) { }

      public SettingsEditor(string settingsJsonContent) {
         if (!string.IsNullOrEmpty(settingsJsonContent)) {
            _settingsJson = JsonConvert.DeserializeObject(settingsJsonContent);
         } else {
            _settingsJson = JsonConvert.DeserializeObject("{}");
         }
         _fileSystem = new FileSystem();
      }

      public void AddStsSettings(StsSettings stsSettings) {
         _settingsJson.StsSettings = JToken.FromObject(stsSettings);
      }

      public void AddSettings(object settings) {
         _settingsJson = JToken.FromObject(settings);
      }

      public StsSettings GetStsSettings() {
         return new StsSettings {
            VCenterAddress = _settingsJson?["StsSettings"]?["VCenterAddress"]?.ToString(),
            SolutionOwnerId = _settingsJson?["StsSettings"]?["SolutionOwnerId"]?.ToString(),
            SolutionServiceId = _settingsJson?["StsSettings"]?["SolutionServiceId"]?.ToString(),
            Realm = _settingsJson?["StsSettings"]?["Realm"]?.ToString(),
            StsServiceEndpoint = _settingsJson?["StsSettings"]?["StsServiceEndpoint"]?.ToString(),
            SolutionUserSigningCertificatePath = _settingsJson?["StsSettings"]?["SolutionUserSigningCertificatePath"]?.ToString(),
         };
      }

      public string GetSettingsJsonContent() {
         return JsonConvert.SerializeObject(_settingsJson, Formatting.Indented);
      }

      public void Save() {
         if (!_fileSystem.File.Exists(_settingsFilePath)) {
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(_settingsFilePath));
            _fileSystem.File.Create(_settingsFilePath);
         }
         string output = JsonConvert.SerializeObject(_settingsJson, Formatting.Indented);
         _fileSystem.File.WriteAllText(_settingsFilePath, output);
      }

   }
}
