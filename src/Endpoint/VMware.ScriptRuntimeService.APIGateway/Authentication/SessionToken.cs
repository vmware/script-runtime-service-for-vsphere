// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using VMware.ScriptRuntimeService.APIGateway.Properties;
using VMware.ScriptRuntimeService.Sts.SamlToken;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("VMware.ScriptRuntimeService.APIGateway.Tests")]
namespace VMware.ScriptRuntimeService.APIGateway.Authentication {
   /// <summary>
   /// Represents SRS Service Session Token
   /// Valid token contains authenticated UserName and SessionId and HoK token for the User
   /// 
   /// </summary>
   internal class SessionToken : ISessionToken {
      public static SessionToken FromHeaders(IHeaderDictionary headers) {
         var authzHeader = headers[APIGatewayResources.SRSAuthorizationHeader];

         if (authzHeader.Count > 0) {
            return Sessions.Instance.GetSessionToken(authzHeader[0]);
         }

         return null;
      }

      #region Token Issueance
      public static SessionToken Issue(ISamlToken samlToken) {
         if (samlToken == null) throw new ArgumentNullException(nameof(samlToken));

         SessionToken sessionToken = null;

         // Create output SamlToken Instance with parsed UserName
         sessionToken = new SessionToken {
            UserName = samlToken.SubjectNameId,
            SessionId = Guid.NewGuid().ToString()
         };

         // Register in Sessions
         Sessions.Instance.RegisterSession(sessionToken);

         return sessionToken;
      }
      #endregion

      #region Public Interface
      public string UserName { get; set; }

      public string SessionId { get; set; }

      public ISamlToken HoKSamlToken { get; set; }
      #endregion

      #region Overrides
      public override string ToString() {
         return $"{SessionId}";
      }

      public override int GetHashCode() {
         return ToString().GetHashCode();
      }

      public override bool Equals(object obj) {
         return obj is SessionToken sourceToken &&
                sourceToken.UserName == UserName &&
                sourceToken.SessionId == SessionId;
      }

      #endregion
   }
}
