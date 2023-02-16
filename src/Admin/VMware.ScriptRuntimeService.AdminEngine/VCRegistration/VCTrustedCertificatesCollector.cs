// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration.Exceptions;

namespace VMware.ScriptRuntimeService.AdminEngine.VCRegistration {
   /// <summary>
   /// This class gets trusted certificates from vCenter server.
   /// </summary>
   public class VCTrustedCertificatesCollector {
      private readonly Func<HttpMessageHandler, HttpMessageHandler> _httpMessageHandlerFactory;
      private readonly ILoggerFactory _loggerFactory;
      private readonly string _psc;
      private readonly string _username;
      private readonly SecureString _password;
      private readonly string _thumbprint;
      private readonly bool _ignoreServerCertificateValidation;
      private readonly ILogger _logger;

      public VCTrustedCertificatesCollector(
         ILoggerFactory loggerFactory,
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool ignoreServerCertificateValidation) : this(null, loggerFactory, psc, username, password, thumbprint, ignoreServerCertificateValidation) {

      }

      internal VCTrustedCertificatesCollector(
         Func<HttpMessageHandler, HttpMessageHandler> httpMessageHandlerFactory,
         ILoggerFactory loggerFactory,
         string psc,
         string username,
         SecureString password,
         string thumbprint,
         bool ignoreServerCertificateValidation) {

         _httpMessageHandlerFactory = httpMessageHandlerFactory;
         _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
         _logger = loggerFactory.CreateLogger(typeof(VCRegistrator));

         _psc = psc;
         _username = username;
         _password = password;
         _thumbprint = thumbprint;
         _ignoreServerCertificateValidation = ignoreServerCertificateValidation;
      }

      public IEnumerable<string> GetEncodedTrustedCertificates() {
         _logger.LogInformation(
                  string.Format(Resources.PerofomingOperation, Resources.EncodedTrustedCertificatesRetrieval));
         var task = Task.Run<IEnumerable<string>>(async () => {
            using (var client = GetHttpClient()) {
               await AuthenticateClientAsync(client);

               List<string> certs = new List<string>();
               foreach (var chain in await GetTrustedRootChainsChainsAsync(client)) {
                  foreach (var cert in await GetTrustedCertificateAsync(client, chain)) {
                     certs.Add(cert);

                     _logger.LogDebug($"Trusted CA cert: {cert}");
                  }
               }

               return certs;
            }
         });

         try {
            task.Wait();
            return task.Result;
         } catch (AggregateException ex) {
            throw new TrustedCertificateRetrievalException("Getting trusted certificates failed", ex);
         }
      }

