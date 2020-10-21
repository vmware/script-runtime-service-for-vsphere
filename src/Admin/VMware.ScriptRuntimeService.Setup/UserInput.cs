// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using LookupServiceReference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using VMware.ScriptRuntimeService.Setup.SetupFlows;

namespace VMware.ScriptRuntimeService.Setup {
   public class UserInput {
      public UserInput() {
         InitializeFromEnvironment();
      }

      private void InitializeFromEnvironment() {
         var flowTypeValue = Environment.GetEnvironmentVariable("RUN");
         if (Enum.TryParse<SetupFlowType>(flowTypeValue, out var flowType)) {
            Run = flowType;
         } else {
            // Default flow is 'RegisterWithVC'
            Run = SetupFlowType.RegisterWithVC;
         }
         StsSettingsPath = Environment.GetEnvironmentVariable("SERVICE_STS_SETTINGS_PATH");
         TlsCertificatePath = Environment.GetEnvironmentVariable("SERVICE_TLS_CERTIFICATE_PATH");
         SigningCertificatePath = Environment.GetEnvironmentVariable("SERVICE_SIGNING_CERTIFICATE_PATH");
         ServiceHostname = Environment.GetEnvironmentVariable("SERVICE_HOSTNAME");
         Psc = Environment.GetEnvironmentVariable("TARGET_VC_SERVER");
         User = Environment.GetEnvironmentVariable("TARGET_VC_USER");
         var password = Environment.GetEnvironmentVariable("TARGET_VC_PASSWORD");
         if (!string.IsNullOrEmpty(password)) {
            var securePassword = new SecureString();
            foreach (char c in password) {
               securePassword.AppendChar(c);
            }
            Password = securePassword;
         }         
         if (bool.TryParse(Environment.GetEnvironmentVariable("TARGET_VC_INSECURE"), out var forceSpecified)) {
            ForceSpecified = forceSpecified;
         }
         VcThumbprint = Environment.GetEnvironmentVariable("TARGET_VC_THUMBPRINT");
      }

     internal void SetPassword(string password) {
         if (!string.IsNullOrEmpty(password)) {
            var securePassword = new SecureString();
            foreach (char c in password) {
               securePassword.AppendChar(c);
            }
            Password = securePassword;
         }
      }

      public SetupFlowType Run { get; set; }
      public string StsSettingsPath { get; set; }
      public string TlsCertificatePath { get; set; }
      public string SigningCertificatePath { get; set; }
      public string ServiceHostname{ get; set; }
      public string Psc { get; set; }
      public string User { get; set; }
      public SecureString Password { get; set; }
      public string ConfigDir { get; set; }
      public string K8sSettings { get; set; }
      public bool ForceSpecified { get; set; }
      public string VcThumbprint { get; set; }

      /// <summary>
      /// Verifies user input settings are valid for the specified Setup Flow Type.
      /// If user input is not valid exception with invalid properties is thrown
      /// </summary>
      /// <param name="setupFlowType"></param>      
      public void EnsureIsValid(SetupFlowType setupFlowType) {         
         if (string.IsNullOrEmpty(Psc)) {
            throw new InvalidUserInputException("VC IP/FQDN is not specified");
         }
         if (string.IsNullOrEmpty(User)) {
            throw new InvalidUserInputException("VC Username is not specified");
         }
         if (Password == null) {
            throw new InvalidUserInputException("VC Password is not specified");
         }

         if (setupFlowType == SetupFlowType.UnregisterFromVC || 
             setupFlowType == SetupFlowType.UpdateTlsCertificate) {
            if (string.IsNullOrEmpty(StsSettingsPath)) {
               throw new InvalidUserInputException("StsSettings file path is not specified");
            }

            if (!File.Exists(StsSettingsPath)) {
               throw new InvalidUserInputException($"StsSettings file not found '{StsSettingsPath}'");
            }
         }

         if (setupFlowType == SetupFlowType.UpdateTlsCertificate) {
            if (string.IsNullOrEmpty(TlsCertificatePath)) {
               throw new InvalidUserInputException("TlsCertificatePath file path is not specified");
            }

            if (!File.Exists(TlsCertificatePath)) {
               throw new InvalidUserInputException($"StsSettings file not found '{TlsCertificatePath}'");
            }
         }
      }
   }
}
