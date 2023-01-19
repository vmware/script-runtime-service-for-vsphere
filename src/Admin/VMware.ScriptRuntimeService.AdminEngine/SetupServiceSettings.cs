// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;

namespace VMware.ScriptRuntimeService.AdminEngine {
   public class SetupServiceSettings {
      #region Nested Types
      public class SetupServiceSettingsMemento {
         public string HostName { get; set; }
         public int Port { get; set; }
         public string TlsCertificate { get; set; }
         public string SigningCertificate { get; set; }
      }
      #endregion

      #region Constructors
      public SetupServiceSettings() : this(Guid.NewGuid().ToString()) {
      }

      public SetupServiceSettings(string serviceId) {
         NodeId = Guid.NewGuid().ToString();
         OwnerId = "ScriptRuntimeService-SolutionOwner";
         ServiceDescriptionResourceKey = "srs.ServiceDescritpion";
         ServiceId = serviceId;
         ServiceNameResourceKey = "srs.ServiceName";
         ServiceVersion = "1.0";
         ServiceTypeProduct = "com.vmware.srs";
         ServiceTypeType = "srs";
         EndpointProtocol = "rest";
         EndpointType = "com.vmware.srs";
      }

      public static SetupServiceSettings NewService(
         X509Certificate2 tlsCertificate,
         X509Certificate2 signingCertificate,
         string hostName,
         int port) {
         var result = new SetupServiceSettings();
         result.NodeId = Guid.NewGuid().ToString();
         result.OwnerId = $"srs-SolutionOwner-{Guid.NewGuid()}";
         result.ServiceId = Guid.NewGuid().ToString();
         result.TlsCertificate = tlsCertificate;
         result.SigningCertificate = signingCertificate;
         var scheme = "https";
         result.EndpointUrl = $"{scheme}://{hostName}:{port}";
         return result;
      }

      public static SetupServiceSettings FromStsSettings(StsSettings stsSettings) {
         var result = new SetupServiceSettings();
         result.ServiceId = stsSettings.SolutionServiceId;
         result.OwnerId = stsSettings.SolutionOwnerId;
         return result;
      }

      public static SetupServiceSettings FromConfigDir(string serviceSettingsConfigDir) {
         var serviceSettingsFile = Path.Combine(serviceSettingsConfigDir, "SetupServiceSettings.json");
         var result = new SetupServiceSettings();

         if (!File.Exists(serviceSettingsFile)) {
            throw new FileNotFoundException(
               "ConfigDir can't be found",
               serviceSettingsFile);
         }

         var jsonContent = File.ReadAllText(serviceSettingsFile);
         var svcSettingsFromFile = JsonConvert.DeserializeObject<SetupServiceSettingsMemento>(jsonContent);

         var scheme = "https";
         result.EndpointUrl = $"{scheme}://{svcSettingsFromFile.HostName}:{svcSettingsFromFile.Port}";
         result.TlsCertificatePath = Path.Combine(serviceSettingsConfigDir, svcSettingsFromFile.TlsCertificate);
         result.SigningCertificatePath = Path.Combine(serviceSettingsConfigDir, svcSettingsFromFile.SigningCertificate);
         result.TlsCertificate = new X509Certificate2(result.TlsCertificatePath);
         result.SigningCertificate = new X509Certificate2(result.SigningCertificatePath);

         return result;
      }
      #endregion

      public string NodeId { get; private set; }
      public string OwnerId { get; private set; }
      public string ServiceDescriptionResourceKey { get; private set; }
      public string ServiceId { get; private set; }
      public string ServiceNameResourceKey { get; private set; }
      public string ServiceVersion { get; private set; }
      public string ServiceTypeProduct { get; private set; }
      public string ServiceTypeType { get; private set; }
      public string EndpointUrl { get; private set; }
      public string EndpointProtocol { get; private set; }
      public string EndpointType { get; private set; }
      public string TlsCertificatePath { get; set; }
      public string SigningCertificatePath { get; set; }
      public X509Certificate2 TlsCertificate { get; set; }
      public X509Certificate2 SigningCertificate { get; set; }

      public SetupServiceSettingsMemento Memento {
         get {
            var serviceUri = new Uri(EndpointUrl);
            return new SetupServiceSettingsMemento {
               HostName = serviceUri.Host,
               Port = serviceUri.Port,
               TlsCertificate = Path.GetFileName(TlsCertificatePath),
               SigningCertificate = Path.GetFileName(SigningCertificatePath)
            };
         }
      }

      public void Export(string outFilePath) {
         var serviceUri = new Uri(EndpointUrl);
         var data = new SetupServiceSettingsMemento {
            HostName = serviceUri.Host,
            Port = serviceUri.Port,
            TlsCertificate = Path.GetFileName(TlsCertificatePath),
            SigningCertificate = Path.GetFileName(SigningCertificatePath)
         };
         var rawJson = JsonConvert.SerializeObject(data, Formatting.Indented);
         File.WriteAllText(outFilePath, rawJson);
      }

      public SetupServiceSettings SetHostname(string hostname) {
         var serviceUri = new Uri(EndpointUrl);
         EndpointUrl = $"{serviceUri.Scheme}://{hostname}:{serviceUri.Port}";
         return this;
      }
   }
}
