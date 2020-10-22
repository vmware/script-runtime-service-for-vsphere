// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication.Impl {

   /**
    * Source: https://wiki.eng.vmware.com/SSO/REST
    * Formats and parses HTTP token.
    *
    * Grammar:<br>
    * <code> credentials  = 'SIGN' RWS 1#param<br>
    * param               = token | nonce | body-hash | signature-algorithm | signature<br>
    * token               = 'token' '=' <"> plain-string <"><br>
    * nonce               = 'nonce' '=' <"> 1*DIGIT ':' plain-string <"><br>
    * body-hash           = 'bodyhash' '=' <"> plain-string <"><br>
    * signature-algorithm = 'signature_alg' '=' <"> plain-string <"><br>
    * signature           = 'signature' '=' <"> plain-string <"><br>
    * plain-string        = 1*( DIGIT | ALPHA | %x20-21 | %x23-5B | %x5D-7E )<br>
    *
    * 1#element => element *( OWS "," OWS element )<br>
    * RWS - required whitespace<br>
    * RWS = 1*( [ obs-fold ] WSP )<br>
    * WSP = SP | HTAB<br>
    * OWS = *( [ obs-fold ] WSP )<br>
    * obs-fold = CRLF<br>
    * </code>
    *
    * The formatter is safe for use by multiple threads.
    */
   public class TokenFormatter {
      private const string AUTH_SCHEME = "SIGN";

      static byte[] Compress(byte[] data) {
         using (var compressedStream = new MemoryStream())
         using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress)) {
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
         }
      }

      static byte[] Decompress(byte[] data) {
         var result = new List<byte>();

         using (var compressedStream = new MemoryStream(data))
         using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress)) {
            int count = 0;
            do {
               byte[] buffer = new byte[1024];
               count = zipStream.Read(buffer, 0, buffer.Length);
               result.AddRange(new Span<byte>(buffer, 0, count).ToArray());
            } while (count > 0);

            return result.ToArray();
         }
      }

      private ChunkBuilder AppendParam(List<string> chunks, ChunkBuilder chunkFormatter, int maxLength, ParamValue param) {
         ChunkBuilder res = chunkFormatter;
         while (param != null) {
            ParamValue next = res.Append(param);
            if (next != null && next.Equals(param)) {
               chunks.Add(res.GetValue());
               res = new ChunkBuilder(maxLength);
            }
            param = next;
         }
         return res;
      }

      public string[] Format(IToken token, int maxChunkLength) {
         var parameters = new List<ParamValue>();
         parameters.Add(new ParamValue(ParamValue.Param.token, Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(token.SamlToken)))));
         parameters.Add(new ParamValue(ParamValue.Param.nonce, token.Nonce.ToString()));
         if (token.BodyHash != null) {
            parameters.Add(new ParamValue(ParamValue.Param.bodyhash, Convert.ToBase64String(token.BodyHash)));
         }
         parameters.Add(new ParamValue(ParamValue.Param.signature_alg, SigningAlgorithmConverter.EnumToString(token.SignatureAlgorithm)));
         parameters.Add(new ParamValue(ParamValue.Param.signature, Convert.ToBase64String(token.Signature)));

         var res = new List<string>();

         ChunkBuilder chunkFormatter = new ChunkBuilder(maxChunkLength, $"{AUTH_SCHEME} ");
         foreach (var param in parameters) {
            chunkFormatter = AppendParam(res, chunkFormatter, maxChunkLength, param);
         }

         res.Add(chunkFormatter.GetValue());

         return res.ToArray();
      }

      public IToken Parse(string[] tokens) {
         var paramValues = new Dictionary<ParamValue.Param, ParamValue>();
         for (int i = 0; i < tokens.Length; i++) {
            var chunkParser = new ChunkParser(tokens[i], i == 0 ? AUTH_SCHEME : null);
            do {
               var currentParamValue = chunkParser.Current;
               if (currentParamValue == null) continue;

               if (paramValues.ContainsKey(currentParamValue.Key)) {
                  paramValues[currentParamValue.Key] = paramValues[currentParamValue.Key].Concat(currentParamValue);
               } else {
                  paramValues[currentParamValue.Key] = currentParamValue;
               }

            } while (chunkParser.MoveNext());
         }

         CheckMissingParams(paramValues);

         var samlToken = Encoding.UTF8.GetString(
            Decompress(
               Convert.FromBase64String(
                  paramValues[ParamValue.Param.token].Value)));

         var nonce = Nonce.FromString(paramValues[ParamValue.Param.nonce].Value);

         var signAlgorithm = SigningAlgorithmConverter.StringToEnum(paramValues[ParamValue.Param.signature_alg].Value);

         byte[] bodyHash = null;
         if (paramValues.ContainsKey(ParamValue.Param.bodyhash)) {
            bodyHash = Convert.FromBase64String(paramValues[ParamValue.Param.bodyhash].Value);
         }

         var signature = Convert.FromBase64String(paramValues[ParamValue.Param.signature].Value);

         return new Token(samlToken, nonce, signAlgorithm, bodyHash, signature);
      }

      private void CheckMissingParams(Dictionary<ParamValue.Param, ParamValue> paramValues) {
         foreach (var name in Enum.GetNames(typeof(ParamValue.Param))) {
            if ((ParamValue.Param.bodyhash.ToString() == name)) continue;
            if (!paramValues.ContainsKey(Enum.Parse<ParamValue.Param>(name))) {
               throw new AuthException(string.Format(Resources.Param_Is_Missing_In_Token, name));
            }
         }
      }
   }
}
