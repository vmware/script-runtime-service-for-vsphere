// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Api;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Client;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model;

namespace VMware.ScriptRuntimeService.RunspaceClient {
   /// <summary>
   /// Runspace Endpoint API Client
   /// </summary>
   public class RunspaceClient : IRunspace {
      private readonly ScriptApi _scriptsApiClient;

      static RunspaceClient() {
         // Override Newtonsoft.Json MaxDepth de/serialization because
         // as of version 13.0.1 the default MaxDepth is 64
         // this keeps pre 13.0.1 behaviour
         Func<Newtonsoft.Json.JsonSerializerSettings> defaultSettingsFunc =
            Newtonsoft.Json.JsonConvert.DefaultSettings;
         if (null == Newtonsoft.Json.JsonConvert.DefaultSettings) {
            defaultSettingsFunc =
               () => new Newtonsoft.Json.JsonSerializerSettings() {
                  MaxDepth = int.MaxValue
               };
         } else {
            var settings = defaultSettingsFunc();
            defaultSettingsFunc =
               () => {
                  settings.MaxDepth = int.MaxValue;
                  return settings;
               };
         }

         Newtonsoft.Json.JsonConvert.DefaultSettings = defaultSettingsFunc;
      }

      public RunspaceClient(IPEndPoint endpoint) {
         var runspaceEndpoint = string.Format($"http://{endpoint.Address}:{endpoint.Port}");
         _scriptsApiClient = new ScriptApi(runspaceEndpoint);
      }

      static ScriptExecutionRequest.OutputObjectsFormatEnum ConvertOutputObjectsFormatEnum(OutputObjectsFormat value) {
         return Enum.Parse<ScriptExecutionRequest.OutputObjectsFormatEnum>(value.ToString());
      }

      public async Task<IScriptExecutionResult> StartScript(string content, OutputObjectsFormat outputObjectsFormat, IScriptParameter[] parameters = null, string name = null) {
         try {

            List<ScriptParameter> scriptParameters = null;
            if (parameters != null && parameters.Length > 0) {
               scriptParameters = new List<ScriptParameter>();
               foreach (var scriptParameter in parameters) {
                  scriptParameters.Add(new ScriptParameter(scriptParameter.Name, scriptParameter.Value, scriptParameter.Script));
               }
            }

            var resultTask = await _scriptsApiClient.
               PostAsync(
                  new ScriptExecutionRequest(
                     content,
                     ConvertOutputObjectsFormatEnum(outputObjectsFormat),
                     scriptParameters,
                     name));
            return new ScriptResult(resultTask);
         } catch (ApiException exc) {
            throw new RunspaceEndpointException(exc.ErrorCode, exc.ErrorContent?.Code ?? exc.ErrorCode, exc.Message, exc);
         }
      }

      public IScriptExecutionResult GetScript(string id) {
         try {
            var scriptJob = _scriptsApiClient.Get(id);
            return new ScriptResult(scriptJob);
         } catch (ApiException exc) {
            throw new RunspaceEndpointException(exc.ErrorCode, exc.ErrorContent?.Code ?? exc.ErrorCode, exc.Message, exc);
         }
      }

      public IScriptExecutionResult GetLastScript() {
         try {
            var scriptJob = _scriptsApiClient.GetLastScript();
            return new ScriptResult(scriptJob);
         } catch (ApiException exc) {
            throw new RunspaceEndpointException(exc.ErrorCode, exc.ErrorContent?.Code ?? exc.ErrorCode, exc.Message, exc);
         }
      }

      public void CancelScript(string id) {
         try {
            _scriptsApiClient.Delete(id);
         } catch (ApiException exc) {
            throw new RunspaceEndpointException(exc.ErrorCode, exc.ErrorContent?.Code ?? exc.ErrorCode, exc.Message, exc);
         }
      }
   }
}
