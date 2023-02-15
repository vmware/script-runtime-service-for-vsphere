// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SsoAdminServiceReference;
using VMware.ScriptRuntimeService.Sts;
using VMware.ScriptRuntimeService.WsTrust;
using VMware.ScriptRuntimeService.WsTrust.SecurityContext;

namespace VMware.ScriptRuntimeService.SsoAdmin {
   public class SsoAdminClient {
      private const int WEB_OPERATION_TIMEOUT_SECONDS = 30;
      private SsoPortTypeClient _ssoAdminClient;
      private STSClient _stsClient;      
      private X509CertificateValidator _certificateVAlidator;
      private string _hostName;

      public SsoAdminClient(
         Uri ssoSdkUri, 
         Uri stsUri, 
         X509CertificateValidator serverCertificateValidator) {

         var ssoUri = ssoSdkUri.ToString();
         _hostName = ssoSdkUri.Host;

         _certificateVAlidator = serverCertificateValidator;
         _ssoAdminClient = new SsoPortTypeClient(GetBinding(), new EndpointAddress(new Uri(ssoUri)));
         _ssoAdminClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new WsTrustBehavior());

         var serverAuthentication = GetServerAuthentication(serverCertificateValidator);

         if (serverAuthentication != null) {
            _ssoAdminClient
               .ChannelFactory
               .Credentials
               .ServiceCertificate
               .SslCertificateAuthentication = serverAuthentication;
         }

         // Create STS Client for authorized operations
         _stsClient = new STSClient(stsUri, serverCertificateValidator);
      }

      #region Private Helpers
      private X509ServiceCertificateAuthentication GetServerAuthentication(X509CertificateValidator serverCertificateValidator) {
         if (serverCertificateValidator != null) {
            return new X509ServiceCertificateAuthentication {
               CertificateValidationMode = X509CertificateValidationMode.Custom,
               CustomCertificateValidator = serverCertificateValidator
            };
         }

         // Default .NET behavior for TLS certificate validation
         return null;
      }

