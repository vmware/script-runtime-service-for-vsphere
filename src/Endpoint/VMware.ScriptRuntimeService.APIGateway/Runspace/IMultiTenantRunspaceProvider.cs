// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Authentication;
using VMware.ScriptRuntimeService.APIGateway.Sts;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace {
   internal interface IMultiTenantRunspaceProvider {
      /// <summary>
      /// Verifies whether new runspace can be created.
      /// </summary>
      /// <returns>True if new runspace is allowed to be created, otherwise false</returns>
      bool CanCreateNewRunspace();

      /// <summary>
      /// Starts runspace creation for specified user
      /// </summary>
      /// <param name="userId">User identifier</param>
      /// <param name="sessionToken">Authorized session Roken with which create new runspace is requested</param>
      /// <param name="name">Runspace name</param>
      /// <param name="runVcConnectionScript">Indicates whether VC Connection Script should be started after runspaces creation</param>
      /// <param name="stsClient">StsClient needed to acquire SAML tokens for connect vc operation, if vc connection script is requested</param>
      /// <param name="vcEndpoint">Vc endpoint to connect to, if vc connection script is requested</param>
      /// <returns>
      /// Info object <see cref="IRunspaceData"/> for the created runspace.
      /// </returns>
      IRunspaceData StartCreate(
         string userId,
         ISessionToken sessionToken,
         string name,
         bool runVcConnectionScript,
         ISolutionStsClient stsClient,
         string vcEndpoint);

      /// <summary>
      /// Gets all running runspace instances for specified user
      /// </summary>
      /// <param name="userId">User identifier</param>
      /// <returns>
      /// List of runspace instances <see cref="IRunspaceData"/>  if available, otherwise null.
      /// </returns>
      IEnumerable<IRunspaceData> List(string userId);

      /// <summary>
      /// Get instance of running runspace if available, otherwise null
      /// </summary>
      /// <param name="userId">User identifier</param>
      /// <param name="runspaceId">Runspace id</param>
      /// <returns>
      /// Instance of running runspace  if available, otherwise null.
      /// </returns>
      IRunspaceData Get(string userId, string runspaceId);

      /// <summary>
      /// Removes Runspace 
      /// </summary>
      /// <param name="userId">User identifier</param>
      /// <param name="runspaceId">Runspace id</param>
      void Kill(string userId, string runspaceId);

      /// <summary>
      /// Removes redundant runspaces and all data related for them
      /// Implementers take decision which are redundant runspaces
      /// </summary>
      void Cleanup();

      /// <summary>
      /// Updates runspace provider configuration
      /// </summary>
      /// <param name="runspaceProviderSettings">Runspace provider configuration</param>
      void UpdateConfiguration(RunspaceProviderSettings runspaceProviderSettings);
   }
}
