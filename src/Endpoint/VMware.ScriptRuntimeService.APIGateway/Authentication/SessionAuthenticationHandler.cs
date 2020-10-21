// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VMware.ScriptRuntimeService.APIGateway.Properties;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication {
   public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
      public SessionAuthenticationHandler(
         IOptionsMonitor<AuthenticationSchemeOptions> options,
         ILoggerFactory logger,
         UrlEncoder encoder,
         ISystemClock clock)
         : base(options, logger, encoder, clock) { }

      protected async override Task<AuthenticateResult> HandleAuthenticateAsync() {

            return await Task<AuthenticateResult>.Run(
               () => {
                  try {
                     var authzToken = SessionToken.FromHeaders(Request.Headers);
                     if (authzToken != null) {
                        // Validate Session and Update Lifetime if valid
                        Sessions.Instance.ValidateAndUpdateSession(authzToken);

                        var claims = new[] {
                           new Claim(ClaimTypes.Name, authzToken.UserName),
                        };
                        var identity = new ClaimsIdentity(claims, SrsAuthenticationScheme.SessionAuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        return AuthenticateResult.Success(
                           new AuthenticationTicket(principal, SrsAuthenticationScheme.SessionAuthenticationScheme));
                     }

                     return AuthenticateResult.Fail(
                        APIGatewayResources.SessionsController_AuthorizationHeaderIsNotValid);
                  } catch (Exception e) {
                     return AuthenticateResult.Fail(
                        new Exception(
                           APIGatewayResources.SessionsController_AuthorizationHeaderIsNotValid,
                           e));
                  }
               });
      }
   }
}
