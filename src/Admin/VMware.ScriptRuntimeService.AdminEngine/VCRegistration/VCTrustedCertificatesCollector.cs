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
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration.Exceptions;

namespace VMware.ScriptRuntimeService.AdminEngine.VCRegistration {
   /// <summary>
   /// This class gets trusted certificates from vCenter server.
   /// </summary>
   public class VCTrustedCertificatesCollector {
      private const string PEM_BEGIN = "-----BEGIN CERTIFICATE-----";
      private const string PEM_END = "-----END CERTIFICATE-----";

      private readonly Func<HttpMessageHandler, HttpMessageHandler> _httpMessageHandlerFactory;
      private readonly ILoggerFactory _loggerFactory;
      private readonly ILogger _logger;
      private readonly string _psc;
      private readonly string _username;
      private readonly SecureString _password;
      private readonly string _thumbprint;
      private readonly bool _ignoreServerCertificateValidation;

      public class LoggingHandler : DelegatingHandler {
         private readonly ILoggerFactory _loggerFactory;
         private readonly ILogger _logger;

         public LoggingHandler(ILoggerFactory loggerFactory, HttpMessageHandler innerHandler)
             : base(innerHandler) {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<LoggingHandler>();
         }
         
         internal LoggingHandler(ILoggerFactory loggerFactory, ILogger logger, HttpMessageHandler innerHandler)
             : base(innerHandler) {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _loggerFactory = loggerFactory;
            _logger = logger;
         }

         protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            _logger.LogDebug($"Request: {await ToLogMessage(request)}");

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            _logger.LogDebug($"Response: {await ToLogMessage(response)}");

            return response;
         }

         private async static Task<string> ToLogMessage(HttpResponseMessage response) {

            if (null == response) { return "<null>"; }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("StatusCode: ");
            stringBuilder.Append((int) response.StatusCode);
            stringBuilder.Append(", ReasonPhrase: '");
            stringBuilder.Append(response.ReasonPhrase ?? "<null>");
            stringBuilder.Append("', Version: ");
            stringBuilder.Append(response.Version);
            stringBuilder.Append(", Content: ");
            stringBuilder.Append((response.Content == null) ? "<null>" : await response.Content.ReadAsStringAsync());
            stringBuilder.Append(", Headers:\r\n");
            stringBuilder.Append(DumpHeaders(Enumerable.Empty<string>(), response.Headers, (response.Content == null) ? null : response.Content.Headers));
            return stringBuilder.ToString();
         }

         private async static Task<string> ToLogMessage(HttpRequestMessage request) {

            if (null == request) { return "<null>"; }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Method: ");
            stringBuilder.Append(request.Method);
            stringBuilder.Append(", RequestUri: '");
            stringBuilder.Append((request.RequestUri == null) ? "<null>" : request.RequestUri.ToString());
            stringBuilder.Append("', Version: ");
            stringBuilder.Append(request.Version);
            stringBuilder.Append(", Content: ");
            stringBuilder.Append((request.Content == null) ? "<null>" : await request.Content.ReadAsStringAsync());
            stringBuilder.Append(", Headers:\r\n");
            stringBuilder.Append(DumpHeaders(new[] { "Authorization" }, request.Headers, (request.Content == null) ? null : request.Content.Headers));
            return stringBuilder.ToString();
         }

         private static string DumpHeaders(IEnumerable<string> headersToSanitize, params HttpHeaders[] headers) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{\r\n");
            for (int i = 0; i < headers.Length; i++) {
               if (headers[i] == null) {
                  continue;
               }
               foreach (KeyValuePair<string, IEnumerable<string>> item in headers[i]) {
                  if (headersToSanitize?.Any(h => h.Equals(item.Key, StringComparison.OrdinalIgnoreCase)) ?? false) {
                     stringBuilder.Append("  ");
                     stringBuilder.Append(item.Key);
                     stringBuilder.Append(": sanitized\r\n");
                  } else {
                     foreach (string item2 in item.Value) {
                        stringBuilder.Append("  ");
                        stringBuilder.Append(item.Key);
                        stringBuilder.Append(": ");
                        stringBuilder.Append(item2);
                        stringBuilder.Append("\r\n");
                     }
                  }
               }
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
         }
      }

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

         _psc = psc ?? throw new ArgumentNullException(nameof(_psc));
         if (string.IsNullOrEmpty(psc)) { throw new ArgumentException("psc is empty"); }
         _username = username ?? throw new ArgumentNullException(nameof(username));
         if (string.IsNullOrEmpty(username)) { throw new ArgumentException("username is empty"); }
         _password = password ?? throw new ArgumentNullException(nameof(password));
         if (password.Length == 0) { throw new ArgumentException("password is empty"); }

