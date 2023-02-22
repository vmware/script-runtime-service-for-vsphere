// **************************************************************************
//  Copyright 2020-2023 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using NUnit.Framework;
using Moq.Protected;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using static VMware.ScriptRuntimeService.AdminEngine.VCRegistration.VCTrustedCertificatesCollector;

namespace VMware.ScriptRuntimeService.AdminEngine.Tests {
   public class LoggingHandlerTests {
      private readonly Mock<HttpMessageHandler> _msgHandler = new Mock<HttpMessageHandler>();
      private readonly Mock<ILogger<LoggingHandler>> _logger = new Mock<ILogger<LoggingHandler>>();
      private string _expectedResponseMessage;

      private HttpClient _httpClient;

      private readonly List<string> _loggedMessages = new List<string>();

      public LoggingHandlerTests() {
      }

      [SetUp]
      public void Setup() {

         _msgHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage() {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent("Response content"),
               Headers = {
                  { "vmware-api-session-id", new [] { "session-id" } }
               }
            });
         _expectedResponseMessage = "Response: StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: Response content, Headers:\r\n{\r\n  vmware-api-session-id: session-id\r\n  Content-Type: text/plain; charset=utf-8\r\n}";

         _logger.Setup(logger => logger.Log(
            It.Is<LogLevel>(logLevel => logLevel == LogLevel.Debug),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>())).
            Callback<LogLevel, EventId, object, Exception, object>((lvl, eventId, msg, ex, format) => {
               _loggedMessages.Add(msg.ToString());
            });


         var loggingHandler = new LoggingHandler(new NullLoggerFactory(), _logger.Object, _msgHandler.Object);
         _httpClient = new HttpClient(loggingHandler);
         _loggedMessages.Clear();
      }

      [TearDown]
      public void TearDown() {
         _httpClient.Dispose();
      }

      [Test]
      public void SendAsync_LogRequestAndResponse_Successfully() {
         // Arrange
         var request = new HttpRequestMessage(HttpMethod.Post, "https://dummy-address.com/") {
            Headers = {
               { "Authorization", new[] { "Basic BASE64" } },
               { "vmware-api-session-id", new[] { "SESSION_ID" } }
            },
            Content = new StringContent(@"{
   ""prop"": ""value"",
   ""prop2"": ""value2""
}")
         };

         string expectedRequestMessage = "Request: Method: POST, RequestUri: 'https://dummy-address.com/', Version: 1.1, Content: {\r\n   \"prop\": \"value\",\r\n   \"prop2\": \"value2\"\r\n}, Headers:\r\n{\r\n  Authorization: sanitized\r\n  vmware-api-session-id: SESSION_ID\r\n  Content-Type: text/plain; charset=utf-8\r\n}";

         // Act
         _httpClient.SendAsync(request).Wait();

         // Assert
         CollectionAssert.IsNotEmpty(_loggedMessages);
         CollectionAssert.AreEquivalent(new[] { expectedRequestMessage, _expectedResponseMessage }, _loggedMessages);
      }

      [Test]
      public void SendAsync_LogRequestAndResponse_NoHeaders() {
         // Arrange
         var request = new HttpRequestMessage(HttpMethod.Post, "https://dummy-address.com/");

         string expectedRequestMessage = "Request: Method: POST, RequestUri: 'https://dummy-address.com/', Version: 1.1, Content: <null>, Headers:\r\n{\r\n}";

         // Act
         _httpClient.SendAsync(request).Wait();

         // Assert
         CollectionAssert.IsNotEmpty(_loggedMessages);
         CollectionAssert.AreEquivalent(new[] { expectedRequestMessage, _expectedResponseMessage }, _loggedMessages);
      }

      [Test]
      public void SendAsync_LogRequestAndResponse_ByteContent() {
         // Arrange
         var request = new HttpRequestMessage(HttpMethod.Post, "https://dummy-address.com/") {
            Content = new ByteArrayContent(new byte[] { 0x20, 0x21, 0x30, 0x31, 0x40, 0x41, 0x50, 0x51 })
         };

         string expectedRequestMessage = "Request: Method: POST, RequestUri: 'https://dummy-address.com/', Version: 1.1, Content:  !01@APQ, Headers:\r\n{\r\n}";

         // Act
         _httpClient.SendAsync(request).Wait();

         // Assert
         CollectionAssert.IsNotEmpty(_loggedMessages);
         CollectionAssert.AreEquivalent(new[] { expectedRequestMessage, _expectedResponseMessage }, _loggedMessages);
      }

   }
}
