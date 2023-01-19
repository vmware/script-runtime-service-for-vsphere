// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using VMware.ScriptRuntimeService.APIGateway.Authentication.Basic;

namespace VMware.ScriptRuntimeService.AdminApi.Tests.Authentication.Basic {
   internal class BasicAuthenticationHandlerTests {

      private Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _options;
      private Mock<ILoggerFactory> _loggerFactory;
      private Mock<UrlEncoder> _encoder;
      private Mock<ISystemClock> _clock;
      private BasicAuthenticationHandler _handler;
      private Mock<IEnvironment> _environment;

      [SetUp]
      public void Setup() {
         _environment = new Mock<IEnvironment>();
         _options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();

         // This Setup is required for .NET Core 3.1 onwards.
         _options
             .Setup(x => x.Get(It.IsAny<string>()))
             .Returns(new AuthenticationSchemeOptions());

         var logger = new Mock<ILogger<BasicAuthenticationHandler>>();
         _loggerFactory = new Mock<ILoggerFactory>();
         _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(logger.Object);

         _encoder = new Mock<UrlEncoder>();
         _clock = new Mock<ISystemClock>();

         _handler = new BasicAuthenticationHandler(_options.Object, _loggerFactory.Object, _encoder.Object, _clock.Object, _environment.Object);

      }

      [Test]
      public async Task HandleAuthenticateAsync_NoAuthorizationHeader_ReturnsAuthenticateResultFail() {
         var context = new DefaultHttpContext();

         await _handler.InitializeAsync(new AuthenticationScheme(BasicAuthenticationHandler.AuthenticationScheme, null, typeof(BasicAuthenticationHandler)), context);
         var result = await _handler.AuthenticateAsync();

         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("No Authorization header sent", result.Failure.Message);
      }

      [Test]
      public async Task HandleAuthenticateAsync_CredentialsTryParseFails_ReturnsAuthenticateResultFail() {
         var context = new DefaultHttpContext();
         var authorizationHeader = new StringValues("Basic YWRtaW5pc3RyYXRvckB2c3BoZXJlLmxvY2FsOkE6ZG1pbiEyMw==");
         context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

         await _handler.InitializeAsync(new AuthenticationScheme(BasicAuthenticationHandler.AuthenticationScheme, null, typeof(BasicAuthenticationHandler)), context);
         var result = await _handler.AuthenticateAsync();

         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Invalid Authorization header format", result.Failure.Message);
      }


      [Test]
      public async Task HandleAuthenticateAsync_AuthenticationNotSetup_ReturnsAuthenticateResultFail() {
         var context = new DefaultHttpContext();
         var authorizationHeader = new StringValues("Basic VGVzdFVzZXJOYW1lOlRlc3RQYXNzd29yZA==");
         context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

         await _handler.InitializeAsync(new AuthenticationScheme(BasicAuthenticationHandler.AuthenticationScheme, null, typeof(BasicAuthenticationHandler)), context);
         var result = await _handler.AuthenticateAsync();

         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Script Runtime Service admin credentials are not set up correctly", result.Failure.Message);
      }

      [Test]
      public async Task HandleAuthenticateAsync_InvalidUsernameOrPassword_ReturnsAuthenticateResultFail() {
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_USER")).Returns("administrator@vsphere.local");
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD")).Returns("078addd7eb7a3548be13e76d7abb62be96c5804c1a8428f4adfe3c4e6ac73e2e");
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")).Returns("g9H6g+AGZOY/uA8+");

         var context = new DefaultHttpContext();
         var authorizationHeader = new StringValues("Basic YWRtaW5pc3RyYXRvckB2c3BoZXJlLmxvY2FsOkFEZG1pbiEyMw==");
         context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

         await _handler.InitializeAsync(new AuthenticationScheme(BasicAuthenticationHandler.AuthenticationScheme, null, typeof(BasicAuthenticationHandler)), context);
         var result = await _handler.AuthenticateAsync();

         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Invalid username or password", result.Failure.Message);
      }
      
      [Test]
      public async Task HandleAuthenticateAsync_ReturnsAuthenticateResultSuccess() {
         var username = "administrator@vsphere.local";

         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_USER")).Returns("administrator@vsphere.local");
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD")).Returns("078addd7eb7a3548be13e76d7abb62be96c5804c1a8428f4adfe3c4e6ac73e2e");
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")).Returns("g9H6g+AGZOY/uA8+");

         var context = new DefaultHttpContext();
         var authorizationHeader = new StringValues("Basic YWRtaW5pc3RyYXRvckB2c3BoZXJlLmxvY2FsOkFkbWluITIz");
         context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

         await _handler.InitializeAsync(new AuthenticationScheme(BasicAuthenticationHandler.AuthenticationScheme, null, typeof(BasicAuthenticationHandler)), context);
         var result = await _handler.AuthenticateAsync();

         Assert.IsTrue(result.Succeeded);
         Assert.AreEqual(BasicAuthenticationHandler.AuthenticationScheme, result.Ticket.AuthenticationScheme);
         Assert.AreEqual(username, result.Ticket.Principal.Identity.Name);
      }
   }
}
