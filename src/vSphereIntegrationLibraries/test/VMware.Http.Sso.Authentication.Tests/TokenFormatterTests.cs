// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Moq;
using NUnit.Framework;
using VMware.Http.Sso.Authentication.Impl;
using VMware.Http.Sso.Authentication.Tests.Properties;

namespace VMware.Http.Sso.Authentication.Tests {
   public class TokenFormatterTests {
      private static string Bas64CompressedUtf8(string input) {
         var data = Encoding.UTF8.GetBytes(input);
         using (var compressedStream = new MemoryStream())
         using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress)) {
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return Convert.ToBase64String(compressedStream.ToArray());
         }
      }

      private static string GetExpected(
         string samlToken, 
         Nonce nonce, 
         byte[] bodyhash, 
         SigningAlgorithm signAlg, 
         byte[] signature) {
         return $"SIGN token=\"{Bas64CompressedUtf8(samlToken)}\", " + 
                $"nonce=\"{nonce}\", " + 
                $"bodyhash=\"{Convert.ToBase64String(bodyhash)}\", "+
                $"signature_alg=\"{SigningAlgorithmConverter.EnumToString(signAlg)}\", "+
                $"signature=\"{Convert.ToBase64String(signature)}\"";
      }

      [Test]
      public void FormatNotChunkedToken() {
         // Arrange
         var tokenMock = new Mock<IToken>();
         var nonceMock = new Mock<Nonce>();
         nonceMock.Setup(n => n.ToString()).Returns("123:654");

         var samlToken = "<xml>saml</xml>";
         tokenMock.Setup<string>(t => t.SamlToken).Returns(samlToken);

         var nonce = nonceMock.Object;
         tokenMock.Setup<Nonce>(t => t.Nonce).Returns(nonce);

         byte[] bodyHash = {1, 2, 3};
         tokenMock.Setup<byte[]>(t => t.BodyHash).Returns(bodyHash);

         SigningAlgorithm signAlg = SigningAlgorithm.RSA_SHA384;
         tokenMock.Setup<SigningAlgorithm>(t => t.SignatureAlgorithm).Returns(signAlg);

         byte[] signature = { 4, 5, 6 };
         tokenMock.Setup<byte[]>(t => t.Signature).Returns(signature);

         var expected = GetExpected(samlToken, nonce, bodyHash, signAlg, signature);

         // Act
         var actual = new TokenFormatter().Format(tokenMock.Object, 4086);

         // Assert
         Assert.AreEqual(1, actual.Length);
         Assert.AreEqual(expected, actual[0]);
      }

      [Test]
      public void FormatChunkedToken() {
         // Arrange
         var tokenMock = new Mock<IToken>();
         var nonceMock = new Mock<Nonce>();
         nonceMock.Setup(n => n.ToString()).Returns("123:654");

         var samlToken = Resources.HoKSamlToken;
         var tokenCompressedValue = Bas64CompressedUtf8(samlToken);
         var maxChunkSize = (tokenCompressedValue.Length + 3 + ParamValue.Param.token.ToString().Length) / 2 + 1;

         tokenMock.Setup<string>(t => t.SamlToken).Returns(samlToken);

         var nonce = nonceMock.Object;
         tokenMock.Setup<Nonce>(t => t.Nonce).Returns(nonce);

         byte[] bodyHash = { 1, 2, 3 };
         tokenMock.Setup<byte[]>(t => t.BodyHash).Returns(bodyHash);

         SigningAlgorithm signAlg = SigningAlgorithm.RSA_SHA384;
         tokenMock.Setup<SigningAlgorithm>(t => t.SignatureAlgorithm).Returns(signAlg);

         byte[] signature = { 4, 5, 6 };
         tokenMock.Setup<byte[]>(t => t.Signature).Returns(signature);

         // Act
         var actual = new TokenFormatter().Format(tokenMock.Object, maxChunkSize);

         // Assert
         Assert.GreaterOrEqual(actual.Length, 2);
         Assert.IsTrue(actual[0].StartsWith("SIGN token=\""));
         Assert.IsTrue(actual[0].EndsWith("\""));
         Assert.IsTrue(actual[1].StartsWith("token=\""));
      }

      [Test]
      public void ParseValidToken() {
         // Arrange
         var expectedSamlToken = "<xml>samltoken</xml>";
         var expectedNonce = Nonce.FromNow();
         var expectedBodyHash = new byte[] {1, 2, 3};
         var expectedSignature = new byte[] {1, 4, 1};
         var expectedSignatureAlg = SigningAlgorithm.RSA_SHA384;
         var token = GetExpected(
            expectedSamlToken,
            expectedNonce,
            expectedBodyHash,
            expectedSignatureAlg,
            expectedSignature);

         // Act
         var actual = new TokenFormatter().Parse(new [] { token });

         // Assert
         Assert.AreEqual(expectedSamlToken, actual.SamlToken);
         Assert.AreEqual(expectedNonce, actual.Nonce);
         Assert.That(expectedBodyHash, Is.EqualTo(actual.BodyHash));
         Assert.AreEqual(expectedSignature, actual.Signature);
         Assert.That(expectedSignatureAlg, Is.EqualTo(actual.SignatureAlgorithm));
      }

      [Test]
      public void ParseMissingSignatureToken() {
         // Arrange
         var expectedSamlToken = "<xml>samltoken</xml>";
         var expectedNonce = Nonce.FromNow();
         var expectedBodyHash = new byte[] { 1, 2, 3 };
         var expectedSignature = new byte[] { 1, 4, 1 };
         var expectedSignatureAlg = SigningAlgorithm.RSA_SHA384;
         var token = GetExpected(
            expectedSamlToken,
            expectedNonce,
            expectedBodyHash,
            expectedSignatureAlg,
            expectedSignature);
         token = token.Substring(0, token.IndexOf(", signature="));

         // Act & Assert
         Assert.Throws<AuthException>(() => new TokenFormatter().Parse(new[] { token }));
      }

      [Test]
      public void ParseChunkedToken() {
         // Arrange
         var tokenMock = new Mock<IToken>();
         var nonceMock = new Mock<Nonce>();
         var nonceString = "123:654";
         nonceMock.Setup(n => n.ToString()).Returns(nonceString);

         var samlToken = Resources.HoKSamlToken;
         var tokenCompressedValue = Bas64CompressedUtf8(samlToken);
         var maxChunkSize = (tokenCompressedValue.Length + 3 + ParamValue.Param.token.ToString().Length) / 2 + 1;

         tokenMock.Setup<string>(t => t.SamlToken).Returns(samlToken);

         var nonce = nonceMock.Object;
         tokenMock.Setup<Nonce>(t => t.Nonce).Returns(nonce);

         byte[] bodyHash = { 1, 2, 3 };
         tokenMock.Setup<byte[]>(t => t.BodyHash).Returns(bodyHash);

         SigningAlgorithm signAlg = SigningAlgorithm.RSA_SHA384;
         tokenMock.Setup<SigningAlgorithm>(t => t.SignatureAlgorithm).Returns(signAlg);

         byte[] signature = { 4, 5, 6 };
         tokenMock.Setup<byte[]>(t => t.Signature).Returns(signature);
         
         var formattedToken = new TokenFormatter().Format(tokenMock.Object, maxChunkSize);

         // Act
         var actual = new TokenFormatter().Parse(formattedToken);

         // Assert
         Assert.AreEqual(samlToken, actual.SamlToken);
         Assert.AreEqual(nonceString, actual.Nonce.ToString());
         Assert.That(bodyHash, Is.EqualTo(actual.BodyHash));
         Assert.AreEqual(signAlg, actual.SignatureAlgorithm);
         Assert.That(signature, Is.EqualTo(actual.Signature));
      }

      [Test]
      public void ParseJavaLibraryGeneratedToken() {
         // Arrange
         var token = "SIGN token=\"H4sIAAAAAAAAALOpyM2xS8zNUSjJz07Ns9EHcQHi3QwdFAAAAA==\", nonce=\"1579529430238:737412601\", bodyhash=\"8ZBm4RjyoqMQhJPl5Iur3OfYmwchbLLSV5vAz/Om+Ak=\", signature_alg=\"RSA-SHA256\", signature=\"TIUhHrX85O8mDdE6ZWq6AYSQkguHA9h7w1eRH8Lalq9aejaTscAR8cVOEy7f8CfgoQGgJet6H7V59TIpYQUfXZ8xguMzbHARugQv53jgdWinZMfnCchQZl6EAIPZrFBn0F6Pxp9R2tPSNFBQbNh8obPXVt5Lx8kiSQUVydFWKHzcMzul7rXtO4nb5dUKP99oyb93wfqxftU4IhWOVTI12iAL3qdcbXQUigQw9utpJPXAtFwsLCXAayLwaCSR2wHmoIM85tsUAdXrtuL9fy4BKmBYkx5Wv7pdBIQGqTd8WjzJAjRbqRmNZxygyn4QYCIyHwP9hYiXCzG8Tg==\"";

         // Act
         var actual = new TokenFormatter().Parse(new [] { token });

         // Assert
         Assert.NotNull(actual);
      }
   }
}
