// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.Authentication.Basic;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.APIGateway.Sts;

namespace VMware.ScriptRuntimeService.APIGateway.Controllers {
   [Route("api/auth")]
   [Produces("application/json")]
   [Consumes("application/json")]
   [ApiController]
   public class AuthenticationController : ControllerBase {
      private StsSettings _stsSettings = new StsSettings();

      private const string BasicSignAuthSchemes = BasicAuthenticationHandler.AuthenticationScheme + "," +
                                         Http.Sso.Authentication.Constants.SignAuthenticationScheme;

      public AuthenticationController(IConfiguration Configuration) {
         Configuration.Bind("StsSettings", _stsSettings);
      }

      /// <summary>
      /// Exchanges client credentials or SIGN token for SRS access key
      /// </summary>
      /// <remarks>
      /// Uses VCenter SSO as Identity and Authentication Server.
      /// Two types of authentication are supported SIGN and Basic.
      /// When Basic authentication is used service exchanges username and password for SAML from VCenter SSO.
      /// When SIGN authentication is used service exchanges the SSO SAML token from the SIGN token for another SAML token on behalf of the user from VCenter SSO.
      /// On successful authentication with SSO the service issues **X-SRS-API-KEY** token are returns it the response headers. **X-SRS-API-KEY** token is used to authorize access to service resources. 
      /// The service associates **X-SRS-API-KEY** token to acquired from SSO SAML token. The associated SSO SAML token is used to authorize PowerCLI to VCenter services. 
      /// </remarks>
      [HttpPost("login", Name = "login")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      [Authorize(AuthenticationSchemes = BasicSignAuthSchemes)]
      public void Login() { }

      /// <summary>
      /// Revokes SRS access key
      /// </summary>
      /// <remarks>
      /// The service revokes **X-SRS-API-KEY** and deletes all non-active **runspace** resources associated with it.
      /// Active runspaces will be deletely immediately after the completion of the scripts they run.
      /// </remarks>
      [HttpPost("logout", Name = "logout")]
      [ProducesResponseType(StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status401Unauthorized)]
      [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
      [Authorize(AuthenticationSchemes = SrsAuthenticationScheme.SessionAuthenticationScheme)]
      public void Logout() {
         try {
            var authzToken = SessionToken.FromHeaders(Request.Headers);

            // Unregister session from active sessions
            Sessions.Instance.UnregisterSession(authzToken);

            Ok();

         } catch (Exception e) {
            StatusCode(
               500,
               new ErrorDetails(
                  ApiErrorCodes.GetErrorCode(
                     nameof(APIGatewayResources.SessionsController_SessionsService_FailedToDeleteSession)),
                  APIGatewayResources.SessionsController_SessionsService_FailedToDeleteSession,
                  e.ToString()));
         }
      }
   }
}