// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using NUnit.Framework;
using VMware.Http.Sso.Authentication.Impl;
using VMware.Http.Sso.Authentication.Tests.Properties;

namespace VMware.Http.Sso.Authentication.Tests {
   public class AuthVerifierTests {
      private X509Certificate2 _signingCertificate;
      
      [SetUp]
      public void TestSetup() {
         _signingCertificate = new X509Certificate2(
            Path.Combine(
               Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
               "test_signing_certs",
               "signing.pfx"),
            "test_pass");
      }

      [Test]
      public void VerifyNoBodyRequest() {
         // Arrange
         var samlToken = Resources.HoKSamlToken;
         var authCalculator = AuthCalculatorFactory.Create();
         var request = RequestFactory.Create(HttpMethod.POST, "/api/session", "127.0.0.1", 80, null);
         var generatedToken = authCalculator.ComputeToken(request, _signingCertificate, SigningAlgorithm.RSA_SHA256, samlToken);
         var authVerifier = AuthVerifierFactory.Create(10, 10);

         // Act
         var actualSamlToken = authVerifier.VerifyToken(request, generatedToken);

         // Assert
         Assert.AreEqual(samlToken, actualSamlToken.RawXml);
      }

      [Test]
      public void VerifyRequestWithBody() {
         // Arrange
         var samlToken = Resources.HoKSamlToken;
         var authCalculator = AuthCalculatorFactory.Create();
         var request = RequestFactory.Create(HttpMethod.POST, "/api/session", "127.0.0.1", 80, "{'body': 'test'}");
         var generatedToken = authCalculator.ComputeToken(request, _signingCertificate, SigningAlgorithm.RSA_SHA256, samlToken);
         var authVerifier = AuthVerifierFactory.Create(10, 10);

         // Act
         var actualSamlToken = authVerifier.VerifyToken(request, generatedToken);

         // Assert
         Assert.AreEqual(samlToken, actualSamlToken.RawXml);
      }
   }
}
