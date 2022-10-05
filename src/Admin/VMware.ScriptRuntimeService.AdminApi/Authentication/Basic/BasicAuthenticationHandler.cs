// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication.Basic {
   public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
      public const string AuthenticationScheme = "Basic";

      private readonly ILogger _logger;
      private readonly ILoggerFactory _loggerFactory;

      public BasicAuthenticationHandler(
         IOptionsMonitor<AuthenticationSchemeOptions> options,
         ILoggerFactory loggerFactory,
         UrlEncoder encoder,
         ISystemClock clock)
         : base(options, loggerFactory, encoder, clock) {
         _loggerFactory = loggerFactory;
         _logger = _loggerFactory.CreateLogger(typeof(BasicAuthenticationHandler));
      }

      protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
         return await Task.Run(
         () => {
            AuthenticateResult result = null;
            try {
               _logger.Log(LogLevel.Debug, "Handle Basic Authentication Start");

               var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
               var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
               var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
               var username = credentials[0];
               var password = credentials[1];

               var adminUser = Environment.GetEnvironmentVariable("ADMIN_USER")?.Trim();
               var adminPass = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")?.Trim();
               var adminPassSalt = Environment.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")?.Trim();
               using (SHA256 sha256Hash = SHA256.Create()) {
                  if (!string.IsNullOrEmpty(username) &&
                  !string.IsNullOrEmpty(password) &&
                  !string.IsNullOrEmpty(adminUser) &&
                  !string.IsNullOrEmpty(adminPass) &&
                  !string.IsNullOrEmpty(adminPassSalt) &&
                  CryptographicOperations.FixedTimeEquals(
                     Encoding.UTF8.GetBytes(username),
                     Encoding.UTF8.GetBytes(adminUser)) &&
                  CryptographicOperations.FixedTimeEquals(
                     sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(adminPassSalt + password)),
                     Encoding.UTF8.GetBytes(adminPass))) {

                     // Successful authnetication

                     var claims = new[] {
                        new Claim(ClaimTypes.Name, username),
                     };
                     var identity = new ClaimsIdentity(claims, AuthenticationScheme);
                     var principal = new ClaimsPrincipal(identity);

                     result = AuthenticateResult.Success(
                        new AuthenticationTicket(principal, AuthenticationScheme));
                  } else {

                     // Unsuccessful authentication

                     if (string.IsNullOrEmpty(adminUser) ||
                        string.IsNullOrEmpty(adminPass)) {
                        result = AuthenticateResult.Fail("Script Runtime Service admin credentials are not setted up correctly");
                     } else if ((!username?.Equals(adminUser) ?? true) ||
                        (!password?.Equals(adminPass) ?? true)) {
                        result = AuthenticateResult.Fail("Invalid username or password");
                     }
                  }
               }
            } catch (Exception exc) {
               _logger.Log(LogLevel.Error, $"Basic Authorization failure: {exc}");
               result = AuthenticateResult.Fail(exc.Message);
            }

            return result;
         });
      }

      protected override async Task HandleChallengeAsync(AuthenticationProperties properties) {
         var wwwAuthenticate = Response.Headers["WWW-Authenticate"].FirstOrDefault();
         var basicWwwAuthenticate = $"{AuthenticationScheme} realm=\"SRS endpoint\"";
         if (string.IsNullOrEmpty(wwwAuthenticate)) {
            wwwAuthenticate = basicWwwAuthenticate;
         } else {
            wwwAuthenticate = string.Join(',', basicWwwAuthenticate, wwwAuthenticate);
         }
         Response.Headers["WWW-Authenticate"] = wwwAuthenticate;
         await base.HandleChallengeAsync(properties);
      }
   }
}
