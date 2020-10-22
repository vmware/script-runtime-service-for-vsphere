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
   public class IssueHoKTokenIntegration {
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
         _stsClient = new STSClient(
            new Uri(STS_ENDPOINT), 
            new AcceptAllX509CertificateValidator());
         _testsSigningCertificate = new X509Certificate2(
            @"C:\git-repos\SsoAdminClientLib\TestCertificate\ssoSigning.pfx",
            "ca$hc0w");
      }

      //[Test]
      public void IssueHoKTokenByUserCredetials() {
         // Act
         var rawXmlToken = _stsClient.IssueHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            _testsSigningCertificate);

         // Assert
         Assert.NotNull(rawXmlToken);
      }

      /// <summary>
      /// In order to run this test successfully:
      /// 1. Create vsphere.local user  with name "seconduser"
      /// 2. Add administrator@vsphere.local user in ActAsUser group
      /// </summary>
      //[Test]
      public void IssueActAsHoKTokenByUserCredetials() {
         // Arrange
         var actAsSecondUserToken = _stsClient.IssueHoKTokenByUserCredential(
            "seconduser@vsphere.local", 
            ConvertStringToSecureString("Admin!23"),
            new X509Certificate2(
               @"C:\git-repos\SsoAdminClientLib\TestCertificate\secondUserSigningCertificate.pfx",
               "ca$hc0w"));

         // Act
         var rawXmlToken = _stsClient.IssueActAsHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            _testsSigningCertificate,
            actAsSecondUserToken);

         // Assert
         Assert.NotNull(rawXmlToken);
      }

      //[Test]
      public void IssueDelegateToVAPIEndpointServiceSolutionHoKTokenByUserCredetials() {
         // Act
         var delegateToToken = _stsClient.IssueDelegateToHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            delegateToServiceId: "cdecdfbd-278a-4de5-a486-a95a04e90c52",
            delegateToSolutionUser: "vsphere-webclient-59e4e6d9-33da-472a-a0ff-0da488256d71");

         // Assert
         Assert.NotNull(delegateToToken);
      }

      //[Test]
      public void IssueDelegateToPSExecutionHostServiceSolutionHoKTokenByUserCredetials() {
         // Act
         var delegateToToken = _stsClient.IssueDelegateToHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            delegateToServiceId: "4f13a520-ad4e-422c-853c-38c387a964a5",
            delegateToSolutionUser: "psexecutionhost2");

         // Assert
         Assert.NotNull(delegateToToken);
      }

      //[Test]
      public void IssueDelegateToRenewableHoKToken() {
         // Act
         var reneableDelegatedToken = _stsClient.IssueDelegateToHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            delegateToServiceId: "4f13a520-ad4e-422c-853c-38c387a964a5",
            delegateToSolutionUser: "psexecutionhost2",
            renewable:true);

         // Assert
         Assert.NotNull(reneableDelegatedToken);
      }

      //[Test]
      public void IssueHoKTokenFromDelegatedToken() {
         // Arrange
         var delegateToToken = _stsClient.IssueDelegateToHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            delegateToServiceId: "4f13a520-ad4e-422c-853c-38c387a964a5",
            delegateToSolutionUser: "psexecutionhost2");

         /// psexecutionhost2 signing certificate. psexecutionhost2 principal is
         /// registered with this certificate in the SSO
         var psExecutionHostSigningCertificate = new X509Certificate2(
            @"C:\git-repos\SsoAdminClientLib\TestCertificate\solutionUserCertificate.pfx",
            "ca$hc0w");

         // Act
         var rawXmlToken = _stsClient.IssueHoKTokenByHoKToken(
            delegateToToken,
            psExecutionHostSigningCertificate);

         // Assert
         Assert.NotNull(rawXmlToken);
      }

      //[Test]
      public void IssueBearerTokenFromSolutionHoKToken() {
         // This test covers workflow for 
         // 1. User authenticates to solution 1
         // 2. Solution 1 acquires delegate to token for Script Runtime Service
         // 3. Script Runtime Service acquires HoK token from the delegated token
         // 4. Script Runtime Service acquires bearer token that can be given to PowerCLI to connect to VC

         // Arrange
         /// Let's suppose Solution 1 issues this delegate to HoK token
         var delegateToToken = _stsClient.IssueDelegateToHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            delegateToServiceId: "4f13a520-ad4e-422c-853c-38c387a964a5",
            delegateToSolutionUser: "psexecutionhost2");

         /// psexecutionhost2 signing certificate. psexecutionhost2 principal is
         /// registered with this certificate in the SSO
         var psExecutionHostSigningCertificate = new X509Certificate2(
            @"C:\git-repos\SsoAdminClientLib\TestCertificate\solutionUserCertificate.pfx",
            "ca$hc0w");

         /// SRS Acquires HoK token with its solution user
         var sesHokToken = _stsClient.IssueHoKTokenByHoKToken(
            delegateToToken,
            psExecutionHostSigningCertificate);

         // Act
         var rawXmlToken = _stsClient.
            IssueBearerTokenByHoKToken(
               sesHokToken,
               psExecutionHostSigningCertificate);

         // Assert
         Assert.NotNull(rawXmlToken);
         // Validate Bearer SAML Token with SSO Service
         Assert.IsTrue(_stsClient.ValidateToken(rawXmlToken));
      }


      //[Test]
      public void RenewHoKToken() {
         // Act
         var renewableDelegatedToken = _stsClient.IssueDelegateToHoKTokenByUserCredential(
            VC_USERNAME,
            ConvertStringToSecureString(VC_PASSWORD),
            delegateToServiceId: "4f13a520-ad4e-422c-853c-38c387a964a5",
            delegateToSolutionUser: "psexecutionhost2",
            renewable: true);

         /// psexecutionhost2 signing certificate. psexecutionhost2 principal is
         /// registered with this certificate in the SSO
         var psExecutionHostSigningCertificate = new X509Certificate2(
            @"C:\git-repos\SsoAdminClientLib\TestCertificate\solutionUserCertificate.pfx",
            "ca$hc0w");;
         
         // Act
         var renewedToken = _stsClient.RenewToken(
            renewableDelegatedToken,
            psExecutionHostSigningCertificate,
            null);

         // Assert
         Assert.NotNull(renewedToken);
      }
   }
}