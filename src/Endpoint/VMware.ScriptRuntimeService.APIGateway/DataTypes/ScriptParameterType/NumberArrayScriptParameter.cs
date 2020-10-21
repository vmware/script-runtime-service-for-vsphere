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
   /// NumberArrayScriptParameter gives ability to provide argument of number array type to a script execution.
   /// </summary>
   [DataContract(Name = "number_array_script_parameter")]
   public class NumberArrayScriptParameter : ScriptParameter {
      /// <summary>
      /// Object that will be passed as an argument to a given parameter. Value, script, or both can be provided as an
      /// argument. If only value is provided without script the object is passed to the script's parameter as is.
      /// </summary>
      [DataMember(Name = "value", IsRequired = false)]
      public double[] NumberArrayValue { get; set; }
   }
}
