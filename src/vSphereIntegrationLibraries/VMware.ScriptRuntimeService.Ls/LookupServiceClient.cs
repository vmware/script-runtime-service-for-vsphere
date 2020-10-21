// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Text;
using LookupServiceReference;
using VMware.ScriptRuntimeService.Sts;
using VMware.ScriptRuntimeService.WsTrust;
using VMware.ScriptRuntimeService.WsTrust.SecurityContext;

namespace VMware.ScriptRuntimeService.Ls
{
   public class LookupServiceClient {
      private const int WEB_OPERATION_TIMEOUT_SECONDS = 30;
      private LsPortTypeClient _lsClient;
      private STSClient _stsClient;

      private static readonly ManagedObjectReference RootMoRef = new ManagedObjectReference
      {
         type = "LookupServiceInstance",
         Value = "ServiceInstance"
      };

      public LookupServiceClient(string hostname, X509CertificateValidator serverCertificateValidator) {
         var lsUri = $"https://{hostname}/lookupservice/sdk";

         _lsClient = new LsPortTypeClient(GetBinding(), new EndpointAddress(new Uri(lsUri)));
         _lsClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new WsTrustBehavior());

         var serverAuthentication = GetServerAuthentication(serverCertificateValidator);

         if (serverAuthentication != null)
         {
            _lsClient
               .ChannelFactory
               .Credentials
               .ServiceCertificate
               .SslCertificateAuthentication = serverAuthentication;
         }

         // Create STS Client for authorized operations
         _stsClient = new STSClient(GetStsEndpointUri(), serverCertificateValidator);
      }

      #region Private Helpers
      private X509ServiceCertificateAuthentication GetServerAuthentication(X509CertificateValidator serverCertificateValidator)
      {
         if (serverCertificateValidator != null) {
            return new X509ServiceCertificateAuthentication {
               CertificateValidationMode = X509CertificateValidationMode.Custom,
               CustomCertificateValidator = serverCertificateValidator
            };
         }

         // Default .NET behavior for TLS certificate validation
         return null;
      }

