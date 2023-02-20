using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NUnit.Framework;
using Moq.Protected;
using Moq;
using Moq.Language.Flow;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration;
using System.Security;
using Microsoft.Extensions.Logging.Abstractions;
using VMware.ScriptRuntimeService.AdminEngine.VCRegistration.Exceptions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace VMware.ScriptRuntimeService.AdminEngine.Tests {
   public class VCTrustedCertificatesCollectorTests {
      private readonly Mock<HttpMessageHandler> _msgHandler = new Mock<HttpMessageHandler>();

      private const string _baseAddress = "https://localhost";
      private const string _host = "localhost";
      private const string _username = "administrator@vsphere.local";
      private const string _passwordStr = "P@ssword!234";
      private const string _thumbprint = "TH UM BP IN TT";
      private const string _validSessionKey = "valid-sesion-key";
      private readonly SecureString _password = new SecureString();

      private const string _cert1 = "cert1";
      private const string _cert2 = "cert2";
      private const string _pemBegin = "-----BEGIN CERTIFICATE-----";
      private const string _pemEnd = "-----END CERTIFICATE-----";
      private readonly string _certPem1 = $"{_pemBegin}\n{_cert1}\n{_pemEnd}";
      private readonly string _certPem2 = $"{_pemBegin}\n{_cert2}\n{_pemEnd}";
      private const string _junk1 = "-----BEGIN RANDOM JUNK -----\nrandom-junk\n-----END RANDOM JUNK-----";
      private const string _junk2 = "-----BEGIN RANDOM JUNK -----\nother-random-junk\n-----END RANDOM JUNK-----";

      public VCTrustedCertificatesCollectorTests() {
         Array.ForEach(_passwordStr.ToCharArray(), _password.AppendChar);
      }

      [SetUp]
      public void Setup() {
         
      }

      private ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupCreateSessionRequest(IProtectedMock<HttpMessageHandler> mockedProtected = null) {
         if (null == mockedProtected) {
            mockedProtected = _msgHandler.Protected();
         }
         return mockedProtected.Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.Is<HttpRequestMessage>(
                m => 
                  m.RequestUri!.Equals(_baseAddress + "/api/session")),
             ItExpr.IsAny<CancellationToken>());
      }

      private Func<HttpMessageHandler, HttpMessageHandler> SetupInvalidSessionResponse(ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setupApiRequest) {
         var apiMockedResponse = setupApiRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("")
             });

         return (handler) => _msgHandler.Object;
      }
      
      private Func<HttpMessageHandler, HttpMessageHandler> SetupEmptySessionResponse(ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setupApiRequest) {
         var apiMockedResponse = setupApiRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent("")
             });

         return (handler) => _msgHandler.Object;
      }

      private Func<HttpMessageHandler, HttpMessageHandler> SetupValidSessionResponse(ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setupApiRequest) {
         var apiMockedResponse = setupApiRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent($"\"{_validSessionKey}\"")
             });

         return (handler) => _msgHandler.Object;
      }

      [Test]
      public void GetEncodedTrustedCertificates_UnableToCreateSession_ExceptionIsThrown() {
         // Arrange
         ISetup<HttpMessageHandler, Task<HttpResponseMessage>> request = SetupCreateSessionRequest();
         HttpRequestMessage requestMessage = null;
         request.Callback<HttpRequestMessage, CancellationToken>((m, _) => requestMessage = m);
         var handlerFactory = SetupInvalidSessionResponse(request);

         var collector = new VCTrustedCertificatesCollector(
            handlerFactory,
            new NullLoggerFactory(),
            _host,
            _username,
            _password,
            _thumbprint,
            false);

         // Act
         var ex = Assert.Throws<TrustedCertificateRetrievalException>(() => collector.GetEncodedTrustedCertificates(), "Did not throw on invalid session creation");

         //Assert request
         Assert.IsNotNull(requestMessage, "Request message is null");
         Assert.IsNotNull(requestMessage.Headers.Authorization, "No AuthorizationHeader is present");
         Assert.AreEqual(requestMessage.Headers.Authorization.Scheme, "Basic", "Authorization scheme is not Basic");
         Assert.AreEqual(requestMessage.Headers.Authorization.Parameter, "YWRtaW5pc3RyYXRvckB2c3BoZXJlLmxvY2FsOlBAc3N3b3JkITIzNA==", "Unexpected Authorization value");
         Assert.AreEqual(requestMessage.Method, HttpMethod.Post);

         // Assert
         Assert.That(ex.InnerException, Is.TypeOf<AggregateException>(), "InnerException is not AggregateException");

         AggregateException aggEx = ex.InnerException as AggregateException;
         Assert.That(aggEx.InnerExceptions.Count == 1, "More than one exceptions inside the AggregateException");

         Assert.That(aggEx.InnerExceptions[0], Is.TypeOf<TrustedCertificateRetrievalException>(), "The exception inside AggregateException is not TrustedCertificateRetrievalException");
         Assert.That(aggEx.InnerExceptions[0].Message == "Unable to create session for getting CA trusted certificates", "Not expected error message something went wrong");
      }

      [Test]
      public void GetEncodedTrustedCertificates_EmptySessionFromServer_ExceptionIsThrown() {
         // Arrange
         var request = SetupCreateSessionRequest();
         HttpRequestMessage requestMessage = null;
         request.Callback<HttpRequestMessage, CancellationToken>((m, _) => requestMessage = m);
         var handlerFactory = SetupEmptySessionResponse(request);
         var collector = new VCTrustedCertificatesCollector(
            handlerFactory,
            new NullLoggerFactory(),
            _host,
            _username,
            _password,
            _thumbprint,
            false);

         // Act
         var ex = Assert.Throws<TrustedCertificateRetrievalException>(() => collector.GetEncodedTrustedCertificates(), "Did not throw on invalid session creation");

         //Assert request
         Assert.IsNotNull(requestMessage, "Request message is null");
         Assert.IsNotNull(requestMessage.Headers.Authorization, "No AuthorizationHeader is present");
         Assert.AreEqual(requestMessage.Headers.Authorization.Scheme, "Basic", "Authorization scheme is not Basic");
         Assert.AreEqual(requestMessage.Headers.Authorization.Parameter, "YWRtaW5pc3RyYXRvckB2c3BoZXJlLmxvY2FsOlBAc3N3b3JkITIzNA==", "Unexpected Authorization value");
         Assert.AreEqual(requestMessage.Method, HttpMethod.Post);

         // Assert
         Assert.That(ex.InnerException, Is.TypeOf<AggregateException>(), "InnerException is not AggregateException");

         AggregateException aggEx = ex.InnerException as AggregateException;
         Assert.That(aggEx.InnerExceptions.Count == 1, "More than one exceptions inside the AggregateException");

         Assert.That(aggEx.InnerExceptions[0], Is.TypeOf<TrustedCertificateRetrievalException>(), "The exception inside AggregateException is not TrustedCertificateRetrievalException");
         Assert.That(aggEx.InnerExceptions[0].Message == "Server returned empty session id for getting CA trusted certificates", "Not expected error message something went wrong");
      }

      [Test]
      public void GetEncodedTrustedCertificates_Successfully_OneCertificatesIsReturned_MultipleInvalidCertificatesAreSkipped() {
         // Arrange
         var mockedProtected = _msgHandler.Protected();
         var expectedCert = _certPem1;

         HttpRequestMessage listCertsMessage = null;
         var listCertsRequest = mockedProtected.Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.Is<HttpRequestMessage>(
                m =>
                  m.RequestUri!.Equals(_baseAddress + "/api/vcenter/certificate-management/vcenter/trusted-root-chains")),
             ItExpr.IsAny<CancellationToken>());
         listCertsRequest.Callback<HttpRequestMessage, CancellationToken>((m, _) => listCertsMessage = m);

         var listCertsResponse = listCertsRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"[
   {
      ""chain"": ""BA1B6F9F38706E4FAAAF8AA0A16A7E1E60C4EBFE""
   }
]
")
             });

         HttpRequestMessage getCertsMessage = null;
         var getCertsRequest = mockedProtected.Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.Is<HttpRequestMessage>(
                m =>
                  m.RequestUri!.Equals(_baseAddress + "/api/vcenter/certificate-management/vcenter/trusted-root-chains/BA1B6F9F38706E4FAAAF8AA0A16A7E1E60C4EBFE")),
             ItExpr.IsAny<CancellationToken>());
         getCertsRequest.Callback<HttpRequestMessage, CancellationToken>((m, _) => getCertsMessage = m);

         var getCertsResponse = getCertsRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@$"{{
   ""cert_chain"": {{
      ""cert_chain"": [
        ""{_pemBegin}junk{_certPem1}junk{_pemEnd}junk{_junk1}junk{_pemEnd}""
     ]
   }}
}}
")
             });

         var request = SetupCreateSessionRequest(mockedProtected);
         var handlerFactory = SetupValidSessionResponse(request);
         var collector = new VCTrustedCertificatesCollector(
            handlerFactory,
            new NullLoggerFactory(),
            _host,
            _username,
            _password,
            _thumbprint,
            false);

         // Act
         var certs = collector.GetEncodedTrustedCertificates();

         // Assert requests
         Assert.IsNotNull(listCertsMessage, "List certificates request is null");
         Assert.AreEqual(listCertsMessage.Method, HttpMethod.Get, "List certificates request was not a GET request");
         Assert.IsTrue(listCertsMessage.Headers.Contains("vmware-api-session-id"), "vmware-api-session-id header was missing");
         Assert.AreEqual(listCertsMessage.Headers.GetValues("vmware-api-session-id").First(), _validSessionKey, "vmware-api-session-id had an invalid value");

         Assert.IsNotNull(getCertsMessage, "Get certificate request is null");
         Assert.AreEqual(getCertsMessage.Method, HttpMethod.Get, "Get certificate request was not a GET request");
         Assert.IsTrue(getCertsMessage.Headers.Contains("vmware-api-session-id"), "vmware-api-session-id header was missing");
         Assert.AreEqual(getCertsMessage.Headers.GetValues("vmware-api-session-id").First(), _validSessionKey, "vmware-api-session-id had an invalid value");

         // Assert
         Assert.IsNotNull(certs);
         CollectionAssert.IsNotEmpty(certs);
         Assert.AreEqual(certs.Count(), 1);
         Assert.AreEqual(expectedCert, certs.First());
      }

      [Test]
      public void GetEncodedTrustedCertificates_Successfully_OneCertificatesIsReturned() {
         // Arrange
         var mockedProtected = _msgHandler.Protected();
         var expectedCert = _certPem1;

         HttpRequestMessage listCertsMessage = null;
         var listCertsRequest = mockedProtected.Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.Is<HttpRequestMessage>(
                m =>
                  m.RequestUri!.Equals(_baseAddress + "/api/vcenter/certificate-management/vcenter/trusted-root-chains")),
             ItExpr.IsAny<CancellationToken>());
         listCertsRequest.Callback<HttpRequestMessage, CancellationToken>((m, _) => listCertsMessage = m);

         var listCertsResponse = listCertsRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"[
   {
      ""chain"": ""BA1B6F9F38706E4FAAAF8AA0A16A7E1E60C4EBFE""
   }
]
")
             });

         HttpRequestMessage getCertsMessage = null;
         var getCertsRequest = mockedProtected.Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.Is<HttpRequestMessage>(
                m =>
                  m.RequestUri!.Equals(_baseAddress + "/api/vcenter/certificate-management/vcenter/trusted-root-chains/BA1B6F9F38706E4FAAAF8AA0A16A7E1E60C4EBFE")),
             ItExpr.IsAny<CancellationToken>());
         getCertsRequest.Callback<HttpRequestMessage, CancellationToken>((m, _) => getCertsMessage = m);

         var getCertsResponse = getCertsRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@$"{{
   ""cert_chain"": {{
      ""cert_chain"": [
        ""{_certPem1}{_junk1}""
     ]
   }}
}}
")
             });

         var request = SetupCreateSessionRequest(mockedProtected);
         var handlerFactory = SetupValidSessionResponse(request);
         var collector = new VCTrustedCertificatesCollector(
            handlerFactory,
            new NullLoggerFactory(),
            _host,
            _username,
            _password,
            _thumbprint,
            false);

         // Act
         var certs = collector.GetEncodedTrustedCertificates();

         // Assert requests
         Assert.IsNotNull(listCertsMessage, "List certificates request is null");
         Assert.AreEqual(listCertsMessage.Method, HttpMethod.Get, "List certificates request was not a GET request");
         Assert.IsTrue(listCertsMessage.Headers.Contains("vmware-api-session-id"), "vmware-api-session-id header was missing");
         Assert.AreEqual(listCertsMessage.Headers.GetValues("vmware-api-session-id").First(), _validSessionKey, "vmware-api-session-id had an invalid value");
         
         Assert.IsNotNull(getCertsMessage, "Get certificate request is null");
         Assert.AreEqual(getCertsMessage.Method, HttpMethod.Get, "Get certificate request was not a GET request");
         Assert.IsTrue(getCertsMessage.Headers.Contains("vmware-api-session-id"), "vmware-api-session-id header was missing");
         Assert.AreEqual(getCertsMessage.Headers.GetValues("vmware-api-session-id").First(), _validSessionKey, "vmware-api-session-id had an invalid value");

         // Assert
         Assert.IsNotNull(certs);
         CollectionAssert.IsNotEmpty(certs);
         Assert.AreEqual(certs.Count(), 1);
         Assert.AreEqual(expectedCert, certs.First());
      }

      [Test]
      public void GetEncodedTrustedCertificates_Successfully_TwoCertificatesAreReturned() {
         // Arrange
         var mockedProtected = _msgHandler.Protected();
         var expectedCert1 = _certPem1;
         var expectedCert2 = _certPem2;

         HttpRequestMessage listCertsMessage = null;
         var listCertsRequest = mockedProtected.Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.Is<HttpRequestMessage>(
                m =>
                  m.RequestUri!.Equals(_baseAddress + "/api/vcenter/certificate-management/vcenter/trusted-root-chains")),
             ItExpr.IsAny<CancellationToken>());
         listCertsRequest.Callback<HttpRequestMessage, CancellationToken>((m, _) => listCertsMessage = m);

         var listCertsResponse = listCertsRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"[
   {
      ""chain"": ""BA1B6F9F38706E4FAAAF8AA0A16A7E1E60C4EBFE""
   }
]
")
             });

         HttpRequestMessage getCertsMessage = null;
         var getCertsRequest = mockedProtected.Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.Is<HttpRequestMessage>(
                m =>
                  m.RequestUri!.Equals(_baseAddress + "/api/vcenter/certificate-management/vcenter/trusted-root-chains/BA1B6F9F38706E4FAAAF8AA0A16A7E1E60C4EBFE")),
             ItExpr.IsAny<CancellationToken>());
         getCertsRequest.Callback<HttpRequestMessage, CancellationToken>((m, _) => getCertsMessage = m);

         var getCertsResponse = getCertsRequest
             .ReturnsAsync(new HttpResponseMessage() {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@$"{{
   ""cert_chain"": {{
      ""cert_chain"": [
        ""{_certPem1}{_junk1}{_certPem2}{_junk2}""
     ]
   }}
}}
")
             });

         var request = SetupCreateSessionRequest(mockedProtected);
         var handlerFactory = SetupValidSessionResponse(request);
         var collector = new VCTrustedCertificatesCollector(
            handlerFactory,
            new NullLoggerFactory(),
            _host,
            _username,
            _password,
            _thumbprint,
            false);

         // Act
         var certs = collector.GetEncodedTrustedCertificates();

         // Assert requests
         Assert.IsNotNull(listCertsMessage, "List certificates request is null");
         Assert.AreEqual(listCertsMessage.Method, HttpMethod.Get, "List certificates request was not a GET request");
         Assert.IsTrue(listCertsMessage.Headers.Contains("vmware-api-session-id"), "vmware-api-session-id header was missing");
         Assert.AreEqual(listCertsMessage.Headers.GetValues("vmware-api-session-id").First(), _validSessionKey, "vmware-api-session-id had an invalid value");
         
         Assert.IsNotNull(getCertsMessage, "Get certificate request is null");
         Assert.AreEqual(getCertsMessage.Method, HttpMethod.Get, "Get certificate request was not a GET request");
         Assert.IsTrue(getCertsMessage.Headers.Contains("vmware-api-session-id"), "vmware-api-session-id header was missing");
         Assert.AreEqual(getCertsMessage.Headers.GetValues("vmware-api-session-id").First(), _validSessionKey, "vmware-api-session-id had an invalid value");

         // Assert
         Assert.IsNotNull(certs);
         CollectionAssert.IsNotEmpty(certs);
         // Assert.AreEqual(certs.Count(), 1);
         CollectionAssert.AreEquivalent(new[] { expectedCert1, expectedCert2 }, certs);
      }
   }
}
