// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Net;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace.Impl {
   internal class DebugRunspaceProvider : IRunspaceProvider {
      internal class RunspaceInfo : IRunspaceInfo {
         public string Id { get; internal set; }
         public IPEndPoint Endpoint { get; internal set; }

         public RunspaceCreationState CreationState { get; set; }

         public RunspaceProviderException CreationError { get; set; }
      }
      public IRunspaceInfo StartCreate() {
         return new RunspaceInfo {
            Id = "Debug-Runspace",
            Endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5550),
            CreationState = RunspaceCreationState.Ready
         };
      }

      public IRunspaceInfo WaitCreateCompletion(IRunspaceInfo runspaceInfo) {
         return runspaceInfo;
      }

      public IWebConsoleInfo WaitCreateCompletion(IWebConsoleInfo webconsole, DateTime creationTime) {
         return webconsole;
      }

      public IRunspaceInfo Get(string id) {
         return StartCreate();
      }

      public void Kill(string id) {
      }

      public IRunspaceInfo[] List() {
         return new [] { StartCreate() };
      }

      public void UpdateConfiguration(IRunspaceProviderSettings runspaceProviderSettings) {
         // Do nothing
      }

      public IWebConsoleInfo CreateWebConsole(string vc, string token, bool allLinked) {
         throw new NotImplementedException();
      }

      public void KillWebConsole(string id) {
         throw new NotImplementedException();
      }

      public IWebConsoleInfo GetWebConsole(string id) {
         throw new NotImplementedException();
      }

      public IWebConsoleInfo[] ListWebConsole() {
         throw new NotImplementedException();
      }
   }
}
