// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// IntegerArrayScriptParameter gives ability to provide argument of integer array type to a script execution.
   /// </summary>
   [DataContract(Name = "integer_array_script_parameter")]
   public class IntegerArrayScriptParameter : ScriptParameter {
      /// <summary>
      /// Object that will be passed as an argument to a given parameter. Value, script, or both can be provided as an
      /// argument. If only value is provided without script the object is passed to the script's parameter as is.
      /// </summary>
      [DataMember(Name = "value", IsRequired = false)]
      public int[] IntegerArrayValue { get; set; }
   }
}