      private async Task<IEnumerable<string>> GetTrustedCertificateAsync(HttpClient client, string chain) {
         // https://{api_host}/api/vcenter/certificate-management/vcenter/trusted-root-chains/{chain}
         using (var response = await client.GetAsync($"/api/vcenter/certificate-management/vcenter/trusted-root-chains/{chain}")) {
            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(responseStr);

            List<string> result = new List<string>();

            string cert_chain = json.cert_chain.cert_chain[0];
            int startIndex = 0;
            var beginIndex = cert_chain.IndexOf("-----BEGIN CERTIFICATE-----", startIndex);
            var endIndex = cert_chain.IndexOf("-----END CERTIFICATE-----", startIndex);
            while (beginIndex > -1 && endIndex > beginIndex) {
               result.Add(cert_chain.Substring(beginIndex, endIndex + beginIndex + 25).Trim());

               startIndex = endIndex + 1;
               beginIndex = cert_chain.IndexOf("-----BEGIN CERTIFICATE-----", startIndex);
               endIndex = cert_chain.IndexOf("-----END CERTIFICATE-----", startIndex);
            }

            return result;
         }

         //{
         //   "cert_chain": {
         //      "cert_chain": [
         //        "-----BEGIN CERTIFICATE-----\nMIIFOzCCA6OgAwIBAgIJAOrjMkQMNfJUMA0GCSqGSIb3DQEBCwUAMIGoMQswCQYD\nVQQDDAJDQTEXMBUGCgmSJomT8ixkARkWB3ZzcGhlcmUxFTATBgoJkiaJk/IsZAEZ\nFgVsb2NhbDELMAkGA1UEBhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExKjAoBgNV\nBAoMIXNjMi0xMC0xODUtMjI1LTE5OS5lbmcudm13YXJlLmNvbTEbMBkGA1UECwwS\nVk13YXJlIEVuZ2luZWVyaW5nMB4XDTIzMDIxMDEyMDAzNVoXDTMzMDIwNzEyMDAz\nNVowgagxCzAJBgNVBAMMAkNBMRcwFQYKCZImiZPyLGQBGRYHdnNwaGVyZTEVMBMG\nCgmSJomT8ixkARkWBWxvY2FsMQswCQYDVQQGEwJVUzETMBEGA1UECAwKQ2FsaWZv\ncm5pYTEqMCgGA1UECgwhc2MyLTEwLTE4NS0yMjUtMTk5LmVuZy52bXdhcmUuY29t\nMRswGQYDVQQLDBJWTXdhcmUgRW5naW5lZXJpbmcwggGiMA0GCSqGSIb3DQEBAQUA\nA4IBjwAwggGKAoIBgQDAyDKYVYwiCMZDSAgxHu595pU/W5pVFMv4gA01z2XfrOZY\nSuIQFY2PTtxjMM7p/3+oPw/sWXhO6ig94jguBZVselu/ksW+N2U+2nUJoScdGGio\nfGvXsV0DmJ0nMfdnSpVmY6R1ImIqZkRjonR4qpHGhzRjqjzSL/hU1Djiez+pIaKW\nugQ9QCFO+DVwiZ2OQqNESPOPQHera0bn4qzJSAjjibsyaoI7MCqhC0cM3oIZ8xLK\nq4QoP0fhUlv8fGLSb3gztO9QD3GGGZf/3nb3Xkf4l1TszKljzVMEjJbztnTiNIHj\nkRd1Am0X3DWpQl2yI6qwOU/Jbt7gVVad+7c9lmxnHVrMjX08pNF6A5k+X7MPg3CS\nWVcfs2LX/J41bTfOQpCgWYZnsh7OfSVlIDthfwh3f8MvSHc+JkACCJwVHTxR4uNW\nER48InN6p2/V+1GglliV/fo9ZMW9e6fI5EwQbEBGScOvpzkV59ABsl4SE0M7J8WT\n3AFrg+hO/cZkQACuTL0CAwEAAaNmMGQwHQYDVR0OBBYEFLobb584cG5Pqq+KoKFq\nfh5gxOv+MB8GA1UdEQQYMBaBDmVtYWlsQGFjbWUuY29thwR/AAABMA4GA1UdDwEB\n/wQEAwIBBjASBgNVHRMBAf8ECDAGAQH/AgEAMA0GCSqGSIb3DQEBCwUAA4IBgQCA\nynTFEvpjnBcrV7gjI2GBbVwK4kJqRURWbFMUZO/pgUKgT9pA7f2bOuoVgnLn0suY\nXXTPK0hBRQ5CsPvgUPuq2B3u9rOf25WMlnasDWqG3NPi+8GFsYKRr2nQecClI36q\nPmafcQ+cao3fm5OEXrecZRb5tG313p0YtlYgu9dJRjIw9CcJHrDsNKvj5WMyqrD0\ndHWlzq+DvZSZRLbk+ddn1QaU9D6SXYY+t8rSH+XvFj+RCgNZ1zWi4oxtx22rfMW6\n2dDooR4tRZ4FdtD9OOBTUg8bZs78hgBHD+2Hi5pH+HobO8yYvPs1BHPiAL/7/GCi\nRviQ205ZFw0hbMn8PMOQrHilFA1jx4NxqRlEWSBSOFVrmuB2YftHit77rzwfvHCl\nZ27QNsJu6u+wbGb9IMyipNsWWZ8sr7dAegdm3JkgJ9Ig89BfxsIfGXykuKrVYyx3\nrbhtJpljRHRdB0in5pvu36XCTrUWKcYoEjPSPrAr8MEd6hRqx1AlHglo41lQvNM=\n-----END CERTIFICATE-----\n-----BEGIN X509 CRL-----\nMIICpDCCAQwCAQEwDQYJKoZIhvcNAQELBQAwgagxCzAJBgNVBAMMAkNBMRcwFQYK\nCZImiZPyLGQBGRYHdnNwaGVyZTEVMBMGCgmSJomT8ixkARkWBWxvY2FsMQswCQYD\nVQQGEwJVUzETMBEGA1UECAwKQ2FsaWZvcm5pYTEqMCgGA1UECgwhc2MyLTEwLTE4\nNS0yMjUtMTk5LmVuZy52bXdhcmUuY29tMRswGQYDVQQLDBJWTXdhcmUgRW5naW5l\nZXJpbmcXDTIzMDIxNTEzNTAzNVoXDTIzMDMxNzEzNTAzNVqgLzAtMAoGA1UdFAQD\nAgELMB8GA1UdIwQYMBaAFLobb584cG5Pqq+KoKFqfh5gxOv+MA0GCSqGSIb3DQEB\nCwUAA4IBgQAGIImskOv8S2F140sGtP/mZ5MossDuks1cfbAC6nwsrPBDcwaoqpnl\n317ixIb3BHoW0b3MQydYivzybYehp//HxWOzqFFu0DJtBxEYbFADbxR33mr2O2hu\n8ANT8zTUgosBlheLqUZSH5DumAtiZa/Q1JKUGCG4QuFoMHi5EPfRK5/qdFeOEz+I\n1OELahB+9pyd6kO8EhzmmhN8UFUwImGJhdAR0NdXE7naBEQv2Y/nKhOk8gbJUEAR\nifDNeVdptKQR0cp+rS7ygscU1tfIntzdd9hkVGpSwuPkOK27iSqxjO9OQW6/x57M\nbJc1t39KmbqiwuDf/anJvzyM4sUVPPw4+ZMZdoqkBPW58eRZdk+x5I6kXQf+ZntC\nEvcSdV0JQGC8mR+j+YD36lfLIkL60payKMwohIhEr7Cj2SMVEJRZv3RgHaWi+e6U\nKkgOV2RbFt5wV5A+CczVJVDKPU4Q0wFsV23HIhURXmEDUz1ZLZ6I2vJaj5eJ80za\nYSCgKPcAsxA=\n-----END X509 CRL-----"
         //     ]
         //   }
         //}


            // dynamic result = JsonConvert.DeserializeObject<dynamic>(responseBody);
      }

