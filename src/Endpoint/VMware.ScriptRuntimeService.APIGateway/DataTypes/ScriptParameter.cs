// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// ScriptParameter object gives ability to provide argument to a script execution.
   /// </summary>
   [DataContract(Name = "script_parameter")]
   public class ScriptParameter : IScriptParameter {
      /// <summary>
      /// Name of the parameter. When a parameter is specified on a script execution create the name should match
      /// the name of the parameter that is defined in the script.
      /// </summary>
      [Required]
      [DataMember(Name = "name", IsRequired = true)]
      public string Name { get; set; }

      /// <summary>
      /// Object that will be passed as an argument to a given parameter. Value, script, or both can be provided as an
      /// argument. If only value is provided without script the object is passed to the script's parameter as is.
      /// </summary>
      [DataMember(Name = "value", IsRequired = false)]
      public virtual object Value { get; set; }

      /// <summary>
      /// Script to be executed for this parameter. Value produced by the script will be the argument for the parameter.
      /// 
      /// In case a script is specified as an argument for a script parameter the service runs the script of the
      /// parameter before running the requested script. The value that is produced as an output is used
      /// as an argument for the script parameter.
      /// If both script and value are specified for a script parameter the script is executed with single argument
      /// with value specified in the value field. The object that is produced as an output is used as an argument
      /// for the script parameter.
      /// </summary>
      [DataMember(Name = "script", IsRequired = false)]
      public string Script { get; set; }
   }
}
