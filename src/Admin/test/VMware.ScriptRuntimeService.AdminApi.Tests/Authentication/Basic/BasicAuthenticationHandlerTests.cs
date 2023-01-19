// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;
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
         // Arrange

         // Act
         var result = await AuthenticateAsync();

         // Assert
         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("No Authorization header sent", result.Failure.Message);
      }

      [Test]
      public async Task HandleAuthenticateAsync_CredentialsTryParseFails_ReturnsAuthenticateResultFail() {
         // Arrange
         var context = new DefaultHttpContext();
         var username = "administrator@vsphere.local";
         var password = "A:dmin!23";
         var header = FormatAuthorizationHeader(username, password);

         // Act
         var result = await AuthenticateAsync(header);

         // Assert
         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Invalid Authorization header format", result.Failure.Message);
      }


      [Test]
      public async Task HandleAuthenticateAsync_AuthenticationNotSetup_ReturnsAuthenticateResultFail() {
         // Arrange
         var username = "administrator@vsphere.local";
         var password = "Admin!23";
         var header = FormatAuthorizationHeader(username, password);

         // Act
         var result = await AuthenticateAsync(header);

         // Assert
         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Script Runtime Service admin credentials are not set up correctly", result.Failure.Message);
      }

      [Test]
      public async Task HandleAuthenticateAsync_InvalidUsernameOrPassword_ReturnsAuthenticateResultFail() {
         // Arrange
         var username = "administrator@vsphere.local";
         var password = "Wrong-Password";
         var header = FormatAuthorizationHeader(username, password);

         _environment.Reset();
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_USER")).Returns(username);
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD")).Returns("078addd7eb7a3548be13e76d7abb62be96c5804c1a8428f4adfe3c4e6ac73e2e");
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")).Returns("g9H6g+AGZOY/uA8+");

         // Act
         var result = await AuthenticateAsync(header);

         // Assert
         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Invalid username or password", result.Failure.Message);
      }

      [Test]
      public async Task HandleAuthenticateAsync_UseSaltedPasswordDirectly_ReturnsAuthenticateResultSuccess() {
         // Arrange
         var username = "administrator@vsphere.local";
         var password = "Admin!23";
         var salt = "g9H6g+AGZOY/uA8+";
         var saltedPassword = salt + password;
         var header = FormatAuthorizationHeader(username, saltedPassword);

         _environment.Reset();
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_USER")).Returns(username);
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD")).Returns(GetSha256Hash(saltedPassword));
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")).Returns(salt);

         // Act
         var result = await AuthenticateAsync(header);

         // Assert
         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Invalid username or password", result.Failure.Message);
      }

      [Test]
      public async Task HandleAuthenticateAsync_UseSaltedHashedPasswordDirectly_ReturnsAuthenticateResultSuccess() {
         // Arrange
         var username = "administrator@vsphere.local";
         var password = "Admin!23";
         var salt = "g9H6g+AGZOY/uA8+";
         var saltedPassword = GetSha256Hash(salt + password);
         var header = FormatAuthorizationHeader(username, saltedPassword);

         _environment.Reset();
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_USER")).Returns(username);
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD")).Returns(saltedPassword);
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")).Returns(salt);

         // Act
         var result = await AuthenticateAsync(header);

         // Assert
         Assert.IsFalse(result.Succeeded);
         Assert.AreEqual("Invalid username or password", result.Failure.Message);
      }

      [Test]
      public async Task HandleAuthenticateAsync_ReturnsAuthenticateResultSuccess() {
         // Arrange
         var username = "administrator@vsphere.local";
         var password = "Admin!23";
         var header = FormatAuthorizationHeader(username, password);

         _environment.Reset();
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_USER")).Returns(username);
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD")).Returns("078addd7eb7a3548be13e76d7abb62be96c5804c1a8428f4adfe3c4e6ac73e2e");
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")).Returns("g9H6g+AGZOY/uA8+");

         // Act
         var result = await AuthenticateAsync(header);

         // Assert
         Assert.IsTrue(result.Succeeded);
         Assert.AreEqual(BasicAuthenticationHandler.AuthenticationScheme, result.Ticket.AuthenticationScheme);
         Assert.AreEqual(username, result.Ticket.Principal.Identity.Name);
      }

      [Test]
      public async Task HandleAuthenticateAsync_CalculateSaltedPassword_ReturnsAuthenticateResultSuccess() {
         // Arrange
         var username = "administrator@vsphere.local";
         var password = "Admin!23";
         var header = FormatAuthorizationHeader(username, password);
         var salt = "g9H6g+AGZOY/uA8+";

         _environment.Reset();
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_USER")).Returns(username);
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD")).Returns(GetSha256Hash(salt + password));
         _environment.Setup(e => e.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")).Returns(salt);

         // Act
         var result = await AuthenticateAsync(header);

         // Assert
         Assert.IsTrue(result.Succeeded);
         Assert.AreEqual(BasicAuthenticationHandler.AuthenticationScheme, result.Ticket.AuthenticationScheme);
         Assert.AreEqual(username, result.Ticket.Principal.Identity.Name);
      }

      private Task<AuthenticateResult> AuthenticateAsync() {
         var context = new DefaultHttpContext();

         return AuthenticateAsync(context);
      }

      private async Task<AuthenticateResult> AuthenticateAsync(DefaultHttpContext context) {
         await _handler.InitializeAsync(new AuthenticationScheme(BasicAuthenticationHandler.AuthenticationScheme, null, typeof(BasicAuthenticationHandler)), context);

         return await _handler.AuthenticateAsync();
      }

      private Task<AuthenticateResult> AuthenticateAsync(string authorizationHeaderValue) {
         var context = new DefaultHttpContext();
         var authorizationHeader = new StringValues(authorizationHeaderValue);
         context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

         return AuthenticateAsync(context);
      }

      private string FormatAuthorizationHeader(string username, string password) {
         return "Basic " + Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes($"{username}:{password}"));
      }

      private string GetSha256Hash(string str) {
         using (SHA256 sha = SHA256.Create()) {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(str));
            var strBuilder = new StringBuilder(bytes.Length);
            for (var i = 0; i < bytes.Length; i++) {
               strBuilder.Append(bytes[i].ToString("x2"));
            }
            return strBuilder.ToString();
         }
      }
   }
}
