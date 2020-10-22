// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using NUnit.Framework;
using VMware.Http.Sso.Authentication.Impl;
using VMware.Http.Sso.Authentication.Tests.Properties;

namespace VMware.Http.Sso.Authentication.Tests {
   public class TokenTests {
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
      public void NonComputePublicProperties() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.AreEqual(Resources.HoKSamlToken, token.SamlToken);
            Assert.AreEqual(expectedAlg, token.SignatureAlgorithm);
         }
      }

      [Test]
      public void ComputeBodyHash() {
         // Arrange
         var body = "{'body':'test'}";
         var expectedAlg = SigningAlgorithm.RSA_SHA256;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            body);

         byte[] expectedBodyHash = null;
         using (var hashAlgorithm = SHA256.Create()) {
            expectedBodyHash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(body));
         }

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.That(expectedBodyHash, Is.EqualTo(token.BodyHash));
         }
      }

      [Test]
      public void NonceGenerated() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         var expectedNonceDate = (DateTimeOffset.Now.ToUniversalTime()).ToUnixTimeMilliseconds().ToString();

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);
            var nonceDate = token.Nonce.ToString().Split(':')[0];

            // Assert
            // Check nonce generated date is not later a second after expected date
            Assert.LessOrEqual(long.Parse(nonceDate) - long.Parse(expectedNonceDate), 1000);
         }
      }

      [Test]
      public void ComputeAndVerifySignature() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.DoesNotThrow(() => {
               token.VerifySignature(testRequest, _signingCertificate);
            });
         }
      }

      [Test]
      public void ComputeAndVerifySignatureOfNoBodyRequest() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            null);

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.DoesNotThrow(() => {
               token.VerifySignature(testRequest, _signingCertificate);
            });
         }
      }

      [Test]
      public void VerifySignatureModifiedHostThrows() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         var modifiedRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "badhost",
            80,
            "{'body':'test'}");

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.Throws<AuthException>(() => {
               token.VerifySignature(modifiedRequest, _signingCertificate);
            });
         }
      }

      [Test]
      public void VerifySignatureModifiedBodyThrows() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         var modifiedRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'modified'}");

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.Throws<AuthException>(() => {
               token.VerifySignature(modifiedRequest, _signingCertificate);
            });
         }
      }

      [Test]
      public void VerifySignatureRemovedBodyThrows() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         var modifiedRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            null);

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.Throws<AuthException>(() => {
               token.VerifySignature(modifiedRequest, _signingCertificate);
            });
         }
      }

      [Test]
      public void ComputeAndVerifyBodyHash() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.DoesNotThrow(() => {
               token.VerifyBodyHash(testRequest);
            });
         }
      }

      [Test]
      public void ComputeAndVerifyBodyHashOfNoBodyRequest() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            null);

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.DoesNotThrow(() => {
               token.VerifyBodyHash(testRequest);
            });
         }
      }

     [Test]
      public void VerifyBodyHashModifiedBodyThrows() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         var modifiedRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'modified'}");

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.Throws<AuthException>(() => {
               token.VerifyBodyHash(modifiedRequest);
            });
         }
      }

      [Test]
      public void VerifyBodyHashRemovedBodyThrows() {
         // Arrange
         var expectedAlg = SigningAlgorithm.RSA_SHA384;
         var testRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            "{'body':'test'}");

         var modifiedRequest = RequestFactory.Create(
            HttpMethod.POST,
            "/api/sessions",
            "localhost",
            80,
            null);

         // Act
         using (var privateKey = _signingCertificate.GetRSAPrivateKey()) {
            var token = new Token(
               testRequest,
               privateKey,
               expectedAlg,
               Resources.HoKSamlToken);

            // Assert
            Assert.Throws<AuthException>(() => {
               token.VerifyBodyHash(modifiedRequest);
            });
         }
      }

   }
}
