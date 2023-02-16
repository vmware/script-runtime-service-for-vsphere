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
      public void GetEncodedTrustedCertificates_Successfully_TwoCertificatesAreReturned() {
         // Arrange
         var mockedProtected = _msgHandler.Protected();
         var expectedCert = "-----BEGIN CERTIFICATE-----\nMIIFOzCCA6OgAwIBAgIJAOrjMkQMNfJUMA0GCSqGSIb3DQEBCwUAMIGoMQswCQYD\nVQQDDAJDQTEXMBUGCgmSJomT8ixkARkWB3ZzcGhlcmUxFTATBgoJkiaJk/IsZAEZ\nFgVsb2NhbDELMAkGA1UEBhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExKjAoBgNV\nBAoMIXNjMi0xMC0xODUtMjI1LTE5OS5lbmcudm13YXJlLmNvbTEbMBkGA1UECwwS\nVk13YXJlIEVuZ2luZWVyaW5nMB4XDTIzMDIxMDEyMDAzNVoXDTMzMDIwNzEyMDAz\nNVowgagxCzAJBgNVBAMMAkNBMRcwFQYKCZImiZPyLGQBGRYHdnNwaGVyZTEVMBMG\nCgmSJomT8ixkARkWBWxvY2FsMQswCQYDVQQGEwJVUzETMBEGA1UECAwKQ2FsaWZv\ncm5pYTEqMCgGA1UECgwhc2MyLTEwLTE4NS0yMjUtMTk5LmVuZy52bXdhcmUuY29t\nMRswGQYDVQQLDBJWTXdhcmUgRW5naW5lZXJpbmcwggGiMA0GCSqGSIb3DQEBAQUA\nA4IBjwAwggGKAoIBgQDAyDKYVYwiCMZDSAgxHu595pU/W5pVFMv4gA01z2XfrOZY\nSuIQFY2PTtxjMM7p/3+oPw/sWXhO6ig94jguBZVselu/ksW+N2U+2nUJoScdGGio\nfGvXsV0DmJ0nMfdnSpVmY6R1ImIqZkRjonR4qpHGhzRjqjzSL/hU1Djiez+pIaKW\nugQ9QCFO+DVwiZ2OQqNESPOPQHera0bn4qzJSAjjibsyaoI7MCqhC0cM3oIZ8xLK\nq4QoP0fhUlv8fGLSb3gztO9QD3GGGZf/3nb3Xkf4l1TszKljzVMEjJbztnTiNIHj\nkRd1Am0X3DWpQl2yI6qwOU/Jbt7gVVad+7c9lmxnHVrMjX08pNF6A5k+X7MPg3CS\nWVcfs2LX/J41bTfOQpCgWYZnsh7OfSVlIDthfwh3f8MvSHc+JkACCJwVHTxR4uNW\nER48InN6p2/V+1GglliV/fo9ZMW9e6fI5EwQbEBGScOvpzkV59ABsl4SE0M7J8WT\n3AFrg+hO/cZkQACuTL0CAwEAAaNmMGQwHQYDVR0OBBYEFLobb584cG5Pqq+KoKFq\nfh5gxOv+MB8GA1UdEQQYMBaBDmVtYWlsQGFjbWUuY29thwR/AAABMA4GA1UdDwEB\n/wQEAwIBBjASBgNVHRMBAf8ECDAGAQH/AgEAMA0GCSqGSIb3DQEBCwUAA4IBgQCA\nynTFEvpjnBcrV7gjI2GBbVwK4kJqRURWbFMUZO/pgUKgT9pA7f2bOuoVgnLn0suY\nXXTPK0hBRQ5CsPvgUPuq2B3u9rOf25WMlnasDWqG3NPi+8GFsYKRr2nQecClI36q\nPmafcQ+cao3fm5OEXrecZRb5tG313p0YtlYgu9dJRjIw9CcJHrDsNKvj5WMyqrD0\ndHWlzq+DvZSZRLbk+ddn1QaU9D6SXYY+t8rSH+XvFj+RCgNZ1zWi4oxtx22rfMW6\n2dDooR4tRZ4FdtD9OOBTUg8bZs78hgBHD+2Hi5pH+HobO8yYvPs1BHPiAL/7/GCi\nRviQ205ZFw0hbMn8PMOQrHilFA1jx4NxqRlEWSBSOFVrmuB2YftHit77rzwfvHCl\nZ27QNsJu6u+wbGb9IMyipNsWWZ8sr7dAegdm3JkgJ9Ig89BfxsIfGXykuKrVYyx3\nrbhtJpljRHRdB0in5pvu36XCTrUWKcYoEjPSPrAr8MEd6hRqx1AlHglo41lQvNM=\n-----END CERTIFICATE-----";

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
                Content = new StringContent(@"{
   ""cert_chain"": {
      ""cert_chain"": [
        ""-----BEGIN CERTIFICATE-----\nMIIFOzCCA6OgAwIBAgIJAOrjMkQMNfJUMA0GCSqGSIb3DQEBCwUAMIGoMQswCQYD\nVQQDDAJDQTEXMBUGCgmSJomT8ixkARkWB3ZzcGhlcmUxFTATBgoJkiaJk/IsZAEZ\nFgVsb2NhbDELMAkGA1UEBhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExKjAoBgNV\nBAoMIXNjMi0xMC0xODUtMjI1LTE5OS5lbmcudm13YXJlLmNvbTEbMBkGA1UECwwS\nVk13YXJlIEVuZ2luZWVyaW5nMB4XDTIzMDIxMDEyMDAzNVoXDTMzMDIwNzEyMDAz\nNVowgagxCzAJBgNVBAMMAkNBMRcwFQYKCZImiZPyLGQBGRYHdnNwaGVyZTEVMBMG\nCgmSJomT8ixkARkWBWxvY2FsMQswCQYDVQQGEwJVUzETMBEGA1UECAwKQ2FsaWZv\ncm5pYTEqMCgGA1UECgwhc2MyLTEwLTE4NS0yMjUtMTk5LmVuZy52bXdhcmUuY29t\nMRswGQYDVQQLDBJWTXdhcmUgRW5naW5lZXJpbmcwggGiMA0GCSqGSIb3DQEBAQUA\nA4IBjwAwggGKAoIBgQDAyDKYVYwiCMZDSAgxHu595pU/W5pVFMv4gA01z2XfrOZY\nSuIQFY2PTtxjMM7p/3+oPw/sWXhO6ig94jguBZVselu/ksW+N2U+2nUJoScdGGio\nfGvXsV0DmJ0nMfdnSpVmY6R1ImIqZkRjonR4qpHGhzRjqjzSL/hU1Djiez+pIaKW\nugQ9QCFO+DVwiZ2OQqNESPOPQHera0bn4qzJSAjjibsyaoI7MCqhC0cM3oIZ8xLK\nq4QoP0fhUlv8fGLSb3gztO9QD3GGGZf/3nb3Xkf4l1TszKljzVMEjJbztnTiNIHj\nkRd1Am0X3DWpQl2yI6qwOU/Jbt7gVVad+7c9lmxnHVrMjX08pNF6A5k+X7MPg3CS\nWVcfs2LX/J41bTfOQpCgWYZnsh7OfSVlIDthfwh3f8MvSHc+JkACCJwVHTxR4uNW\nER48InN6p2/V+1GglliV/fo9ZMW9e6fI5EwQbEBGScOvpzkV59ABsl4SE0M7J8WT\n3AFrg+hO/cZkQACuTL0CAwEAAaNmMGQwHQYDVR0OBBYEFLobb584cG5Pqq+KoKFq\nfh5gxOv+MB8GA1UdEQQYMBaBDmVtYWlsQGFjbWUuY29thwR/AAABMA4GA1UdDwEB\n/wQEAwIBBjASBgNVHRMBAf8ECDAGAQH/AgEAMA0GCSqGSIb3DQEBCwUAA4IBgQCA\nynTFEvpjnBcrV7gjI2GBbVwK4kJqRURWbFMUZO/pgUKgT9pA7f2bOuoVgnLn0suY\nXXTPK0hBRQ5CsPvgUPuq2B3u9rOf25WMlnasDWqG3NPi+8GFsYKRr2nQecClI36q\nPmafcQ+cao3fm5OEXrecZRb5tG313p0YtlYgu9dJRjIw9CcJHrDsNKvj5WMyqrD0\ndHWlzq+DvZSZRLbk+ddn1QaU9D6SXYY+t8rSH+XvFj+RCgNZ1zWi4oxtx22rfMW6\n2dDooR4tRZ4FdtD9OOBTUg8bZs78hgBHD+2Hi5pH+HobO8yYvPs1BHPiAL/7/GCi\nRviQ205ZFw0hbMn8PMOQrHilFA1jx4NxqRlEWSBSOFVrmuB2YftHit77rzwfvHCl\nZ27QNsJu6u+wbGb9IMyipNsWWZ8sr7dAegdm3JkgJ9Ig89BfxsIfGXykuKrVYyx3\nrbhtJpljRHRdB0in5pvu36XCTrUWKcYoEjPSPrAr8MEd6hRqx1AlHglo41lQvNM=\n-----END CERTIFICATE-----\n-----BEGIN X509 CRL-----\nMIICpDCCAQwCAQEwDQYJKoZIhvcNAQELBQAwgagxCzAJBgNVBAMMAkNBMRcwFQYK\nCZImiZPyLGQBGRYHdnNwaGVyZTEVMBMGCgmSJomT8ixkARkWBWxvY2FsMQswCQYD\nVQQGEwJVUzETMBEGA1UECAwKQ2FsaWZvcm5pYTEqMCgGA1UECgwhc2MyLTEwLTE4\nNS0yMjUtMTk5LmVuZy52bXdhcmUuY29tMRswGQYDVQQLDBJWTXdhcmUgRW5naW5l\nZXJpbmcXDTIzMDIxNTEzNTAzNVoXDTIzMDMxNzEzNTAzNVqgLzAtMAoGA1UdFAQD\nAgELMB8GA1UdIwQYMBaAFLobb584cG5Pqq+KoKFqfh5gxOv+MA0GCSqGSIb3DQEB\nCwUAA4IBgQAGIImskOv8S2F140sGtP/mZ5MossDuks1cfbAC6nwsrPBDcwaoqpnl\n317ixIb3BHoW0b3MQydYivzybYehp//HxWOzqFFu0DJtBxEYbFADbxR33mr2O2hu\n8ANT8zTUgosBlheLqUZSH5DumAtiZa/Q1JKUGCG4QuFoMHi5EPfRK5/qdFeOEz+I\n1OELahB+9pyd6kO8EhzmmhN8UFUwImGJhdAR0NdXE7naBEQv2Y/nKhOk8gbJUEAR\nifDNeVdptKQR0cp+rS7ygscU1tfIntzdd9hkVGpSwuPkOK27iSqxjO9OQW6/x57M\nbJc1t39KmbqiwuDf/anJvzyM4sUVPPw4+ZMZdoqkBPW58eRZdk+x5I6kXQf+ZntC\nEvcSdV0JQGC8mR+j+YD36lfLIkL60payKMwohIhEr7Cj2SMVEJRZv3RgHaWi+e6U\nKkgOV2RbFt5wV5A+CczVJVDKPU4Q0wFsV23HIhURXmEDUz1ZLZ6I2vJaj5eJ80za\nYSCgKPcAsxA=\n-----END X509 CRL-----""
     ]
   }
}
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
         Assert.AreEqual(certs.First(), expectedCert);
      }
   }
}
