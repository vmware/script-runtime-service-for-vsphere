// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes {
   [DataContract]
   public class StreamRecord {
      public StreamRecord(Runspace.Types.IStreamRecord source) {
         Message = source.Message;
         Time = source.Time;
      }

      public static StreamRecord[] FromRunspaceStreamRecords(Runspace.Types.IStreamRecord[] sourceRecords) {
         StreamRecord[] result = null;
         if (sourceRecords != null) {

            var resultList = new List<StreamRecord>();

            foreach (var sourceRecord in sourceRecords) {
               resultList.Add(new StreamRecord(sourceRecord));
            }

            result = resultList.ToArray();
         }

         return result;
      }

      [DataMember(Name = "message", IsRequired = false)]
      public string Message { get; set; }

      [DataMember(Name = "time", IsRequired = false)]
      [JsonConverter(typeof(IsoDateTimeConverter))]
      public DateTime Time { get; set; }
   }
}
