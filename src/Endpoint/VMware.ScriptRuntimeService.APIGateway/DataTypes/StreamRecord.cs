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
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// StreamRecord object represents a single record of script stream.
   /// </summary>
   [DataContract(Name = "stream_record")]
   public class StreamRecord {
      public static StreamRecord[] FromStreamRecords(IStreamRecord[] sourceRecords) {
         StreamRecord[] result = null;
         if (sourceRecords != null) {
            List<StreamRecord> resultList = new List<StreamRecord>();
            foreach (var sourceRecord in sourceRecords) {
               resultList.Add(new StreamRecord {
                  Time = sourceRecord.Time,
                  Message = sourceRecord.Message
               });
            }

            result = resultList.ToArray();
         }
         return result;
      }

      /// <summary>
      /// Message of the stream records.
      /// </summary>
      [DataMember(Name = "message", IsRequired = false)]
      public string Message { get; set; }

      /// <summary>
      /// Time the message was received by the script execution engine. String representing time in format ISO 8601.
      /// </summary>
      [DataMember(Name = "time", IsRequired = false)]
      [JsonConverter(typeof(IsoDateTimeConverter))]
      public DateTime Time { get; set; }
   }
}
