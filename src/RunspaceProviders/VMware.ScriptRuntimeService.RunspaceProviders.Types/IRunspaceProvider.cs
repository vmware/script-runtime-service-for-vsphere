// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;

namespace VMware.ScriptRuntimeService.RunspaceProviders.Types
{
   public interface IRunspaceProvider {
      /// <summary>
      /// Starts asynchronous creation of runspace. The CreationState property of the result
      /// object provides information about state of the creation process.
      /// </summary>
      /// <returns>
      /// Info object <see cref="IRunspaceInfo"/> for the created runspace.
      /// </returns>
      IRunspaceInfo StartCreate();

      /// <summary>
      /// Synchronously waits <see cref="IRunspaceInfo"/> CreationState to become different than 'Creating'.
      /// Implementer decides whether to introduce timeout to not block execution.
      /// </summary>
      /// <param name="runspaceInfo">RunspaceInfo instance info to wait.
      /// In case creation state is completed the input object hat to return as a result.</param>
      /// <returns><see cref="IRunspaceInfo"/> instance with CreationState different than 'Creating'</returns>
      IRunspaceInfo WaitCreateCompletion(IRunspaceInfo runspaceInfo);

      /// <summary>
      /// Synchronously waits <see cref="IWebConsoleInfo"/> CreationState to become different than 'Creating'.
      /// Implementer decides whether to introduce timeout to not block execution.
      /// </summary>
      /// <param name="webConsoleInfo">WebConsoleInfo instance info to wait.
      /// In case creation state is completed the input object hat to return as a result.</param>
      /// <returns><see cref="IWebConsoleInfo"/> instance with CreationState different than 'Creating'</returns>
      IWebConsoleInfo WaitCreateCompletion(IWebConsoleInfo webConsoleInfo);

      /// <summary>
      /// Get instance of running runspace if available, otherwise null
      /// </summary>
      /// <returns>
      /// instance of running runspace <see cref="IRunspaceInfo"/> if available, otherwise null.
      /// </returns>
      IRunspaceInfo Get(string id);

      /// <summary>
      /// Removes Runspace
      /// </summary>
      /// <param name="id">Runspace id from <see cref="IRunspaceInfo.Id"/> </param>
      void Kill(string id);

      /// <summary>
      /// Returns available runspaces
      /// </summary>
      IRunspaceInfo[] List();

      /// <summary>
      /// Creates a WebConsole Runspace
      /// </summary>
      /// <param name="vc">VC Address to connect to</param>
      /// <param name="token">VC Token for connection</param>
      /// <param name="allLinked">True to connect all linked servers, otherwise false</param>
      /// <returns>WebConsoleInfo instance</returns>
      IWebConsoleInfo CreateWebConsole(string vc, string token, bool allLinked);

      /// <summary>
      /// Gets a WebConsole Runspace by specified Id
      /// </summary>
      /// <param name="id">The Id of the WebConsole runspace</param>
      /// <returns>WebConsoleInfo instance</returns>
      IWebConsoleInfo GetWebConsole(string id);

      /// <summary>
      /// Gets all WebConsole Runspaces
      /// </summary>
      /// <returns>List of WebConsole isntances</returns>
      IWebConsoleInfo[] ListWebConsole();

      /// <summary>
      /// Removes a WebConsole Runspace
      /// </summary>
      /// <param name="id">The id of the WebConsole runspace to delete</param>
      void KillWebConsole(string id);

      /// <summary>
      /// Updates runspace provider configuration
      /// </summary>
      /// <param name="runspaceProviderSettings">Runspace provider configuration</param>
      void UpdateConfiguration(IRunspaceProviderSettings runspaceProviderSettings);
   }
}
