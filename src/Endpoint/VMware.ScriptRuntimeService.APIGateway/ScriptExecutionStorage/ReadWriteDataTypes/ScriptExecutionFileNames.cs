// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.ReadWriteDataTypes {
   public static class ScriptExecutionFileNames {
      public static string ScriptExecution => "ScriptExecution.json";
      public static string ScriptExecutionOutput => "OutputObjects.json";
      public static string ScriptExecutionStreams => "DataStreams.json";
   }
}
