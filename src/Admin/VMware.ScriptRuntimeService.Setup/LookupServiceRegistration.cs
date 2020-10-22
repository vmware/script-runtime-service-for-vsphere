// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using LookupServiceReference;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security;
using VMware.ScriptRuntimeService.Ls;

namespace VMware.ScriptRuntimeService.Setup
{
   public class LookupServiceRegistration
   {
      private LookupServiceClient _lsClient;
      private readonly SetupServiceSettings _serviceSettings;
      private readonly ILogger _logger;

      public LookupServiceRegistration(
         ILoggerFactory loggerFactory,
         SetupServiceSettings serviceSettings,
         LookupServiceClient lsClient) {

         if (loggerFactory == null) { throw new ArgumentNullException(nameof(loggerFactory)); }
         _logger = loggerFactory.CreateLogger(typeof(LookupServiceRegistration).FullName);
         _lsClient = lsClient ?? throw new ArgumentNullException(nameof(lsClient));
         _serviceSettings = serviceSettings ?? throw new ArgumentNullException(nameof(serviceSettings));
      }

      public void Register(string user, SecureString password) {
         try {

            _logger.LogInformation(
               string.Format(Resources.PerofomingOperation, Resources.RegisteringToLookupServiceOperation));
            _lsClient.RegisterService(
               user,
               password,
               _serviceSettings.NodeId,
               _serviceSettings.OwnerId,
               _serviceSettings.ServiceDescriptionResourceKey,
               _serviceSettings.ServiceId,
               _serviceSettings.ServiceNameResourceKey,
               _serviceSettings.ServiceVersion,
               _serviceSettings.ServiceTypeProduct,
               _serviceSettings.ServiceTypeType,
               _serviceSettings.EndpointUrl,
               _serviceSettings.EndpointProtocol,
               _serviceSettings.EndpointType,
               _serviceSettings.TlsCertificate);
         } catch (AggregateException ex) {
            _logger.LogError(ex.ToString());
            throw;
         }
      }


      /// <summary>
      /// Updates service registration useing delete current and creat ne service registration
      /// First searches for existing service registration by service Id provided by <see cref="_serviceSettings"/>.
      /// Then removes previous registration and creates new with updated Tls certificate provided by <see cref="_serviceSettings"/>.
      /// </summary>
      public void UpdateServiceRegistrationTlsCertificate(string user, SecureString password) {
         try {
            _logger.LogInformation(
               string.Format(Resources.PerofomingOperation, Resources.UpdateLookupServiceRegistration));
            var serviceFound = _lsClient.ListRegisteredServices().
               Where<LookupServiceRegistrationInfo>(r => r.serviceId == _serviceSettings.ServiceId).
               FirstOrDefault();

            if (serviceFound == null) {
               throw new Exception($"Service with id '{_serviceSettings.ServiceId}' not found in lookup service registered services.");
            }

            var registeredEndpoint = serviceFound.serviceEndpoints.FirstOrDefault<LookupServiceRegistrationEndpoint>();
            if (registeredEndpoint == null ||
               string.IsNullOrEmpty(registeredEndpoint.url)) {
               throw new Exception($"Lookup service registration for service with id '{_serviceSettings.ServiceId}' has no valid Endpoint record needed for srvice registrion update operation.");
            }

            // Remove previous registration
            Deregister(user, password);

            _logger.LogInformation(
               string.Format(Resources.PerofomingOperation, Resources.RegisteringToLookupServiceOperation));
            // Create new service registration with new Tls Certificate
            _lsClient.RegisterService(
               user,
               password,
               serviceFound.nodeId,
               serviceFound.ownerId,
               serviceFound.serviceDescriptionResourceKey,
               serviceFound.serviceId,
               serviceFound.serviceNameResourceKey,
               serviceFound.serviceVersion,
               serviceFound.serviceType?.product,
               serviceFound.serviceType?.type,
               registeredEndpoint.url,
               _serviceSettings.EndpointProtocol,
               _serviceSettings.EndpointType,
               _serviceSettings.TlsCertificate);
         } catch (AggregateException ex) {
            _logger.LogError(ex.ToString());
            throw;
         }
      }

      public void Deregister(string user, SecureString password) {
         try {
            _logger.LogInformation(
               string.Format(Resources.PerofomingOperation, Resources.DeregisterFromLookupServiceOperation));
            _lsClient.DeleteService(
               user,
               password,
               _serviceSettings.ServiceId);
         } catch (AggregateException ex) {
            _logger.LogError(ex.ToString());
            throw;
         }
      }
   }
}