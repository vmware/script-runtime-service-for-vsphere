// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.Sts;
using VMware.ScriptRuntimeService.APIGateway.Sts.Impl;
using VMware.ScriptRuntimeService.Sts.SamlToken;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication.Basic {
   public class BasicAuthenticationHandler : AuthenticationHandler<StsSettings> {
      public const string AuthenticationScheme = "Basic";

      private readonly ILogger _logger;
      private readonly ILoggerFactory _loggerFactory;

      public BasicAuthenticationHandler(
         IOptionsMonitor<StsSettings> options,
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
               SecureString password = new SecureString();
               foreach (char c in credentials[1]) {
                  password.AppendChar(c);
               }

               _logger.Log(LogLevel.Debug, "Issue solution HoK token for the user authorized with credentials");
               _logger.Log(LogLevel.Debug, $"STS Settings SolutionOwnerId: {Options.SolutionOwnerId}");
               _logger.Log(LogLevel.Debug, $"STS Settings SolutionServiceId: {Options.SolutionServiceId}");
               _logger.Log(LogLevel.Debug, $"STS Settings SolutionUserSigningCertificatePath: {Options.SolutionUserSigningCertificatePath}");
               _logger.Log(LogLevel.Debug, $"STS Settings Realm: {Options.Realm}");
               var sessionHoKToken = new SolutionStsClient(_loggerFactory, Options)
                  .IssueSolutionTokenByUserCredential(
                     username,
                     password);

               // Issue session for the subject of the SAML token
               var sessionToken = SessionToken.Issue(new SamlToken(sessionHoKToken.OuterXml));
               
               Response.Headers.Add(APIGatewayResources.SRSAuthorizationHeader, sessionToken.SessionId);
               sessionToken.HoKSamlToken = new SamlToken(sessionHoKToken.OuterXml);

               var claims = new[] {
                  new Claim(ClaimTypes.Name, sessionToken.UserName),
               };
               var identity = new ClaimsIdentity(claims, AuthenticationScheme);
               var principal = new ClaimsPrincipal(identity);

               result = AuthenticateResult.Success(
                  new AuthenticationTicket(principal, AuthenticationScheme));
               
            } catch (Exception exc) {
               _logger.LogError(exc, "Basic Authorization failure");
               try {
                  _logger.LogTrace($"Failed Authorization header '{Request.Headers["Authorization"]}'");
               } catch { }
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
