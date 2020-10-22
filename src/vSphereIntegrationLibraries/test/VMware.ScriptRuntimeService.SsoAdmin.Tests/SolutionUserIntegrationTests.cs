// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace VMware.ScriptRuntimeService.SsoAdmin.Tests {
   /// <summary>
   /// Contains integrations tests interacting with vSphere SSO Admin API
   /// To run those configure test vSphere on SERVER_ADDRESS with ADMIN_USER, ADMIN_PASSWORD constants
   /// and uncomment [Test] attributes
   /// </summary>
   public class Integration {
      private const string SERVER_ADDRESS = "10.23.81.78";
      private const string ADMIN_USER = "Administrator";
      private const string ADMIN_PASSWORD = "Admin!23";
      private const string DOMAIN_NAME = "vsphere.local";
      private SsoAdminClient _ssoAdminClient;

      string _userName = "sesSolutionUser-123";
      string _authorizationUsername = $"{ADMIN_USER}@{DOMAIN_NAME}";
      private SecureString _authorizationPassword;

      private static SecureString ConvertStringToSecureString(string str) {
         // There must be a better way to do it but I'm not wasting more time
         // with it right now
         SecureString secureString = new SecureString();
         foreach (char c in str) {
            secureString.AppendChar(c);
         }

         return secureString;
      }

      [SetUp]
      public void Setup() {
         var ssoAdminUri = new Uri($"https://{SERVER_ADDRESS}:443/sso-adminserver/sdk");
         var stsUri = new Uri($"https://{SERVER_ADDRESS}/sts/STSService/vsphere.local");
         _ssoAdminClient = new SsoAdminClient(
            ssoAdminUri, 
            stsUri,
            new AcceptAllX509CertificateValidator());
         _authorizationPassword = ConvertStringToSecureString(ADMIN_PASSWORD);
         Assert.NotNull(_ssoAdminClient);
      }

      [TearDown]
      public void TearDown() {
         try {
            _ssoAdminClient.DeleteLocalPrincipal(
               _authorizationUsername,
               _authorizationPassword,
               _userName);
         } finally { }
      }

      //[Test]
      public void TestCreateUpdateDeleteLocalSolutionUser() {
         // Arrange
         var userSigningCertificate = new X509Certificate2(
            @"C:\git-repos\SsoAdminClientLib\TestCertificate\solutionUserCertificate.pfx",
            "ca$hc0w");

         // Act
         /// Create Solution User
         string principal = _ssoAdminClient.
            CreateLocalSolutionUser(
               _authorizationUsername,
               _authorizationPassword,
               _userName, 
               userSigningCertificate, 
               "ScriptExecutionServer solution user");

         // Assert
         /// Check CreateSolutionUser result
         Assert.AreEqual($"{_userName}@{DOMAIN_NAME}", principal);

         // Act
         /// Update Solution User Description
         principal = _ssoAdminClient.
            UpdateLocalSolutionUser(
               _authorizationUsername,
               _authorizationPassword,
               _userName,
               userSigningCertificate,
               "Updated description of ScriptExecutionServer solution user");

         // Assert
         /// Check CreateSolutionUser result
         Assert.AreEqual($"{_userName}@{DOMAIN_NAME}", principal);


         // Act
         /// Delete Solution User
         Assert.DoesNotThrow(
            () => {
               _ssoAdminClient.DeleteLocalPrincipal(
                  _authorizationUsername,
                  _authorizationPassword,
                  _userName);
            });
      }

      //[Test]
      public void TestGetSsoTrustedCertificates() {
         // Act 
         var certificates = _ssoAdminClient.GetTrustedCertificatesAsync(
            _authorizationUsername,
            _authorizationPassword);

         // Assert
         Assert.NotNull(certificates);
         Assert.GreaterOrEqual(1, certificates.Length);
         Assert.NotNull(certificates[0]);
      }

      //[Test]
      public void TestGetEncodedCACertificateFromVecs() {
         // Act 
         var encodedCACertificate = _ssoAdminClient.GetEncodedCACertificateFromVecs().FirstOrDefault<string>();

         // Assert
         Assert.NotNull(encodedCACertificate);
      }

      //[Test]
      public void TestGetCACertificateFromVecs() {
         // Act 
         var all = _ssoAdminClient.GetCACertificateFromVecs().ToArray<X509Certificate2>();
         var encodedCACertificate = _ssoAdminClient.GetCACertificateFromVecs().FirstOrDefault<X509Certificate2>();

         // Assert
         Assert.NotNull(encodedCACertificate);
      }
   }
}