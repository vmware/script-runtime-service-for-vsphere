// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.Runspace.Types;
using VMware.ScriptRuntimeService.RunspaceClient.Bindings.Model;

namespace VMware.ScriptRuntimeService.RunspaceClient {
   class DataStreams : IDataStreams {
      public static IStreamRecord[] FromModelRecords(Bindings.Model.StreamRecord[] sourceRecords) {
         IStreamRecord[] result = null;
         if (sourceRecords != null) {
            List<IStreamRecord> resultList = new List<IStreamRecord>();
            foreach (var sourceRecord in sourceRecords) {
               resultList.Add(new StreamRecord {
                  Time = sourceRecord.Time ?? DateTime.Now,
                  Message = sourceRecord.Message
               });
            }

            result = resultList.ToArray();
         }
         return result;
      }
      public IStreamRecord[] Information { get; set; }
      public IStreamRecord[] Error { get; set; }
      public IStreamRecord[] Warning { get; set; }
      public IStreamRecord[] Debug { get; set; }
      public IStreamRecord[] Verbose { get; set; }
   }
}
