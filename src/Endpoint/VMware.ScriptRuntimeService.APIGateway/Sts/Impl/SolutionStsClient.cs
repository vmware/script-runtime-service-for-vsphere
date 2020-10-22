// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using VMware.ScriptRuntimeService.Sts;

namespace VMware.ScriptRuntimeService.APIGateway.Sts.Impl {
   public class SolutionStsClient : ISolutionStsClient {
      StsSettings _stsSettings;
      STSClient _stsClient;
      ILoggerFactory _loggerFactory;
      ILogger _logger;
      public SolutionStsClient(ILoggerFactory loggerFactory, StsSettings stsSettings) {         
         _loggerFactory = loggerFactory;
         _logger = _loggerFactory.CreateLogger(typeof(SolutionStsClient).FullName);
         _stsSettings = stsSettings;
      }

      private STSClient GetStsClient() {
         if (_stsClient == null) {            
            _stsClient = new STSClient(
               new Uri(_stsSettings.StsServiceEndpoint), 
               null);
         }
         return _stsClient;
      }

      public XmlElement IssueSolutionTokenByUserCredential(string username, SecureString password) {
         _logger.LogDebug($"IssueSolutionTokenByUserCredential for user {username}");
         // Issue delegated saml token to srs solution
         var delegatedSamlToken = GetStsClient().IssueDelegateToHoKTokenByUserCredential(
            username,
            password,
            _stsSettings.SolutionServiceId,
            _stsSettings.SolutionOwnerId);
         
         // Issue solution HoK token for the user authorized with credentials
         return GetStsClient().IssueHoKTokenByHoKToken(
            delegatedSamlToken,
            new X509Certificate2(_stsSettings.SolutionUserSigningCertificatePath));
      }

      public XmlElement IssueSolutionTokenByToken(XmlElement samlToken) {
         _logger.LogDebug("IssueSolutionTokenByToken");
         var sesSolutionSigningCertificate = new X509Certificate2(_stsSettings.SolutionUserSigningCertificatePath);
         var sesHoKToken = GetStsClient().IssueHoKTokenByCertificate(
            new X509Certificate2(_stsSettings.SolutionUserSigningCertificatePath));

         // Issue Solution ActAs HoK SAML Token by for the Subject that authenticates"         
         return GetStsClient().IssueActAsHoKTokenByHoKToken(
            sesHoKToken,
            sesSolutionSigningCertificate,
            samlToken);
      }

      public XmlElement IssueBearerTokenBySolutionToken(XmlElement solutionToken) {
         return GetStsClient().IssueBearerTokenByHoKToken(
                        solutionToken,
                        new X509Certificate2(_stsSettings.SolutionUserSigningCertificatePath));
      }
   }
}
