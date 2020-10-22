// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IdentityModel.Selectors;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;
using StsServiceReference;
using VMware.ScriptRuntimeService.WsTrust;
using VMware.ScriptRuntimeService.WsTrust.SecurityContext;

namespace VMware.ScriptRuntimeService.Sts
{
   public class STSClient {
      private const int WEB_OPERATION_TIMEOUT_SECONDS = 30;
      private STSServiceClient _stsServiceClient;

      public STSClient(Uri stsUri, X509CertificateValidator serverCertificateValidator) {
         _stsServiceClient = new STSServiceClient(GetBinding(), new EndpointAddress(stsUri));
         _stsServiceClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new WsTrustBehavior());

         var serverAuthentication = GetServerAuthentication(serverCertificateValidator);

         if (serverAuthentication != null) {
            _stsServiceClient
               .ChannelFactory
               .Credentials
               .ServiceCertificate
               .SslCertificateAuthentication = serverAuthentication;
         }

      }

      #region Private Helpers

      private X509ServiceCertificateAuthentication GetServerAuthentication(
         X509CertificateValidator serverCertificateValidator) {
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

         return transport;
      }

      private static CustomBinding GetBinding() {

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


      private static LifetimeType GetLifetime(TimeSpan? lifetimeSpan) {
         LifetimeType result;

         if (lifetimeSpan.HasValue) {
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime + lifetimeSpan.Value;

            result = new LifetimeType {
               Created = new AttributedDateTime {
                  Value = XmlConvert.ToString(startTime, XmlDateTimeSerializationMode.Utc)
               },
               Expires = new AttributedDateTime {
                  Value = XmlConvert.ToString(endTime, XmlDateTimeSerializationMode.Utc)
               }
            };
         } else {
            result = null;
         }

         return result;
      }

      #endregion

      #region Public Interface

      /// <summary>
      /// Acquires Bearer Token by Username and Password.
      /// Bearer Token is short lived token that can be presented to the resource server
      /// to authorize the request to access the resource. Typically used by
      /// clients to authorize their requests to the server.
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <returns>Raw XML representation of the SAML 2.0 Bearer Token</returns>
      public XmlElement IssueBearerTokenByUserCredential(
         string username,
         SecureString password) {

         if (username == null) {
            throw new ArgumentNullException("username");
         }

         if (password == null) {
            throw new ArgumentNullException("password");
         }

         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer",
            Lifetime = null,
            DelegatableSpecified = false
         };

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  User = username,
                  Password = password
               }
            }
         };

         // Request Authorization Server to Issue Bearer Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }

      /// <summary>
      /// Acquires Holder of Key Token by Username and Password.
      /// HoK token issuance requires Clients to
      /// 1. Sign AcquireToken Request with the  private key of their Signing Certificate
      /// 2. To present the public key of the Signing Certificate to the Authorization Server in the AcquireToken Request
      /// 3. Authorization Server validate the request signature is signed with the private key of the presented public key
      /// This way Authorization Server ensures request is not modified and the client has the private key corresponding the public key.
      /// HoK tokens are valid for longer period of time compared to the bearer tokens.
      /// HoK tokens can be used to acquire baerer tokens on behalf of the subject. This way servers
      /// can keep HoK and issue Bearer Tokens with which to present the Subject to other services.
      /// 4. HoK token can be delegateble to delegate authorization to other services to act on behalf of the subject.
      /// User authorizes one service to act on his behalf against another server
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <param name="signingCertitificate">The Signing Certificate of the client that will be used to sign the request and present the public key to the Authorization Server</param>
      /// <param name="lifetimeSpan"></param>
      /// <returns>Raw XML representation of the SAML 2.0 HoK Token</returns>
      public XmlElement IssueHoKTokenByUserCredential(
         string username,
         SecureString password,
         X509Certificate2 signingCertitificate,
         TimeSpan? lifetimeSpan = null) {

         if (username == null) {
            throw new ArgumentNullException("username");
         }

         if (password == null) {
            throw new ArgumentNullException("password");
         }

         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey",
            Lifetime = GetLifetime(lifetimeSpan),
            DelegatableSpecified = true,
            Delegatable = true,
            SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
         };

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  User = username,
                  Password = password,
                  // Certificate which will be used by the Authorization Server to validate the signature of the request
                  ClientCertificate = signingCertitificate
               },
               // This Key will be used to sign the request
               SigningKey = signingCertitificate.PrivateKey
            }
         };

         // Request Authorization Server to Issue HoK Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }

      /// <summary>
      /// Acquires Holder of Key Token by Certificate.
      /// HoK token issuance requires Clients to
      /// 1. Sign AcquireToken Request with the  private key of their Signing Certificate
      /// 2. To present the public key of the Signing Certificate to the Authorization Server in the AcquireToken Request
      /// 3. Authorization Server validate the request signature is signed with the private key of the presented public key
      /// This way Authorization Server ensures request is not modified and the client has the private key corresponding the public key.
      /// HoK tokens are valid for longer period of time compared to the bearer tokens.
      /// HoK tokens can be used to acquire bearer tokens on behalf of the subject. This way servers
      /// can keep HoK and issue Bearer Tokens with which to present the Subject to other services.
      /// 4. HoK token can be delegatable to delegate authorization to other services to act on behalf of the subject.
      /// User authorizes one service to act on his behalf against another server
      /// </summary>
      /// <param name="signingCertitificate">Solution user certificate</param>
      /// <param name="lifetimeSpan"></param>
      /// <returns>Raw XML representation of the SAML 2.0 HoK Token</returns>
      public XmlElement IssueHoKTokenByCertificate(
         X509Certificate2 signingCertitificate,
         TimeSpan? lifetimeSpan = null) {


         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey",
            Lifetime = GetLifetime(lifetimeSpan),
            DelegatableSpecified = true,
            Delegatable = true,
            SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
         };

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  // Certificate which will be used by the Authorization Server to validate the signature of the request
                  ClientCertificate = signingCertitificate
               },
               // This Key will be used to sign the request
               SigningKey = signingCertitificate.PrivateKey
            }
         };

         // Request Authorization Server to Issue HoK Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }


      /// <summary>
      /// Acquires Holder of Key Token by Username and Password.
      /// HoK token issuance requires Clients to
      /// 1. Sign AcquireToken Request with the  private key of their Signing Certificate
      /// 2. To present the public key of the Signing Certificate to the Authorization Server in the AcquireToken Request
      /// 3. Authorization Server validate the request signature is signed with the private key of the presented public key
      /// This way Authorization Server ensures request is not modified and the client has the private key corresponding the public key.
      /// HoK tokens are valid for longer period of time compared to the bearer tokens.
      /// HoK tokens can be used to acquire baerer tokens on behalf of the subject. This way servers
      /// can keep HoK and issue Bearer Tokens with which to present the Subject to other services.
      /// 4. HoK token can be delegetable to delegate authorization to other services to act on behalf of the subject.
      /// User authorizes one service to act on his behalf against another server
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <param name="signingCertitificate">The Signing Certificate of the client that will be used to sign the request and present the public key to the Authorization Server</param>
      /// <param name="actAsSamlToken">Act As Saml token</param>
      /// <returns>Raw XML representation of the SAML 2.0 HoK Token</returns>
      public XmlElement IssueActAsHoKTokenByUserCredential(
         string username,
         SecureString password,
         X509Certificate2 signingCertitificate,
         XmlElement actAsSamlToken) {

         if (username == null) {
            throw new ArgumentNullException("username");
         }

         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey",
            ActAs = actAsSamlToken,
            DelegatableSpecified = true,
            Delegatable = true,
            Renewing = new RenewingType {
               AllowSpecified = true,
               Allow = true,
               OKSpecified = true,
               OK = true
            },
            SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
         };

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  User = username,
                  Password = password,
                  // Certificate which will be used by the Authorization Server to validate the signature of the request
                  ClientCertificate = signingCertitificate
               },
               // This Key will be used to sign the request
               SigningKey = signingCertitificate.PrivateKey
            }
         };

         // Request Authorization Server to Issue HoK Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }

      /// <summary>
      /// Acquires Holder of Key Token by HoK Token.
      /// HoK token issuance requires Clients to
      /// 1. Sign AcquireToken Request with the  private key of their Signing Certificate
      /// 2. To present the public key of the Signing Certificate to the Authorization Server in the AcquireToken Request
      /// 3. Authorization Server validate the request signature is signed with the private key of the presented public key
      /// This way Authorization Server ensures request is not modified and the client has the private key corresponding the public key.
      /// HoK tokens are valid for longer period of time compared to the bearer tokens.
      /// HoK tokens can be used to acquire baerer tokens on behalf of the subject. This way servers
      /// can keep HoK and issue Bearer Tokens with which to present the Subject to other services.
      /// 4. HoK token can be delegetable to delegate authorization to other services to act on behalf of the subject.
      /// User authorizes one service to act on his behalf against another server
      /// </summary>
      /// <param name="hokToken"></param>
      /// <param name="signingCertitificate">The Signing Certificate of the client that will be used to sign the request and present the public key to the Authorization Server</param>
      /// <param name="actAsSamlToken">Act As Saml token</param>
      /// <returns>Raw XML representation of the SAML 2.0 HoK Token</returns>
      public XmlElement IssueActAsHoKTokenByHoKToken(
         XmlElement hokToken,
         X509Certificate2 signingCertificate,
         XmlElement actAsSamlToken) {

         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey",
            ActAs = actAsSamlToken,
            DelegatableSpecified = true,
            Delegatable = true,
            Renewing = new RenewingType {
               AllowSpecified = true,
               Allow = true,
               OKSpecified = true,
               OK = true
            },
            SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
         };

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  HolderOfKeyToken = hokToken
               },
               // This Key will be used to sign the request
               SigningKey = signingCertificate.PrivateKey
            }
         };

         // Request Authorization Server to Issue HoK Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }

      /// <summary>
      /// Acquires delegate to HoK token by user and password.
      /// DelegateTo tokens are issued to registered in sso solution users typically from one service
      /// to authorize access to other service on behalf of the principal.
      /// This method issues Token for principal <param name="username"></param> with its password
      /// for service with id <param name="delegateToServiceId"></param> which is registered in the
      /// SSO with solution user <param name="delegateToSolutionUser"></param>
      /// https://wiki.eng.vmware.com/SSO/UseCases#Executing_tasks_on_behalf_of_an_user_.28delegation.29
      /// </summary>
      /// <param name="username"></param>
      /// <param name="password"></param>
      /// <param name="signingCertitificate"></param>
      /// <param name="delegateToServiceId"></param>
      /// <param name="delegateToSolutionUser"></param>
      /// <param name="renewableOk">True to allow token to be renewed after it has expired. Default is false, it is recommended to remain with default value.</param>
      /// <returns>Raw XML representation of the SAML 2.0 DelegateTo token</returns>
      public XmlElement IssueDelegateToHoKTokenByUserCredential(
         string username,
         SecureString password,
         string delegateToServiceId,
         string delegateToSolutionUser,
         TimeSpan? lifetime = null,
         bool? renewable = null,
         bool renewableOk = false) {

         if (username == null) {
            throw new ArgumentNullException("username");
         }

         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey",
            Lifetime = GetLifetime(lifetime),
            DelegateTo = new DelegateToType {
               UsernameToken = new UsernameTokenType {
                  Id = $"Id-{Guid.NewGuid().ToString()}",
                  Username = new AttributedString {
                     Id = $"Id-{delegateToServiceId}",
                     Value = delegateToSolutionUser
                  }
               },

            },
            DelegatableSpecified = true,
            Delegatable = true,
            SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
         };

         if (renewable != null) {
            securityTokenRequest.Renewing = new RenewingType {
               Allow = renewable.Value,
               AllowSpecified = true,
               OK = renewableOk,
               OKSpecified = true,
            };
         }

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  User = username,
                  Password = password
               }
            }
         };

         // Request Authorization Server to Issue HoK Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }


      /// <summary>
      /// Acquires delegate to HoK token by another solution HoK token.
      /// This method issues Token delegated to
      /// service with id <param name="delegateToServiceId"></param> which is registered in the
      /// SSO with solution user <param name="delegateToSolutionUser"></param>
      /// https://wiki.eng.vmware.com/SSO/UseCases#Executing_tasks_on_behalf_of_an_user_.28delegation.29
      /// </summary>
      /// <param name="hokToken"></param>
      /// <param name="signingCertitificate"></param>
      /// <param name="delegateToServiceId"></param>
      /// <param name="delegateToSolutionUser"></param>
      /// <param name="renewableOk">True to allow token to be renewed after it has expired. Default is false, it is recommended to remain with default value.</param>
      /// <returns>Raw XML representation of the SAML 2.0 DelegateTo token</returns>
      public XmlElement IssueDelegateToHoKTokenBySolutionHoK(
         XmlElement hokToken,
         X509Certificate2 signingCertificate,
         string delegateToServiceId,
         string delegateToSolutionUser,
         TimeSpan? lifetime = null,
         bool? renewable = null,
         bool renewableOk = false) {
         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey",
            Lifetime = GetLifetime(lifetime),
            DelegateTo = new DelegateToType {
               UsernameToken = new UsernameTokenType {
                  Id = $"Id-{Guid.NewGuid().ToString()}",
                  Username = new AttributedString {
                     Id = $"Id-{delegateToServiceId}",
                     Value = delegateToSolutionUser
                  }
               },

            },
            DelegatableSpecified = true,
            Delegatable = true,
            SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"
         };

         if (renewable != null) {
            securityTokenRequest.Renewing = new RenewingType {
               Allow = renewable.Value,
               AllowSpecified = true,
               OK = renewableOk,
               OKSpecified = true,
            };
         }

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  HolderOfKeyToken = hokToken
               },
               SigningKey = signingCertificate.PrivateKey
            }
         };

         // Request Authorization Server to Issue HoK Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }


      /// <summary>
      /// Acquires Bearer Token by HoK Token.
      /// This is the use-case of server that holds HoK token to issue bearer token and
      /// use them to authorize requests to other services.
      /// </summary>
      /// <param name="hokToken"></param>
      /// <param name="signingCertitificate"></param>
      /// <returns></returns>
      public XmlElement IssueBearerTokenByHoKToken(
         XmlElement hokToken,
         X509Certificate2 signingCertitificate) {

         if (hokToken == null) {
            throw new ArgumentNullException("hokToken");
         }

         // SAML Token Request
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer",
            Lifetime = null,
            DelegatableSpecified = false
         };

         // Security Token Request Authorization Header
         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  HolderOfKeyToken = hokToken
               },
               SigningKey = signingCertitificate.PrivateKey
            }
         };

         // Request Authorization Server to Issue HoK Token
         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         // Extract the Raw XML Token from the Authorizations Server's Response
         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="hokToken"></param>
      /// <param name="signingCertificate"></param>
      /// <returns></returns>
      public XmlElement IssueHoKTokenByHoKToken(
         XmlElement hokToken,
         X509Certificate2 signingCertificate) {

         if (hokToken == null) {
            throw new ArgumentNullException("hokToken");
         }

         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            KeyType = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/PublicKey",
            SignatureAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512",
            DelegatableSpecified = true,
            Delegatable = true
         };

         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  HolderOfKeyToken = hokToken
               },
               SigningKey = signingCertificate.PrivateKey
            }
         };

         IssueResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.IssueAsync(securityTokenRequest).Result);

         return (XmlElement)
            response.
               RequestSecurityTokenResponseCollection.
               RequestSecurityTokenResponse.
               Items[0];
      }

      /// <summary>
      /// Requests STS to validate given SAML Token
      /// </summary>
      /// <param name="token"></param>
      /// <returns></returns>
      public bool ValidateToken(XmlElement token) {
         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Validate",
            TokenType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTR/Status",
            ValidateTarget = token
         };

         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel
         };

         var response =
            securityContext.InvokeOperation(() => _stsServiceClient.ValidateAsync(securityTokenRequest).Result);

         // Check Saml Token Status is valid
         return ((StatusType)response.RequestSecurityTokenResponse.Items[0]).Code == "http://docs.oasis-open.org/ws-sx/ws-trust/200512/status/valid";
      }

      /// <summary>
      /// Renew token. This operation allows a long-lvide task operation to scheduled in a service
      /// giving the server a renewable token. Then the service can renew the token before use it
      /// to authorize access for the operation.
      /// Use-case: https://wiki.eng.vmware.com/SSO/UseCases#Scheduling_long_lived_task_.28delegation_.2B_renew.29
      /// </summary>
      /// <param name="renewableToken"></param>
      /// <param name="signingCertificate"></param>
      /// <param name="lifetime"></param>
      /// <returns></returns>
      public XmlElement RenewToken(
         XmlElement renewableToken,
         X509Certificate2 signingCertificate,
         TimeSpan? lifetime = null) {

         if (renewableToken == null) {
            throw new ArgumentNullException("renewableToken");
         }

         var securityTokenRequest = new RequestSecurityTokenType {
            RequestType = @"http://docs.oasis-open.org/ws-sx/ws-trust/200512/Renew",
            TokenType = "urn:oasis:names:tc:SAML:2.0:assertion",
            RenewTarget = renewableToken,
            Lifetime = GetLifetime(lifetime)
         };

         var securityContext = new WsSecurityContext {
            ClientChannel = _stsServiceClient.InnerChannel,
            Properties = {
               Credentials = {
                  ClientCertificate = signingCertificate
               },
               SigningKey = signingCertificate.PrivateKey
            }
         };


         RenewResponse response =
            securityContext.InvokeOperation(() => _stsServiceClient.RenewAsync(securityTokenRequest).Result);

         return (XmlElement)
            response.
               RequestSecurityTokenResponse.
               Items[0];

         #endregion
      }
   }
}
