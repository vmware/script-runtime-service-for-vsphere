// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using NUnit.Framework;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Api;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model;

namespace VMware.ScriptRuntimeService.RunspaceClient.Tests {
   /// <summary>
   /// Contains integrations tests interacting with Runspace API
   /// To run those configure runspace running on local machine on port 5550
   /// and uncomment [Test] attributes
   /// </summary>
   public class Integration {
      private const string RUNSPACE_ENDPOINT_BASE_PATH = @"http://localhost:5550";
      private IScriptApi _scriptsApi;
      [SetUp]
      public void Setup() {
         _scriptsApi = new ScriptApi(RUNSPACE_ENDPOINT_BASE_PATH);
      }

      //[Test]
      public void TestPost() {
         // Arrange
         var executeScriptRequest = new ScriptExecutionRequest("Get-Process");

         // Act
         var result = _scriptsApi.Post(executeScriptRequest);

         // Assert
         Assert.NotNull(result.Id);
      }

      //[Test]
      public void TestGet() {
         // Arrange
         var executeScriptRequest = new ScriptExecutionRequest("Get-Process");
         var executeRequestResult = _scriptsApi.Post(executeScriptRequest);

         // Act
         var result = _scriptsApi.Get(executeRequestResult.Id);

         // Assert
         Assert.NotNull(result.Id);
      }

      //[Test]
      public void TestDelete() {
         // Arrange
         var executeScriptRequest = new ScriptExecutionRequest("Get-Process");
         var executeRequestResult = _scriptsApi.Post(executeScriptRequest);

         // Act & Assert
         Assert.DoesNotThrow(() => _scriptsApi.Delete(executeRequestResult.Id));
      }
   }
}