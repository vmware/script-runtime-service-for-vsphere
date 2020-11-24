// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// ArgumentScriptTemplate object represents a script with placeholders. If placeholders are replaced by values, the script can be executed.
   /// The purpose of the script is to produce an object of a type valid for a given script runtime. The result type of the script is usually
   /// input type for other scripts. The ArgumentScriptTemplate scripts help to convert simple type values to 
   /// objects of types that can only be produced in a given script runtime. 
   /// </summary>
   [DataContract(Name = "argument_script_template")]
   public class ArgumentScriptTemplate {
      private string _id;
      private string _scriptRuntime;
      private string _resultType;
      private string _scriptTemplate;
      private string[] _placeholders;

      public ArgumentScriptTemplate(
         string id,
         string scriptRuntime,
         string resultType,
         string scriptTemplate,
         string[] placeholders) {
         _id = id;
         _scriptRuntime = scriptRuntime;
         _resultType = resultType;
         _scriptTemplate = scriptTemplate;
         _placeholders = placeholders;
      }
      /// <summary>
      /// Unique identifier for the object.
      /// </summary>
      [DataMember(Name = "id")]
      [ReadOnly(true)]
      public string Id {
         get { return _id; }
      }

      /// <summary>
      /// ScriptRuntime on which this script can be executed.
      /// </summary>
      [DataMember(Name = "script_runtime")]
      public string ScriptRuntime {
         get { return _scriptRuntime; }
      }

      /// <summary>
      /// Type name of the object that is produced by the script template.
      /// </summary>
      [DataMember(Name = "result_type")]
      public string ResultType {
         get { return _resultType; }
      }

      /// <summary>
      /// The script template.
      /// </summary>
      [DataMember(Name = "script_template")]
      public string ScriptTemplate {
         get { return _scriptTemplate; }
      }

      /// <summary>
      /// The script template placeholders that has to be replaced by strings to produce a valid script.
      /// </summary>
      [DataMember(Name = "placeholders")]
      public string[] Placeholders {
         get { return _placeholders; }
      }
   }
}