         _psc = psc;
         _username = username;
         _password = password;
         _thumbprint = thumbprint;
         _ignoreServerCertificateValidation = ignoreServerCertificateValidation;
      }

      public IEnumerable<string> GetEncodedTrustedCertificates() {

         var task = Task.Run(GetEncodedTrustedCertificatesAsync);

         try {
            task.Wait();
            return task.Result;
         } catch (AggregateException ex) {
            throw new TrustedCertificateRetrievalException("Getting trusted certificates failed", ex);
         }
      }

      public async Task<IEnumerable<string>> GetEncodedTrustedCertificatesAsync() {
         _logger.LogInformation(
                  string.Format(Resources.PerofomingOperation, Resources.EncodedTrustedCertificatesRetrieval));

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
      }

      private async Task<IEnumerable<string>> GetTrustedCertificateAsync(HttpClient client, string chain) {
         using (var response = await client.GetAsync($"/api/vcenter/certificate-management/vcenter/trusted-root-chains/{Uri.EscapeDataString(chain)}")) {
            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(responseStr);

            List<string> result = new List<string>();

            if (json?.cert_chain?.cert_chain != null &&
               json.cert_chain.cert_chain is IEnumerable &&
               ((IEnumerable) json.cert_chain.cert_chain).Cast<dynamic>().Any()) {

               string cert_chain = json.cert_chain.cert_chain[0];
               int startIndex = 0;

               // Algorithm description:
               // We get the the index of ------END CERTIFICATE----- and then
               // the last index of -----BEGIN CERTIFICATE----- smaller than end index.
               // This way we handle the case where we receive an invalid formated PEM
               // in the format of *-BEGIN- -BEGIN- -END-*

               var endIndex = cert_chain.IndexOf(PEM_END, startIndex);
               var beginIndex = cert_chain.IndexOf(PEM_BEGIN, startIndex);

               // WORKAROUND: LastIndexOf(str, startIndex, count) fails with count must be positive
               // and within the string boundaries. count meets both restrictions but still fails.

               var nextBeginIndex = cert_chain.IndexOf(PEM_BEGIN, beginIndex + 1);

               while(nextBeginIndex < endIndex && nextBeginIndex > -1) {
                  beginIndex = nextBeginIndex;
                  nextBeginIndex = cert_chain.IndexOf(PEM_BEGIN, beginIndex + 1);
               }

               // END WORKAROUND
               
               while (beginIndex > -1 && endIndex > beginIndex) {
                  result.Add(cert_chain.Substring(beginIndex, endIndex - beginIndex + PEM_BEGIN.Length - 2).Trim());

                  startIndex = endIndex + 1;
                  beginIndex = cert_chain.IndexOf(PEM_BEGIN, startIndex);
                  endIndex = cert_chain.IndexOf(PEM_END, startIndex);
               }
            }

            return result;
         }
      }

      private async Task<IEnumerable<string>> GetTrustedRootChainsChainsAsync(HttpClient client) {
         using (var response = await client.GetAsync("/api/vcenter/certificate-management/vcenter/trusted-root-chains")) {
            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject<dynamic>(responseStr);

            if (result is IEnumerable enumerable) {
               return enumerable.Cast<dynamic>().Where(r => r?.chain != null).Select(c => (string) c.chain);
            } else {
               throw new TrustedCertificateRetrievalException("/api/vcenter/certificate-management/vcenter/trusted-root-chains did not return an array");
            }
         } 
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
            clientHandler = _httpMessageHandlerFactory.Invoke(new LoggingHandler(_loggerFactory, clientHandler));
         }

         return new HttpClient(clientHandler) {
            BaseAddress = new Uri($"https://{_psc}")
         };
      }

      private async Task AuthenticateClientAsync(HttpClient client) {
         _logger.LogInformation("Establishing session");

         string authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(_username + ":" + SecureStringToString(_password)));

         using (var request = new HttpRequestMessage(HttpMethod.Post, "/api/session")) {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);
            using (var response = await client.SendAsync(request)) {
               if (response.StatusCode != HttpStatusCode.Created) {
                  throw new TrustedCertificateRetrievalException("Unable to create session for getting CA trusted certificates");
               }

               var sessionId = await response.Content.ReadAsStringAsync();
               sessionId = sessionId?.Trim('"');

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