      private static MessageEncodingBindingElement GetWcfEncoding() {
         // VMware STS requires SOAP version 1.1
         return new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);
      }

      private static HttpsTransportBindingElement GetWcfTransport(bool useSystemProxy) {
         // Communication with the STS is over https
         HttpsTransportBindingElement transport = new HttpsTransportBindingElement {
            RequireClientCertificate = false
         };

         transport.UseDefaultWebProxy = useSystemProxy;
         transport.MaxBufferSize = 2147483647;
         transport.MaxReceivedMessageSize = 2147483647;

         return transport;
      }

      private static Binding GetBinding() {

         // There is no build-in WCF binding capable of communicating
         // with VMware STS, so we create a plain custom one.
         // This binding does not provide support for WS-Trust,
         // that support is currently implemented as a WCF endpoint behaviour.
         var binding = new CustomBinding(GetWcfEncoding(), GetWcfTransport(true));

         var timeout = TimeSpan.FromSeconds(WEB_OPERATION_TIMEOUT_SECONDS);
         binding.CloseTimeout = timeout;
         binding.OpenTimeout = timeout;
         binding.ReceiveTimeout = timeout;
         binding.SendTimeout = timeout;

         return binding;
      }

      private WsSecurityContext CreateAuthorizedInvocationContext(
         string authorizationUsername,
         SecureString authorizationPassword) {
         // Issue Bearer token to authorize create solution user to SSO Admin service
         var bearerToken = _stsClient.IssueBearerTokenByUserCredential(
            authorizationUsername,
            authorizationPassword);

         // Set WS Trust Header Serialization with issued bearer SAML token
         var securityContext = new WsSecurityContext {
            ClientChannel = _ssoAdminClient.InnerChannel,
            Properties = {
               Credentials = {
                  BearerToken = bearerToken
               }
            }
         };
         return securityContext;
      }
      #endregion

      #region Public Interface
      /// <summary>
      /// Creates Solution User in the SSO local domain (default domain is vsphere.local)
      /// This operation requires administrator privileges for SSO
      /// </summary>
      /// <param name="authorizationUsername">User with administrator privileges</param>
      /// <param name="authorizationPassword">Password for the authorizationUsername</param>
      /// <param name="userName">Requested solution user username</param>
      /// <param name="certificate">Certificate fo the solution user. This will be the signing certificate which will be used by the solution user to authorize SSO operations (e.g. acquire HoK SAML token from STS service)</param>
      /// <param name="description">Description of the solution user.</param>
      /// <returns>PrincipalId in format "username@domainname"</returns>
      public string CreateLocalSolutionUser(
         string authorizationUsername,
         SecureString authorizationPassword,
         string userName,
         X509Certificate2 certificate,
         string description) {

         // Create Authorization Invocation Context
         var authorizedInvocationContext =
            CreateAuthorizedInvocationContext(
               authorizationUsername,
               authorizationPassword);

         // Invoke SSO Admin CreateLocalSolutionUser operation
         var ssoPrincipalId = authorizedInvocationContext.
            InvokeOperation(() =>
               _ssoAdminClient.CreateLocalSolutionUserAsync(
                  new ManagedObjectReference {
                     type = "SsoAdminPrincipalManagementService",
                     Value = "principalManagementService"
                  },
                  userName,
                  new SsoAdminSolutionDetails {
                     certificate = Convert.ToBase64String(certificate.RawData),
                     description = description
                  })).Result;

         // Add User to ActAsUsers Group
         var addToActAsUsersGroupResult = authorizedInvocationContext.
            InvokeOperation(() =>
               _ssoAdminClient.AddUsersToLocalGroupAsync(
                  new ManagedObjectReference {
                     type = "SsoAdminPrincipalManagementService",
                     Value = "principalManagementService"
                  },
                  new[] { ssoPrincipalId },
                  "ActAsUsers")).Result;

         if (!addToActAsUsersGroupResult.returnval.FirstOrDefault()) {
            throw new Exception("Solution User Not Successfully Added to ActAsUsers group");
         }

         return $"{ssoPrincipalId.name}@{ssoPrincipalId.domain}";
      }

      /// <summary>
      /// Updates Solution User details. This operation is suitable to update solution user certificate before it gets expired.
      /// This operation requires administrator privileges for SSO
      /// </summary>
      /// <param name="authorizationUsername">User with administrator privileges</param>
      /// <param name="authorizationPassword">Password for the authorizationUsername</param>
      /// <param name="userName">Requested solution user username</param>
      /// <param name="certificate">Certificate fo the solution user. This will be the signing certificate which will be used by the solution user to authorize SSO operations (e.g. acquire HoK SAML token from STS service)</param>
      /// <param name="description">Description of the solution user.</param>
      /// <returns>PrincipalId in format "username@domainname"</returns>
      public string UpdateLocalSolutionUser(string authorizationUsername,
         SecureString authorizationPassword,
         string userName,
         X509Certificate2 certificate,
         string description) {

         // Create Authorization Invocation Context
         var authorizedInvocationContext =
            CreateAuthorizedInvocationContext(
               authorizationUsername,
               authorizationPassword);

         // Invoke SSO Admin CreateLocalSolutionUser operation
         var ssoPrincipalId = authorizedInvocationContext.
            InvokeOperation(() =>
               _ssoAdminClient.UpdateLocalSolutionUserDetailsAsync(
                  new ManagedObjectReference {
                     type = "SsoAdminPrincipalManagementService",
                     Value = "principalManagementService"
                  },
                  userName,
                  new SsoAdminSolutionDetails {
                     certificate = Convert.ToBase64String(certificate.RawData),
                     description = description
                  })).Result;

         return $"{ssoPrincipalId.name}@{ssoPrincipalId.domain}";

      }

      /// <summary>
      /// Finds Solution Users by search string.
      /// This operation requires administrator privileges for SSO
      /// </summary>
      /// <param name="authorizationUsername">User with administrator privileges</param>
      /// <param name="authorizationPassword">Password for the authorizationUsername</param>
      /// <param name="searchString">Search string to find the solution users</param>
      /// <param name="limit">Limit the number of results</param>
      /// <returns>PrincipalId in format "username@domainname"</returns>
      public string[] FindSolutionUser(string authorizationUsername,
         SecureString authorizationPassword,
         string searchString,
         int limit) {

         // Create Authorization Invocation Context
         var authorizedInvocationContext =
            CreateAuthorizedInvocationContext(
               authorizationUsername,
               authorizationPassword);

         // Invoke SSO Admin FindSolutionUsersAsync operation
         var findResult = authorizedInvocationContext.
            InvokeOperation(() =>
               _ssoAdminClient.FindSolutionUsersAsync(
                  new ManagedObjectReference {
                     type = "SsoAdminPrincipalDiscoveryService",
                     Value = "principalDiscoveryService"
                  },
                  searchString,
                  limit)).Result;

         var result = new List<string>();
         foreach (var solutionUserId in findResult?.returnval) {
            result.Add(solutionUserId.id?.name);
         }

         return result.ToArray();
      }

      /// <summary>
      /// Deletes Solution User from SSO
      /// This operation requires administrator privileges for SSO
      /// </summary>
      /// <param name="authorizationUsername">User with administrator privileges</param>
      /// <param name="authorizationPassword">Password for the authorizationUsername</param>
      /// <param name="principalName">Solution User username</param>
      public void DeleteLocalPrincipal(
         string authorizationUsername,
         SecureString authorizationPassword,
         string principalName) {

         // Create Authorization Invocation Context
         var authorizedInvocationContext =
            CreateAuthorizedInvocationContext(
               authorizationUsername,
               authorizationPassword);

         // Invoke SSO Admin DeleteLocalPrincipal operation
         authorizedInvocationContext.
            InvokeOperation(() =>
               _ssoAdminClient.DeleteLocalPrincipalAsync(
                  new ManagedObjectReference {
                     type = "SsoAdminPrincipalManagementService",
                     Value = "principalManagementService"
                  },
                  principalName));
      }

      /// <summary>
      /// Retrieves SSO Trusted certificates
      /// </summary>
      /// <param name="authorizationUsername">User with administrator privileges</param>
      /// <param name="authorizationPassword">Password for the authorizationUsername</param>
      /// <returns></returns>
      public X509Certificate2[] GetTrustedCertificatesAsync(
         string authorizationUsername,
         SecureString authorizationPassword) {

         // Create Authorization Invocation Context
         var authorizedInvocationContext =
            CreateAuthorizedInvocationContext(
               authorizationUsername,
               authorizationPassword);

         // Invoke SSO Admin CreateLocalSolutionUser operation
         var ssoCertificatesResult = authorizedInvocationContext.
            InvokeOperation(() =>
               _ssoAdminClient.GetTrustedCertificatesAsync(
                  new ManagedObjectReference {
                     type = "SsoAdminConfigurationManagementService",
                     Value = "configurationManagementService"
                  })).Result;

         List<X509Certificate2> result = new List<X509Certificate2>();
         foreach (var cert in ssoCertificatesResult.returnval) {
            result.Add(new X509Certificate2(Convert.FromBase64String(cert)));
         }
         return result.ToArray();
      }

      private class VecsCAResponse {         
         [JsonPropertyName("encoded")]
         public string Encoded {get;set;}
      }

      /// <summary>
      /// Retrieves VCSA CA Certificate from VECS
      /// </summary>
      /// <returns>List of strings representing encoded certfificate</returns>
      public IEnumerable<string> GetEncodedCACertificateFromVecs() {
         string jsonResponse = null;

         var request = HttpWebRequest.CreateHttp($"https://{_hostName}/afd/vecs/ssl/");         
         request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => {
            if (_certificateVAlidator != null) {
               _certificateVAlidator.Validate(new X509Certificate2(certificate));
            }
            return true;
         };
         var response = request.GetResponse();         
         try {
            using (var responseStream = response.GetResponseStream()) {
               var reader = new StreamReader(responseStream);
               jsonResponse = reader.ReadToEnd();
            }
         } finally {
            response.Close();
         }

         try {
            if (!string.IsNullOrEmpty(jsonResponse)) {
               var vecsResponse = JsonSerializer.Deserialize<List<VecsCAResponse>>(jsonResponse);
               foreach(var cert in vecsResponse) {
                  yield return cert.Encoded;
               }
            }
         } finally { }
      }

      /// <summary>
      /// Retrieves VCSA CA Certificate from VECS
      /// </summary>
      /// <returns>List of X509Certificate2 representing CA certfificates</returns>
      public IEnumerable<X509Certificate2> GetCACertificateFromVecs() {
         foreach (var encoded in GetEncodedCACertificateFromVecs()) {
            yield return new X509Certificate2(Encoding.ASCII.GetBytes(encoded));
         }
         
      }
      #endregion
   }
}
