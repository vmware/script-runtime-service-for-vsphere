// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes {
   public class ScriptExecutionStorageSettings {
      public string ServiceScriptStorageDir { get; set; }

      /// <summary>
      /// Zero or negative means unset, storage service will use default
      /// </summary>
      public int NumberOfScriptsPerUser { get; set; }

      /// <summary>
      /// Zero or negative means unset, storage service will use default
      /// </summary>
      public int NoOlderThanDays { get; set; }

      /// <summary>
      /// Return true if properties values are initialized by default constructor
      /// </summary>
      /// <returns></returns>
      public bool IsDefault() {
         return
            NumberOfScriptsPerUser == 0 &&
            NoOlderThanDays == 0 &&
            ServiceScriptStorageDir == null;
      }
   }
}
