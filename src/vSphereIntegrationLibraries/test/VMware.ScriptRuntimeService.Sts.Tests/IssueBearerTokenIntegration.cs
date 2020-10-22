// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IdentityModel.Selectors;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using VMware.ScriptRuntimeService.Sts;

namespace VMware.ScriptRuntimeService.Sts.Tests
{
   /// <summary>
   /// Contains integrations tests interacting with vSphere STS
   /// To run those configure test vSphere on STS_ENDPOINT with VC_USERNAME, VC_PASSWORD constant
   /// and uncomment [Test] attributes
   /// </summary>
   public class IssueBearerTokenIntegration
   {
      private const string STS_ENDPOINT = "https://10.23.82.98/sts/STSService/vsphere.local";
      private const string VC_USERNAME = "administrator@vsphere.local";
      private const string VC_PASSWORD = "Admin!23";
      private STSClient _stsClient;
      private X509Certificate2 _testsSigningCertificate;

      private class AcceptAllX509CertificateValidator : X509CertificateValidator
      {
         public override void Validate(X509Certificate2 certificate) {
            // Check that there is a certificate.
            if (certificate == null) {
               throw new ArgumentNullException(nameof(certificate));
            }
         }
      }


      private static SecureString ConvertStringToSecureString(string str)
      {
         // There must be a better way to do it but I'm not wasting more time
         // with it right now
         SecureString secureString = new SecureString();
         foreach (char c in str)
         {
            secureString.AppendChar(c);
         }

         return secureString;
      }

      [SetUp]
      public void Setup()
      {
         _stsClient = new STSClient(new Uri(STS_ENDPOINT), 
            new AcceptAllX509CertificateValidator());
         _testsSigningCertificate = new X509Certificate2(
            @"C:\git-repos\SsoAdminClientLib\TestCertificate\ssoSigning.pfx",
            "ca$hc0w");
      }

      //[Test]
      public void IssueBearerTokenByUserCredential()
      {
         var rawXmlToken = _stsClient.IssueBearerTokenByUserCredential(VC_USERNAME, ConvertStringToSecureString(VC_PASSWORD));
         Assert.NotNull(rawXmlToken);
      }

      //[Test]
      public void IssueBearerTokenByHoKToken() {
         // Arrange
         var rawHoKXmlToken = _stsClient.IssueHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            _testsSigningCertificate);

         // Act
         var rawXmlToken = _stsClient.
            IssueBearerTokenByHoKToken(
               rawHoKXmlToken,
               _testsSigningCertificate);

         // Assert
         Assert.NotNull(rawXmlToken);
      }

      //[Test]
      public void ValidateBearerToken()
      {
         // Arrange
         var rawXmlToken = _stsClient.IssueBearerTokenByUserCredential(VC_USERNAME, ConvertStringToSecureString(VC_PASSWORD));
         
         // Act
         _stsClient.ValidateToken(rawXmlToken);
      }
   }
}