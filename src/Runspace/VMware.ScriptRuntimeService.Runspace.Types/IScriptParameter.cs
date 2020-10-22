// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.Runspace.Types {
   public interface IScriptParameter {
      /// <summary>
      /// Name of the parameter
      /// </summary>
      string Name { get; set; }

      /// <summary>
      /// Object that represents the argument
      /// </summary>
      object Value { get; set; }

      /// <summary>
      /// A scrtips that is executed and the value it produces is used as a value of the Parameter
      /// 
      /// Script could be one with parameter named 'argument'. If it is such the Value object will be 
      /// given as an input to this script. See ArgumentTransformation Unit Tests for details
      /// </summary>
      string Script { get; set; }
   }
}
