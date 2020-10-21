// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.Runspace.Types;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine.Properties;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine {
   public class PSScriptExecutionEngine : IScriptExecutionEngine, IDisposable {
      bool _disposed = false;
      private System.Management.Automation.Runspaces.Runspace _psRunspace;
      private ScriptExecutionResult _lastScriptResult;
      private ReaderWriterLockSlim _lastScriptResultLock = new ReaderWriterLockSlim();

      public PSScriptExecutionEngine() {
         _psRunspace = RunspaceFactory.CreateRunspace(InitialSessionState.CreateDefault());
         _psRunspace.Open();
      }

      #region Interface overloads
      public IScriptExecutionResult ExecuteScript(string content) {
         return ExecuteScript(content, OutputObjectsFormat.Text);
      }

      public IScriptExecutionResult ExecuteScript(string content, OutputObjectsFormat outputObjectsFormat) {
         var task = ExecuteScriptAsync(content, null, outputObjectsFormat);
         task.Wait();
         return task.Result;
      }

      public IScriptExecutionResult ExecuteScript(string content, IScriptParameter[] parameters) {
         return ExecuteScript(content, parameters, OutputObjectsFormat.Text);
      }

      public IScriptExecutionResult ExecuteScript(string content, IScriptParameter[] parameters, OutputObjectsFormat outputObjectsFormat) {
         var task = ExecuteScriptAsync(content, parameters, outputObjectsFormat);
         task.Wait();
         return task.Result;
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(string content) {
         return await ExecuteScriptAsync(content, null, OutputObjectsFormat.Text, new CancellationToken());
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(string content, OutputObjectsFormat outputObjectsFormat) {
         return await ExecuteScriptAsync(content, null, outputObjectsFormat, new CancellationToken());
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(string content, IScriptParameter[] parameters) {
         return await ExecuteScriptAsync(content, parameters, new CancellationToken());
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(string content, IScriptParameter[] parameters, OutputObjectsFormat outputObjectsFormat) {
         return await ExecuteScriptAsync(content, parameters, outputObjectsFormat, new CancellationToken());
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(
         string content,
         CancellationToken cancellationToken) {
         return await ExecuteScriptAsync(content, null, cancellationToken);
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(
         string content,
         OutputObjectsFormat outputObjectsFormat,
         CancellationToken cancellationToken) {
         return await ExecuteScriptAsync(content, null, outputObjectsFormat, cancellationToken);
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(
         string content,
         IScriptParameter[] parameters,
         CancellationToken cancellationToken) {
         return await ExecuteScriptAsync(content, parameters, OutputObjectsFormat.Text, cancellationToken);
      }

      public IScriptExecutionResult ReadResult() {
         _lastScriptResultLock.EnterReadLock();
         try {
            return _lastScriptResult;
         } finally {
            _lastScriptResultLock.ExitReadLock();
         }
         
      }

      public object JsonObjectToNativeEngineObject(string jsonObject) {
         object result = null;

         if (!string.IsNullOrEmpty(jsonObject)) {
            try {
               var ps = GetPowerShell();
               result = ps.AddCommand("ConvertFrom-Json").AddArgument(jsonObject).Invoke().FirstOrDefault();
            } catch (Exception) { }
         }

         return result;
      }
      #endregion

      #region Main Script Execution logic
      private PowerShell GetPowerShell() {
         var powerShell = PowerShell.Create();
         powerShell.Runspace = _psRunspace;
         return powerShell;
      }

      private PowerShell GetPowerShell(string content, IScriptParameter[] parameters, OutputObjectsFormat? outputObjectsFormat = null) {
         var powerShell = GetPowerShell();

         powerShell.Commands.AddScript(content);

         // Add parameters to the PowerShell pipeline
         if (parameters != null) {
            foreach (var scriptParameter in parameters) {
               if (scriptParameter.Value != null) {
                  powerShell.Commands.AddParameter(scriptParameter.Name, scriptParameter.Value);
               } else {
                  powerShell.Commands.AddParameter(scriptParameter.Name);
               }
            }
         }

         // Add output objects formatting to the pipeline
         if (outputObjectsFormat == OutputObjectsFormat.Json) {
            powerShell.AddScript(SystemScripts.SystemScripts.ConvertToJsonWithTypeInfo);
         }

         if (outputObjectsFormat == OutputObjectsFormat.Text) {
            powerShell.AddCommand("Out-String");
         }

         return powerShell;
      }

      public async Task<IScriptExecutionResult> ExecuteScriptAsync(string content,
         IScriptParameter[] parameters,
         OutputObjectsFormat outputObjectsFormat,
         CancellationToken cancellationToken) {
         _lastScriptResult = new ScriptExecutionResult {
            Streams = new DataStreams(),
            OutputObjectsFormat = outputObjectsFormat,
            OutputObjectCollection = new OutputObjectCollection(),
            State = ScriptState.Running,
            StarTime = DateTime.Now
         };

         return await Task.Run(() => {

            // Evaluate parameters
            IScriptParameter[] evaluatedParams = null;
            var argumentTransfomationError = string.Empty;
            if (parameters != null && parameters.Length > 0) {

               var evaluatedParamsList = new List<IScriptParameter>();

               foreach (var parameter in parameters) {
                  if (!string.IsNullOrEmpty(parameter?.Script)) {

                     // Run argument transformation script
                     var argumentTransformationPs = GetPowerShell(
                        parameter.Script, 
                        // Transformation Scripts Are Expected to have single parameter named 'argument'
                        new[] { new ScriptParameter {
                           Name = "argument",
                           Value = parameter.Value
                        }});
                     SetupStreamHandlers(argumentTransformationPs, _lastScriptResult);
                     Collection<PSObject> transformedValue = null;
                     try {
                        transformedValue = argumentTransformationPs.Invoke();
                     } catch (Exception) {
                     }
                     if (argumentTransformationPs.InvocationStateInfo.State == PSInvocationState.Failed) {
                        argumentTransfomationError =
                        string.Format(
                           Resources.ArgumentTransformationError,
                           parameter.Name,
                           argumentTransformationPs.InvocationStateInfo.Reason);
                        break;
                     }

                     var transformedParam = new ScriptParameter {
                        Name = parameter.Name,
                        Value = null
                     };

                     if (transformedValue?.Count > 0) {                        
                        if (transformedValue.Count == 1) {
                           // Array of 1 item is just the item in PS
                           transformedParam.Value = transformedValue[0];
                        } else {
                           // Present it as array of values
                           transformedParam.Value = transformedValue.ToArray();
                        }
                     }

                     evaluatedParamsList.Add(transformedParam);

                  } else {
                     // Get parameter as-is
                     evaluatedParamsList.Add(parameter);
                  }
               }

               evaluatedParams = evaluatedParamsList.ToArray();
            }

            // Requested Script Execution starts here
            var ps = GetPowerShell(content, evaluatedParams, outputObjectsFormat);
            // capture ps object output
            var outputPsDataCollection = new PSDataCollection<PSObject>();

            // Run script only if there are no error from the parameters transformations
            if (string.IsNullOrEmpty(argumentTransfomationError)) {
               SetupStreamHandlers(ps, _lastScriptResult);

               var asyncInvoke = ps.BeginInvoke<PSObject, PSObject>(null, outputPsDataCollection);

               while (!asyncInvoke.IsCompleted) {
                  if (cancellationToken.IsCancellationRequested) {
                     ps.Stop();
                     break;
                  }
                  Thread.Sleep(100);
               }
            }

            ((DataStreams)_lastScriptResult.Streams).Close();

            _lastScriptResultLock.EnterWriteLock();
            try {
               _lastScriptResult.EndTime = DateTime.Now;

               if (!string.IsNullOrEmpty(argumentTransfomationError)) {
                  // Argument transformation scripts are failing
                  _lastScriptResult.Reason = argumentTransfomationError;
                  SetScriptState(_lastScriptResult, PSInvocationState.Failed);
               } else {
                  _lastScriptResult.Reason = ps.InvocationStateInfo.Reason?.ToString();
                  SetScriptState(_lastScriptResult, ps.InvocationStateInfo.State);                  
               }

               if (outputObjectsFormat == OutputObjectsFormat.Json) {
                  var serializedObject = new List<string>();
                  foreach (var stringObject in outputPsDataCollection) {
                     serializedObject.Add(stringObject.ToString());
                  }

                     ((OutputObjectCollection)_lastScriptResult.OutputObjectCollection).SerializedObjects =
                        serializedObject.ToArray();
               }

               if (outputObjectsFormat == OutputObjectsFormat.Text) {
                  var sb = new StringBuilder();

                  foreach (var psObject in outputPsDataCollection ?? Enumerable.Empty<PSObject>()) {
                     if (!string.IsNullOrWhiteSpace(psObject?.ToString())) {
                        sb.AppendLine(psObject.ToString());
                     }
                  }

                  ((OutputObjectCollection)_lastScriptResult.OutputObjectCollection).FormattedTextPresentation =
                     sb.ToString().Trim();
               }

            } finally {
               _lastScriptResultLock.ExitWriteLock();
            }
            

            return ReadResult();
         });
      }

      private static void SetScriptState(ScriptExecutionResult resultObject, PSInvocationState invcationState) {
         resultObject.State = ScriptState.Success;
         switch (invcationState) {
            case PSInvocationState.Failed:
               resultObject.State = ScriptState.Error;
               break;
            case PSInvocationState.Stopping:
            case PSInvocationState.Stopped:
               resultObject.State = ScriptState.Canceled;
               break;
            case PSInvocationState.Running:
               resultObject.State = ScriptState.Running;
               break;
            default:
               resultObject.State = ScriptState.Success;
               break;
         }
      }

      private static void SetupStreamHandlers(PowerShell ps, ScriptExecutionResult resultObject) {
         ps.Streams.Information.DataAdding += (sender, args) => {
            ((DataStreams)resultObject.Streams).AddInformation(args.ItemAdded);
         };

         ps.Streams.Error.DataAdding += (sender, args) => {
            ((DataStreams)resultObject.Streams).AddError(args.ItemAdded);
         };

         ps.Streams.Debug.DataAdding += (sender, args) => {
            ((DataStreams) resultObject.Streams).AddDebug(args.ItemAdded);
         };

         ps.Streams.Warning.DataAdding += (sender, args) => {
            ((DataStreams) resultObject.Streams).AddWarning(args.ItemAdded);
         };

         ps.Streams.Verbose.DataAdding += (sender, args) => {
            ((DataStreams) resultObject.Streams).AddVerbose(args.ItemAdded);
         };
      }
      #endregion

      protected virtual void Dispose(bool disposing) {
         if (!_disposed) {
            if (disposing) {
               _psRunspace.Dispose();
            }
            _disposed = true;
         }         
      }
      public void Dispose() {         
         Dispose(true);
         // Suppress finalization.
         GC.SuppressFinalize(this);
      }
   }
}
