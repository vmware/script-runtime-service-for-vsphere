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
   public class DataStreams : IDataStreams {
      public DataStreams() { }

      public DataStreams(IDataStreams source) {
         Information = StreamRecord.FromStreamRecords(source?.Information);
         Error = StreamRecord.FromStreamRecords(source?.Error);
         Warning = StreamRecord.FromStreamRecords(source?.Warning);
         Debug = StreamRecord.FromStreamRecords(source?.Debug);
         Verbose = StreamRecord.FromStreamRecords(source?.Verbose);
      }

      public StreamRecord[] Information { get; set; }
      public StreamRecord[] Error { get; set; }
      public StreamRecord[] Warning { get; set; }
      public StreamRecord[] Debug { get; set; }
      public StreamRecord[] Verbose { get; set; }

      IStreamRecord[] IDataStreams.Information => Information;

      IStreamRecord[] IDataStreams.Error => Error;

      IStreamRecord[] IDataStreams.Warning => Warning;

      IStreamRecord[] IDataStreams.Debug => Debug;

      IStreamRecord[] IDataStreams.Verbose => Verbose;
   }
}
