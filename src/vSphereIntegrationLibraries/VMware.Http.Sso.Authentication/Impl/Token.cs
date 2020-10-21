// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication.Impl {
   public class Token : IToken  {
      private const char NORMALIZATION_DELIMITER = '\n';
      private string _samlTokenXml;
      private Nonce _nonce;
      private byte[] _bodyHash;
      private byte[] _signature;

      public Token(IRequest request, RSA signer, SigningAlgorithm signingAlgorithm, string samlTokenXml) {
         _samlTokenXml = samlTokenXml;
         _nonce = Nonce.FromNow();
         SignatureAlgorithm = signingAlgorithm;
         _bodyHash = ComputeBodyHash(request);
         _signature = ComputeSignature(request, signer);
      }

      public Token(string samlTokenXml, Nonce nonce, SigningAlgorithm signingAlgorithm, byte[] bodyHash, byte[] signature) {
         _samlTokenXml = samlTokenXml;
         _nonce = nonce;
         SignatureAlgorithm = signingAlgorithm;
         _bodyHash = bodyHash;
         _signature = signature;
      }

      #region Public Interface
      public string SamlToken => _samlTokenXml;
      public Nonce Nonce => _nonce;
      public byte[] BodyHash => _bodyHash;
      public SigningAlgorithm SignatureAlgorithm { get; private set; }
      public byte[] Signature => _signature;

      public void VerifyBodyHash(IRequest request) {
         if (request == null) {
            throw new AuthException(Resources.VerifySignature_Expects_NonNull_Request);
         }

         var requestBodyHash = ComputeBodyHash(request);
         if ((_bodyHash != null && requestBodyHash != null && !_bodyHash.SequenceEqual(requestBodyHash)) ||
             _bodyHash != null && requestBodyHash == null ||
             _bodyHash == null && requestBodyHash != null) {
            throw new AuthException(Resources.Verify_BodyHash_Failed);
         }
      }

      public void VerifySignature(IRequest request, X509Certificate2 signingCertificate) {
         if (request == null || signingCertificate == null) {
            throw new AuthException(Resources.VerifySignature_Expects_NonNull_RequestAndCertificate);
         }

         using (var publicKey = signingCertificate.GetRSAPublicKey()) {
            if (!publicKey.VerifyData(
               NormalizeMsg(request),
               Signature,
               SigningAlgorithmConverter.ToHashAlgorithmName(SignatureAlgorithm),
               RSASignaturePadding.Pkcs1)) {

               throw new AuthException(Resources.Verify_Signature_Failed);
            }
         }
      }
      #endregion

      private HashAlgorithm CreateHashAlgorithm() {
         if (SignatureAlgorithm == SigningAlgorithm.RSA_SHA256) {
            return SHA256.Create();
         }
         if (SignatureAlgorithm == SigningAlgorithm.RSA_SHA384) {
            return SHA384.Create();
         }
         if (SignatureAlgorithm == SigningAlgorithm.RSA_SHA512) {
            return SHA512.Create();
         }
         throw new AuthenticationException(
            string.Format(
               Resources.Hash_Algorithm_Not_Supported, 
               SigningAlgorithmConverter.ToHashAlgorithmName(SignatureAlgorithm)));
      }

      private HashAlgorithmName FromName(string signatureAlgorithm) {
         return Enum.Parse<HashAlgorithmName>(signatureAlgorithm);
      }

      private byte[] ComputeBodyHash(IRequest request) {
         byte[] result = null;

         if (request.Body != null) {
            using (var hashAlgorithm = CreateHashAlgorithm()) {
               result = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(request.Body));
            }
         }

         return result;
      }

      private byte[] ComputeSignature(IRequest request, RSA signingAlgorithm) {
         return signingAlgorithm.SignData(
            NormalizeMsg(request), 
            SigningAlgorithmConverter.ToHashAlgorithmName(SignatureAlgorithm), 
            RSASignaturePadding.Pkcs1);
      }

      private byte[] NormalizeMsg(IRequest request) {
         StringBuilder txtMsg = new StringBuilder();

         AppendElement(txtMsg, Nonce);
         AppendElement(txtMsg, request.Method.ToString().ToUpper());
         AppendElement(txtMsg, request.RequestUri);
         AppendElement(txtMsg, request.HostName);
         AppendElement(txtMsg, request.Port);
         var bodyHash = ComputeBodyHash(request);

         byte[] txtMsgUtf8Bytes = Encoding.UTF8.GetBytes(txtMsg.ToString());

         byte[] res = new byte[txtMsgUtf8Bytes.Length + (bodyHash?.Length ?? 0) + 1];

         int offset = 0;
         Array.Copy(txtMsgUtf8Bytes, res, txtMsgUtf8Bytes.Length);
         offset += txtMsgUtf8Bytes.Length;
         if (bodyHash != null) {
            Array.Copy(bodyHash, 0, res, offset, bodyHash.Length);
            offset += bodyHash.Length;
         }
         res[offset] = 10;
         return res;
      }

      private void AppendElement(StringBuilder txtMsg, object element) {
         txtMsg.Append(element);
         txtMsg.Append(NORMALIZATION_DELIMITER);
      }
   }
}