      private async Task<IEnumerable<string>> GetTrustedRootChainsChainsAsync(HttpClient client) {
         // https://{api_host}/api/vcenter/certificate-management/vcenter/trusted-root-chains
         using (var response = await client.GetAsync("/api/vcenter/certificate-management/vcenter/trusted-root-chains")) {
            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject<dynamic>(responseStr);

            return ((IEnumerable) result).Cast<dynamic>().Select(r => (string) r.chain);
         }
         //[
         //   {
         //      "chain": "BA1B6F9F38706E4FAAAF8AA0A16A7E1E60C4EBFE"
         //   }
         //]

         // 
      }

      private HttpClient GetHttpClient() {
         _logger.LogInformation("Creating http client");

         HttpMessageHandler clientHandler = new HttpClientHandler() {
            ServerCertificateCustomValidationCallback = (message, certificate, chain, policyErrors) => {

               if (certificate == null) { throw new ArgumentNullException(nameof(certificate)); }

               if (policyErrors == System.Net.Security.SslPolicyErrors.None) {
                  return true;
               } else if (_ignoreServerCertificateValidation) {
                  return true;
               } else {
                  var normalizedThumbprint = _thumbprint?.Replace(":", "")?.Replace(" ", "")?.ToUpperInvariant();
                  var sha1Thumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA1);
                  var sha256Thumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);

                  if (!sha1Thumbprint.Equals(normalizedThumbprint) && !sha256Thumbprint.Equals(normalizedThumbprint)) {
                     _logger.LogError(
                        $"VC [${_psc}] certificate thumbprint [sha1:{sha1Thumbprint}, sha256:{sha256Thumbprint}] does not match the supplied one [{_thumbprint}]");
                  }

                  return sha1Thumbprint.Equals(normalizedThumbprint) || sha256Thumbprint.Equals(normalizedThumbprint);
               }
            }
         };

         if (_httpMessageHandlerFactory != null) {
            clientHandler = _httpMessageHandlerFactory.Invoke(clientHandler);
         }

         return new HttpClient(clientHandler) {
            BaseAddress = new Uri($"https://{_psc}")
         };
      }

      private async Task AuthenticateClientAsync(HttpClient client) {
         _logger.LogInformation("Establishing session");

         string authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(_username + ":" + SecureStringToString(_password)));

         // https://{api_host}/api/session

         using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/session")) {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);
            using (var response = await client.SendAsync(request)) {
               if (response.StatusCode != HttpStatusCode.Created) {
                  throw new TrustedCertificateRetrievalException("Unable to create session for getting CA trusted certificates");
               }

               var sessionId = response.Content.ReadAsStringAsync().Result?.Trim('"');
               if (string.IsNullOrEmpty(sessionId)) {
                  throw new TrustedCertificateRetrievalException("Server returned empty session id for getting CA trusted certificates");
               } else {
                  client.DefaultRequestHeaders.Add("vmware-api-session-id", sessionId);
               }
            }
         }
      }

      private String SecureStringToString(SecureString value) {
         IntPtr valuePtr = IntPtr.Zero;
         try {
            valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
            return Marshal.PtrToStringUni(valuePtr);
         } finally {
            Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
         }
      }
   }
}
