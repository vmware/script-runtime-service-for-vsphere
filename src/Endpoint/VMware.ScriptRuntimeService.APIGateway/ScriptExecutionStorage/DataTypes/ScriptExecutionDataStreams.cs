// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes {
   public class ScriptExecutionDataStreams : IScriptExecutionDataStreams {
      public ScriptExecutionDataStreams(IDataStreams dataStreams) {
         Streams = new DataStreams(dataStreams);
      }
      public DataStreams Streams { get; set; }

      IDataStreams IScriptExecutionDataStreams.Streams => Streams;
   }
}
