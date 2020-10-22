// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;

namespace VMware.ScriptRuntimeService.Setup.Tests {
   public class SettingsEditorTests {
      [Test]
      public void AddsStsSettingsToFileWithoutStsSettings() {
         // Arrange
         var settings = new {
            RunspaceSettings = new {
               A = "A",
               B = "B"
            },
            StorageSettings = new {
               C = "C",
               D = "D"
            }

         };
         var settingsFileContent = JsonConvert.SerializeObject(settings, Formatting.Indented);
         var mockFileData = new MockFileData(settingsFileContent);
         var mockFilePath = "/app/service/appsettings.json";
         var mockFileSystem = new MockFileSystem();
         mockFileSystem.AddFile(mockFilePath, mockFileData);

         var settingsEditor = new SettingsEditor(mockFilePath, mockFileSystem);
         var expectedStsSettings = new StsSettings {
            Realm = "realm",
            SolutionUserSigningCertificatePath = "certificate",
            StsServiceEndpoint = "endpoint",
            SolutionServiceId = "ses-test-solution-id",
            SolutionOwnerId= "ses-owner-id"
         };

         // Act
         settingsEditor.AddStsSettings(expectedStsSettings);
         settingsEditor.Save();

         // Assert
         string json = mockFileSystem.File.ReadAllText(mockFilePath);
         dynamic actual = JsonConvert.DeserializeObject(json);
         var actualStsSettings = actual.StsSettings;
         Assert.AreEqual(expectedStsSettings.Realm, actualStsSettings.Realm.ToString());
         Assert.AreEqual(expectedStsSettings.SolutionUserSigningCertificatePath, actualStsSettings.SolutionUserSigningCertificatePath.ToString());
         Assert.AreEqual(expectedStsSettings.StsServiceEndpoint, actualStsSettings.StsServiceEndpoint.ToString());
         Assert.AreEqual(expectedStsSettings.SolutionServiceId, actualStsSettings.SolutionServiceId.ToString());
         Assert.AreEqual(expectedStsSettings.SolutionOwnerId, actualStsSettings.SolutionOwnerId.ToString());
      }

      [Test]
      public void EditsExistingStsSettings() {
         // Arrange
         var initialStsSettings = new StsSettings {
            Realm = "r",
            SolutionUserSigningCertificatePath = "c",
            StsServiceEndpoint = "e"
         };

         var expectedStsSettings = new StsSettings {
            Realm = "realm",
            SolutionUserSigningCertificatePath = "certificate",
            StsServiceEndpoint = "endpoint"
         };

         var settings = new {
            RunspaceSettings = new {
               A = "A",
               B = "B"
            },
            StorageSettings = new {
               C = "C",
               D = "D"
            },
            StsSettings = initialStsSettings

         };
         var settingsFileContent = JsonConvert.SerializeObject(settings, Formatting.Indented);
         var mockFileData = new MockFileData(settingsFileContent);
         var mockFilePath = "/app/service/appsettings.json";
         var mockFileSystem = new MockFileSystem();
         mockFileSystem.AddFile(mockFilePath, mockFileData);

         var settingsEditor = new SettingsEditor(mockFilePath, mockFileSystem);
        

         // Act
         settingsEditor.AddStsSettings(expectedStsSettings);
         settingsEditor.Save();

         // Assert
         string json = mockFileSystem.File.ReadAllText(mockFilePath);
         dynamic actual = JsonConvert.DeserializeObject(json);
         var actualStsSettings = actual.StsSettings;
         Assert.AreEqual(expectedStsSettings.Realm, actualStsSettings.Realm.ToString());
         Assert.AreEqual(expectedStsSettings.SolutionUserSigningCertificatePath, actualStsSettings.SolutionUserSigningCertificatePath.ToString());
         Assert.AreEqual(expectedStsSettings.StsServiceEndpoint, actualStsSettings.StsServiceEndpoint.ToString());
      }


