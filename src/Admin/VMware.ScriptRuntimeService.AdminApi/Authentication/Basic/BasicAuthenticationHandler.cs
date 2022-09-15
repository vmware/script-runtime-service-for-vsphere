// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VMware.ScriptRuntimeService.Sts.SamlToken;

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

               if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN_USER")) ||
                  string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ADMIN_PASSWORD"))) {
                  result = AuthenticateResult.Fail("Script Runtime Service admin credentials are not setted up correctly");
               } else if ((!username?.Equals(Environment.GetEnvironmentVariable("ADMIN_USER")) ?? true) ||
                  (!password?.Equals(Environment.GetEnvironmentVariable("ADMIN_PASSWORD")) ?? true)) {
                  result = AuthenticateResult.Fail("Invalid username or password");
               } else {
                  var claims = new[] {
                     new Claim(ClaimTypes.Name, username),
                  };
                  var identity = new ClaimsIdentity(claims, AuthenticationScheme);
                  var principal = new ClaimsPrincipal(identity);

                  result = AuthenticateResult.Success(
                     new AuthenticationTicket(principal, AuthenticationScheme));
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
