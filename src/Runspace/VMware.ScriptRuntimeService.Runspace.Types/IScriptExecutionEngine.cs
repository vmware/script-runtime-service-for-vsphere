// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Threading;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.Runspace.Types {
   /// <summary>
   /// Interface for communication with the ScriptExecutionEngine
   /// Different implementation of this interface will give the ability to request
   /// script execution, monitor the script progress, and return results.
   /// </summary>
   public interface IScriptExecutionEngine {
      #region Synchronous Operations
      /// <summary>
      /// Runs a script with given content
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <returns>String that represents script execution output</returns>
      IScriptExecutionResult ExecuteScript(string content);

      /// <summary>
      /// Runs a script with given content
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <param name="outputObjectsFormat">Format of the result objects produced by the script</param>
      /// <returns>String that represents script execution output</returns>
      IScriptExecutionResult ExecuteScript(string content, OutputObjectsFormat outputObjectsFormat);

      /// <summary>
      /// Runs a script with given content
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <param name="parameters">Input parameters for the script <see cref="IScriptParameter"/></param>
      /// <returns>String that represents script execution output</returns>
      IScriptExecutionResult ExecuteScript(string content, IScriptParameter[] parameters);

      /// <summary>
      /// Runs a script with given content
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <param name="parameters">Input parameters for the script <see cref="IScriptParameter"/></param>
      /// <param name="outputObjectsFormat">Format of the result objects produced by the script</param>
      /// <returns>String that represents script execution output</returns>
      IScriptExecutionResult ExecuteScript(string content, IScriptParameter[] parameters, OutputObjectsFormat outputObjectsFormat);
      #endregion

      #region Asynchronous Operations
      /// <summary>
      /// Async version of <cref="ExecuteScript" />
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <returns></returns>
      Task<IScriptExecutionResult> ExecuteScriptAsync(string content, CancellationToken cancellationToken);

      /// <summary>
      /// Async version of <cref="ExecuteScript" />
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <param name="outputObjectsFormat">Format of the result objects produced by the script</param>
      /// <returns></returns>
      Task<IScriptExecutionResult> ExecuteScriptAsync(string content, OutputObjectsFormat outputObjectsFormat, CancellationToken cancellationToken);

      /// <summary>
      /// Async version of <cref="ExecuteScript" />
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <param name="parameters">Input parameters for the script <see cref="IScriptParameter"/></param>
      /// <returns></returns>
      Task<IScriptExecutionResult> ExecuteScriptAsync(string content, IScriptParameter[] parameters, CancellationToken cancellationToken);

      /// <summary>
      /// Async version of <cref="ExecuteScript" />
      /// </summary>
      /// <param name="content">Content of the script</param>
      /// <param name="parameters">Input parameters for the script <see cref="IScriptParameter"/></param>
      /// <param name="outputObjectsFormat">Format of the result objects produced by the script</param>
      /// <returns></returns>
      Task<IScriptExecutionResult> ExecuteScriptAsync(string content, IScriptParameter[] parameters, OutputObjectsFormat outputObjectsFormat, CancellationToken cancellationToken);

      /// <summary>
      /// Reads the result of the last started script
      /// </summary>
      /// <returns><see cref="IScriptExecutionResult"/> that represents script execution result</returns>
      IScriptExecutionResult ReadResult();

      /// <summary>
      /// Converts object represented as Json string to negine native object
      /// </summary>
      /// <returns><see cref="System.Object"/> Engine native object</returns>
      object JsonObjectToNativeEngineObject(string jsonObject);
      #endregion
   }
}