      [Test]
      public void AddsStsSettingsToEmptyJson() {
         // Arrange
         var mockFilePath = "/app/service/sts-settings.json";
         var mockFileSystem = new MockFileSystem();

         var settingsEditor = new SettingsEditor(mockFilePath, mockFileSystem);
         var expectedStsSettings = new StsSettings {
            Realm = "realm",
            SolutionUserSigningCertificatePath = "certificate",
            StsServiceEndpoint = "endpoint",
            SolutionServiceId = "ses-test-solution-id",
            SolutionOwnerId = "ses-owner-id"
         };

         // Act
         settingsEditor.AddStsSettings(expectedStsSettings);
         settingsEditor.Save();

         // Assert
         string json = mockFileSystem.File.ReadAllText(mockFilePath);
         dynamic actual = JsonConvert.DeserializeObject(json);
         var actualStsSettings = actual.StsSettings;
         Assert.AreEqual(expectedStsSettings.Realm, actualStsSettings.Realm.ToString());
         Assert.AreEqual(expectedStsSettings.SolutionUserSigningCertificatePath, actualStsSettings.SolutionUserSigningCertificatePath.ToString());
         Assert.AreEqual(expectedStsSettings.StsServiceEndpoint, actualStsSettings.StsServiceEndpoint.ToString());
         Assert.AreEqual(expectedStsSettings.SolutionServiceId, actualStsSettings.SolutionServiceId.ToString());
         Assert.AreEqual(expectedStsSettings.SolutionOwnerId, actualStsSettings.SolutionOwnerId.ToString());
      }

      [Test]
      public void GetStsSettingsObjectFromJsonContent() {
         // Arrange
         var expectedRealm = "6E712619A5B1948FE52EB89A84923DAFF8B15CD0";
         var expectedStsServiceEndpoint = "https://10.23.80.118/sts/STSService/vsphere.local";
         var expectedSolutionServiceId = "61ea54dc-2e6f-4c0c-a9b1-7ed01395670d";
         var expectedSolutionOwnerId = "ses-SolutionOwner-76ab7140-eb18-44ea-8427-ad3d52510a38";
         var expectedSolutionUserSigningCertificatePath = "/app/service/settings/certs/ses-sign.p12";
         var sb = new StringBuilder();
         var settingsJson = sb.AppendLine("{").
           AppendLine("  'StsSettings': {").
           AppendLine($"    'Realm': '{expectedRealm}',").
           AppendLine($"    'StsServiceEndpoint': '{expectedStsServiceEndpoint}',").
           AppendLine($"    'SolutionServiceId': '{expectedSolutionServiceId}',").
           AppendLine($"    'SolutionOwnerId': '{expectedSolutionOwnerId}',").
           AppendLine($"    'SolutionUserSigningCertificatePath': '{expectedSolutionUserSigningCertificatePath}'").
           AppendLine("  }").
           AppendLine("}").
           ToString();

         var settingsEditor = new SettingsEditor(settingsJson);
         // Act
         var actual = settingsEditor.GetStsSettings();

         // Assert
         Assert.NotNull(actual);
         Assert.AreEqual(expectedRealm, actual.Realm);
         Assert.AreEqual(expectedSolutionOwnerId, actual.SolutionOwnerId);
         Assert.AreEqual(expectedSolutionServiceId, actual.SolutionServiceId);
         Assert.AreEqual(expectedSolutionUserSigningCertificatePath, actual.SolutionUserSigningCertificatePath);
         Assert.AreEqual(expectedStsServiceEndpoint, actual.StsServiceEndpoint);
      }

      [Test]
      public void GetPartialStsSettingsObjectFromJsonContent() {
         // Arrange
         var expectedSolutionServiceId = "61ea54dc-2e6f-4c0c-a9b1-7ed01395670d";
         var expectedSolutionOwnerId = "ses-SolutionOwner-76ab7140-eb18-44ea-8427-ad3d52510a38";         
         var sb = new StringBuilder();
         var settingsJson = sb.AppendLine("{").
           AppendLine("  'StsSettings': {").
           AppendLine($"    'SolutionServiceId': '{expectedSolutionServiceId}',").
           AppendLine($"    'SolutionOwnerId': '{expectedSolutionOwnerId}'").
           AppendLine("  }").
           AppendLine("}").
           ToString();

         var settingsEditor = new SettingsEditor(settingsJson);
         // Act
         var actual = settingsEditor.GetStsSettings();

         // Assert
         Assert.NotNull(actual);         
         Assert.AreEqual(expectedSolutionOwnerId, actual.SolutionOwnerId);
         Assert.AreEqual(expectedSolutionServiceId, actual.SolutionServiceId);
      }
   }
}
