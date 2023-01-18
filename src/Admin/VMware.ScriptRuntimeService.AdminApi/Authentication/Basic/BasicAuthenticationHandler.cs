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
using VMware.ScriptRuntimeService.AdminApi;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication.Basic {
   public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
      public const string AuthenticationScheme = "Basic";

      private readonly ILogger _logger;
      private readonly ILoggerFactory _loggerFactory;
      private readonly IEnvironment _environment;

      public BasicAuthenticationHandler(
         IOptionsMonitor<AuthenticationSchemeOptions> options,
         ILoggerFactory loggerFactory,
         UrlEncoder encoder,
         ISystemClock clock,
         IEnvironment environment)
         : base(options, loggerFactory, encoder, clock) {
         _loggerFactory = loggerFactory;
         _environment = environment ?? throw new ArgumentNullException(nameof(environment));
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

               var adminUser = _environment.GetEnvironmentVariable("ADMIN_USER")?.Trim();
               var adminPass = _environment.GetEnvironmentVariable("ADMIN_PASSWORD")?.Trim();
               var adminPassSalt = _environment.GetEnvironmentVariable("ADMIN_PASSWORD_SALT")?.Trim();

               if (!string.IsNullOrEmpty(username) &&
                  !string.IsNullOrEmpty(password) &&
                  !string.IsNullOrEmpty(adminUser) &&
                  !string.IsNullOrEmpty(adminPass) &&
                  !string.IsNullOrEmpty(adminPassSalt) &&
                  FixedTimeEquals(username, adminUser) &&
                  FixedTimeEquals(
                     GetSha256Hash(adminPassSalt + password),
                     adminPass)) {

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
                     result = AuthenticateResult.Fail("Script Runtime Service admin credentials are not set up correctly");
                  } else if ((!username?.Equals(adminUser) ?? true) ||
                     (!password?.Equals(adminPass) ?? true)) {
                     result = AuthenticateResult.Fail("Invalid username or password");
                  }
               }
            } catch (Exception exc) {
               _logger.Log(LogLevel.Error, $"Basic Authorization failure: {exc}");
               result = AuthenticateResult.Fail("Authentication failed, see the administrative log for error details.");
            }

            return result;
         });
      }

      private string GetSha256Hash(string str) {
         using(SHA256 sha = SHA256.Create()) {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(str));
            var strBuilder = new StringBuilder(bytes.Length);
            for (var i = 0; i < bytes.Length; i++) {
               strBuilder.Append(bytes[i].ToString("x2"));
            }
            return strBuilder.ToString();
         }
      }

      private bool FixedTimeEquals(string left, string right) {
         return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(left),
            Encoding.UTF8.GetBytes(right));
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
