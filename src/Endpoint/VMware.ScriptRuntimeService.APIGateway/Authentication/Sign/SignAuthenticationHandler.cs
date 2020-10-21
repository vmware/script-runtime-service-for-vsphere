// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VMware.Http.Sso.Authentication;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.Sts;
using VMware.ScriptRuntimeService.APIGateway.Sts.Impl;
using VMware.ScriptRuntimeService.Sts.SamlToken;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication.Sign {
   public class SignAuthenticationHandler : AuthenticationHandler<StsSettings> {
      private ILogger _logger;
      private ILoggerFactory _loggerFactory;

      public SignAuthenticationHandler(
         IOptionsMonitor<StsSettings> options,
         ILoggerFactory loggerFactory,
         UrlEncoder encoder,
         ISystemClock clock)
         : base(options, loggerFactory, encoder, clock) {
         _loggerFactory = loggerFactory;
         _logger = _loggerFactory.CreateLogger(typeof(SignAuthenticationHandler));
      }

      protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
         // Allow Synchornous IO for this request to be able to read Request Body Stream
         // which is part of the SIGN authentication validation
         var syncIOFeature = Context.Features.Get<IHttpBodyControlFeature>();
         if (syncIOFeature != null) {
            syncIOFeature.AllowSynchronousIO = true;
         }
         return await Task.Run(
         () => {
            AuthenticateResult result = null;
            try {
               _logger.Log(LogLevel.Debug, "Handle SIGN Authentication Start");
               // Authorization Header should conform https://wiki.eng.vmware.com/SSO/REST
               var authZValue = Request.Headers["Authorization"].FirstOrDefault();

               _logger.Log(LogLevel.Debug, $"Authorization header value: {authZValue}");

               // Service support multiple Authentication header, check 
               // this handler should handle current one
               if (!string.IsNullOrEmpty(authZValue) && authZValue.StartsWith(Constants.SignAuthenticationScheme)) {

                  List<string> signAuthzTokens = new List<string>();

                  // Get SIGN Authnetication token.
                  // Depending on the length of the SAML token SIGN 'Authorization' could be presented with multiple header values
                  foreach (var token in authZValue.Split(',', ';')) {
                     if (!string.IsNullOrEmpty(token.Trim())) {
                        signAuthzTokens.Add(token.Trim());
                     }
                  }

                  // If forwarded from a reverse proxy, read the X-Forwarded headers
                  var forwardedHost = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
                  var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();

                  _logger.Log(LogLevel.Debug, $"SIGN authorization Request X-Forwarded-Host: {forwardedHost}");
                  _logger.Log(LogLevel.Debug, $"SIGN authorization Request X-Forwarded-Proto: {forwardedProto}");


                  var request = new SignAuthenticationRequest(Request, forwardedHost, forwardedProto);

                  _logger.Log(LogLevel.Debug, $"SIGN authorization request Method: {request.Method}");
                  _logger.Log(LogLevel.Debug, $"SIGN authorization request Uri: {request.RequestUri}");
                  _logger.Log(LogLevel.Debug, $"SIGN authorization request Port: {request.Port}");
                  _logger.Log(LogLevel.Debug, $"SIGN authorization request HostName: {request.HostName}");

                  // Verify Authorization header and get valid SAML token as a result
                  _logger.Log(LogLevel.Debug, $"Verify SIGN authorization token");
                  var authVerifier = AuthVerifierFactory.Create(60, 60);
                  var samlToken = authVerifier.VerifyToken(request, signAuthzTokens.ToArray());

                  // Issue session for the subject of the SAML token
                  _logger.Log(LogLevel.Debug, $"Issue Session Token for valid SIGN authorization token");
                  var sessionsToken = SessionToken.Issue(samlToken);
                  Response.Headers.Add(APIGatewayResources.SRSAuthorizationHeader, sessionsToken.SessionId);

                  // Issue Solution ActAs HoK SAML Token by for the Subject that authenticates"
                  _logger.Log(LogLevel.Debug, $"Issue SRS Solution ActAs HoK token for the subject");
                  var srsSessionHoKToken = new SolutionStsClient(_loggerFactory, Options)
                     .IssueSolutionTokenByToken(                     
                        samlToken.RawXmlElement);

                  sessionsToken.HoKSamlToken = new SamlToken(srsSessionHoKToken.OuterXml);

                  var claims = new[] {
                     new Claim(ClaimTypes.Name, sessionsToken.UserName),
                  };
                  var identity = new ClaimsIdentity(claims, Constants.SignAuthenticationScheme);
                  var principal = new ClaimsPrincipal(identity);
;
                  result = AuthenticateResult.Success(
                     new AuthenticationTicket(principal, Constants.SignAuthenticationScheme));
               }
            } catch (Exception exc) {
               _logger.Log(LogLevel.Error, $"SIGN Authorization failure: {exc}");
               result = AuthenticateResult.Fail(exc.Message);
            }

            return result;
         });
      }

      protected override async Task HandleChallengeAsync(AuthenticationProperties properties) {
         var signWwwAuthenticate = $"{Constants.SignAuthenticationScheme} realm=\"{Options.Realm}\",service=\"SRS endpoint\",sts=\"{Options.StsServiceEndpoint}\"";
         var wwwAuthenticate = Response.Headers["WWW-Authenticate"].FirstOrDefault();
         if (string.IsNullOrEmpty(wwwAuthenticate)) {
            wwwAuthenticate = signWwwAuthenticate;
         } else {
            wwwAuthenticate = string.Join(',', wwwAuthenticate, signWwwAuthenticate);
         }
         Response.Headers["WWW-Authenticate"] = wwwAuthenticate;
         await base.HandleChallengeAsync(properties);
      }
   }
}