      private static MessageEncodingBindingElement GetWcfEncoding()
      {
         // VMware STS requires SOAP version 1.1
         return new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);
      }

      private static HttpsTransportBindingElement GetWcfTransport(bool useSystemProxy)
      {
         // Communication with the STS is over https
         HttpsTransportBindingElement transport = new HttpsTransportBindingElement
         {
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
         //binding.MaxBufferSize = 2147483647;
         //binding.MaxReceivedMessageSize = 2147483647;

         return binding;
      }
      #endregion

      public LookupServiceRegistrationInfo[] ListRegisteredServices() {
         // Retrieve Service Content
         var svcContent = _lsClient.RetrieveServiceContentAsync(RootMoRef).Result;
         var filterCriteria = new LookupServiceRegistrationFilter();
         filterCriteria.searchAllSsoDomains = true;
         var result = _lsClient.ListAsync(svcContent.serviceRegistration, filterCriteria);
         return result.Result.returnval;
      }

      public IEnumerable<VCEndpoint> GetFederatedVCHostNames() {
         // List VC API endpoints from Lookup Service
         var svcContent = _lsClient.RetrieveServiceContentAsync(RootMoRef).Result;
         var filterCriteria = new LookupServiceRegistrationFilter() {
            searchAllSsoDomains = true,
            serviceType = new LookupServiceRegistrationServiceType {
               product = "com.vmware.cis",
               type = "vcenterserver",
            }
         };

         var result = _lsClient.ListAsync(svcContent.serviceRegistration, filterCriteria).Result;

         if (result.returnval != null) {

            // Collect and return unique VC service hostnames 
            var discoveredHostNames = new List<string>();

            foreach (var svcInfo in result.returnval) {
               foreach (var endpoint in svcInfo.serviceEndpoints) {                 

                  var endpointUri = new Uri(endpoint.url);

                  if (!discoveredHostNames.Contains(endpointUri.Host)) {

                     discoveredHostNames.Add(endpointUri.Host);

                     var endpointCertificates = new List<X509Certificate2>();
                     foreach (var rawCert in endpoint.sslTrust) {
                        if (!string.IsNullOrEmpty(rawCert)) {
                           endpointCertificates.Add(new X509Certificate2(Encoding.ASCII.GetBytes(rawCert)));
                        }
                     }

                     yield return new VCEndpoint {
                        Hostname = endpointUri.Host,
                        SslTrustCertificates = endpointCertificates.ToArray()
                     };
                  }
               }
            }
         }
      }

      public Uri GetStsEndpointUri() {
         var product = "com.vmware.cis";         
         var type = "cs.identity";
         var endpointType = "com.vmware.cis.cs.identity.sso";
         return FindServiceEndpoint(product, type, endpointType);
      }

      public Uri GetSsoAdminEndpointUri() {
         var product = "com.vmware.cis";
         var endpointType = "com.vmware.cis.cs.identity.admin";
         var type = "sso:admin";
         return FindServiceEndpoint(product, type, endpointType);
      }

      private Uri FindServiceEndpoint(string product, string type, string endpointType) {
         Uri result = null;

         var svcContent = _lsClient.RetrieveServiceContentAsync(RootMoRef).Result;
         var filterCriteria = new LookupServiceRegistrationFilter() {
            searchAllSsoDomains = true,
            serviceType = new LookupServiceRegistrationServiceType {
               product = product,
               type = type
            }
         };

         var lsRegInfo = _lsClient.
            ListAsync(svcContent.serviceRegistration, filterCriteria)
            .Result?
            .returnval?
            .FirstOrDefault();
         if (lsRegInfo != null) {
            var registrationEndpooint = lsRegInfo.
               serviceEndpoints?.
               Where(a => a.endpointType.type == endpointType)?.
               FirstOrDefault<LookupServiceRegistrationEndpoint>();
            if (registrationEndpooint != null) {
               result = new Uri(registrationEndpooint.url);
            }
         }
         return result;         
      }

      public void RegisterService(
         string ssoUsername,
         SecureString ssoPassword,
         string nodeId,
         string ownerId,
         string serviceDescriptionResourceKey,
         string serviceId,
         string serviceNameResourceKey,
         string serviceVersion,
         string serviceTypeProduct,
         string serviceTypeType,
         string endpointUrl,
         string endpointProtocol,
         string endpointType,
         X509Certificate2 sslTrustCertificate)
      {
         // Issue Bearer token by user and password fort service registration operation
         var bearerToken = _stsClient.IssueBearerTokenByUserCredential(ssoUsername, ssoPassword);

         // Populate Register Service Spec
         var serviceRegistrationCreateSpec = new LookupServiceRegistrationCreateSpec
         {
            serviceType = new LookupServiceRegistrationServiceType
            {
               product = serviceTypeProduct,
               type = serviceTypeType
            },
            nodeId = nodeId,
            ownerId = ownerId,
            serviceDescriptionResourceKey = serviceDescriptionResourceKey,
            serviceNameResourceKey = serviceNameResourceKey,
            serviceVersion = serviceVersion,
            serviceEndpoints = new[] {
               new LookupServiceRegistrationEndpoint {
                  url = endpointUrl,
                  sslTrust = new[] {Convert.ToBase64String(sslTrustCertificate.RawData)},
                  endpointType = new LookupServiceRegistrationEndpointType {
                     protocol = endpointProtocol,
                     type = endpointType
                  }
               }
            }
         };

         // Retrieve Service Content
         var svcContent = _lsClient.RetrieveServiceContentAsync(RootMoRef).Result;

         // Set WS Trust Header Serialization with issued bearer SAML token
         var securityContext = new WsSecurityContext
         {
            ClientChannel = _lsClient.InnerChannel,
            Properties = {
               Credentials = {
                  BearerToken = bearerToken
               }
            }
         };

         // Register Service
         securityContext.InvokeOperation(() => _lsClient.CreateAsync(
               svcContent.serviceRegistration,
               serviceId,
               serviceRegistrationCreateSpec).Wait());
      }

      public void DeleteService(
         string ssoUsername,
         SecureString ssoPassword,
         string serviceId) {
         // Issue Bearer token by user and password fort service registration operation
         var bearerToken = _stsClient.IssueBearerTokenByUserCredential(ssoUsername, ssoPassword);

        // Retrieve Service Content
         var svcContent = _lsClient.RetrieveServiceContentAsync(RootMoRef).Result;

         // Set WS Trust Header Serialization with issued bearer SAML token
         var securityContext = new WsSecurityContext {
            ClientChannel = _lsClient.InnerChannel,
            Properties = {
               Credentials = {
                  BearerToken = bearerToken
               }
            }
         };

         LookupServiceRegistrationInfo serviceFound = null;
         // Try Get Service with specified Id
         try {
            serviceFound = _lsClient.GetAsync(
               svcContent.serviceRegistration,
               serviceId).
               Result;
         } catch (Exception) {

         }
         
         if (serviceFound != null) {
            // Delete Service
            securityContext.InvokeOperation(() => _lsClient.DeleteAsync(
               svcContent.serviceRegistration,
               serviceId).Wait());
         }            
      }
   }

}
