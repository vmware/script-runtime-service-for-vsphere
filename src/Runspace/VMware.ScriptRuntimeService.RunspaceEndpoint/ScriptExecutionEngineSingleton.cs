// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Threading;
using VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes;
using VMware.ScriptRuntimeService.RunspaceEndpoint.Resources;
using OutputObjectsFormat = VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes.OutputObjectsFormat;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint {
   public class ScriptExecutionEngineSingleton {
      private IScriptExecutionEngine _engine = new PSScriptExecutionEngine();
      CancellationTokenSource _cancelationToken;
      private string _lastScriptId;
      private string _lastScriptName;

      #region Singleton
      private static readonly ScriptExecutionEngineSingleton _instance = new ScriptExecutionEngineSingleton();
      private ScriptExecutionEngineSingleton() {
      }

      public static ScriptExecutionEngineSingleton Instance => _instance;
      #endregion

      private ScriptExecutionResponse GetLastScriptExecutionResponse() {
         ScriptExecutionResponse result = null;
         var lastScriptResult = _engine.ReadResult();
         if (lastScriptResult != null) {
            result = new ScriptExecutionResponse {
               Id = _lastScriptId,
               Name = _lastScriptName,
               State = lastScriptResult.State.ToString(),
               Reason = lastScriptResult.Reason,
               OutputObjectsFormat = Enum.Parse<OutputObjectsFormat>(lastScriptResult.OutputObjectsFormat.ToString()),
               StarTime = lastScriptResult.StarTime,
               EndTime = lastScriptResult.EndTime,
               OutputObjectCollection = new DataTypes.OutputObjectCollection {
                  FormattedTextPresentation =
                     lastScriptResult.OutputObjectCollection.FormattedTextPresentation,
                  SerializedObjects = lastScriptResult.OutputObjectCollection.SerializedObjects
               },
               DataStreams = new ScriptExecutionStreams {
                  Information = DataTypes.StreamRecord.FromRunspaceStreamRecords(lastScriptResult.Streams.Information),
                  Error = DataTypes.StreamRecord.FromRunspaceStreamRecords(lastScriptResult.Streams.Error),
                  Debug = DataTypes.StreamRecord.FromRunspaceStreamRecords(lastScriptResult.Streams.Debug),
                  Verbose = DataTypes.StreamRecord.FromRunspaceStreamRecords(lastScriptResult.Streams.Verbose),
                  Warning = DataTypes.StreamRecord.FromRunspaceStreamRecords(lastScriptResult.Streams.Warning)
               }
            };
         } else if (!string.IsNullOrEmpty(_lastScriptId)) {
            // Unexpected behavior of the engine. Script was requested but engin doesn't report the state
            // return only the script Id i this case.
            result = new ScriptExecutionResponse {
               Id = _lastScriptId,
               Name = _lastScriptName
            };
         }

         return result;
      }

      public ScriptExecutionResponse StartScriptExecution(
         string content, 
         IScriptParameter[] parameters = null,
         Runspace.Types.OutputObjectsFormat outputObjectsFormat = Runspace.Types.OutputObjectsFormat.Text,
         string name = null) {
         var lastScriptExecutionResponse = GetLastScriptExecutionResponse();
         if (lastScriptExecutionResponse?.State != ScriptState.Running.ToString()) {
            try {
               _cancelationToken = new CancellationTokenSource();
               _lastScriptId = Guid.NewGuid().ToString();
               _lastScriptName = name;

               _engine.ExecuteScriptAsync(
                  content, 
                  parameters,
                  outputObjectsFormat,
                  _cancelationToken.Token);

               lastScriptExecutionResponse = GetLastScriptExecutionResponse();
            } catch (Exception e) {
               throw new RunspaceEndpointException(e.Message, e);
            }

         } else {
            throw new RunspaceEndpointException(
               500,
               ApiErrorCodes.GetErrorCode(nameof(RunspaceEndpointResources.AnotherScriptIsRunning)),
               string.Format(RunspaceEndpointResources.AnotherScriptIsRunning, _lastScriptId));
         }
         return lastScriptExecutionResponse;
      }

      public object ConvertToEngineNativeObject(object obj) {
         var result = obj;

         var lastScriptExecutionResponse = GetLastScriptExecutionResponse();
         if (lastScriptExecutionResponse?.State != ScriptState.Running.ToString()) {

            try {
               if (obj is Newtonsoft.Json.Linq.JArray ||
                   obj is Newtonsoft.Json.Linq.JObject) {

                  var jsonString = obj.ToString();
                  result = _engine.JsonObjectToNativeEngineObject(jsonString);

               }
            } catch (Exception e) {
               throw new RunspaceEndpointException(e.Message, e);
            }

         } else {
            throw new RunspaceEndpointException(
               500,
               ApiErrorCodes.GetErrorCode(nameof(RunspaceEndpointResources.AnotherScriptIsRunning)),
               string.Format(RunspaceEndpointResources.AnotherScriptIsRunning, _lastScriptId));
         }

         return result;
      }

      public ScriptExecutionResponse GetScriptStatus(string id) {
         if (_lastScriptId == id) {
            return GetLastScriptExecutionResponse();
         } else {
            throw new RunspaceEndpointException(
               404,
               ApiErrorCodes.GetErrorCode(nameof(RunspaceEndpointResources.ScriptNotScheduled)),
               string.Format(RunspaceEndpointResources.ScriptNotScheduled, id));
         }
      }

      public ScriptExecutionResponse GetLastScriptStatus() {
         if (!string.IsNullOrEmpty(_lastScriptId)) {
            return GetLastScriptExecutionResponse();
         } else {
            throw new RunspaceEndpointException(
               404,
               ApiErrorCodes.GetErrorCode(nameof(RunspaceEndpointResources.ScriptNotScheduled)),
               string.Format(RunspaceEndpointResources.ScriptNotScheduled, string.Empty));
         }
      }

      public void CancelScriptExecution(string id) {
         if (_lastScriptId == id) {
            _cancelationToken.Cancel();
         } else {
            throw new RunspaceEndpointException(
               404,
               ApiErrorCodes.GetErrorCode(nameof(RunspaceEndpointResources.ScriptNotScheduled)),
               string.Format(RunspaceEndpointResources.ScriptNotScheduled, id));
         }
      }

      public void Close() {
         if (_engine is IDisposable disposable) {
            disposable.Dispose();
         }      
      }
   }
}