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
   public class StreamRecord : IStreamRecord {

      public static StreamRecord[] FromStreamRecords(IStreamRecord[] sourceRecords) {
         StreamRecord[] result = null;
         if (sourceRecords != null) {
            List<StreamRecord> resultList = new List<StreamRecord>();
            foreach (var sourceRecord in sourceRecords) {
               resultList.Add(new StreamRecord(sourceRecord));
            }

            result = resultList.ToArray();
         }
         return result;
      }

      public StreamRecord() { }

      public StreamRecord(IStreamRecord source) {
         if (source != null) {
            Time = source.Time;
            Message = source.Message;
         }
      }

      public DateTime Time { get; set; }
      public string Message { get; set; }

      public override bool Equals(object obj) {
         var source = obj as StreamRecord;
         bool result = false;
         if (source != null) {
            result = Time.Equals(source.Time) &&
                     Message.Equals(source.Message);
         }
         return result;
      }

      public override int GetHashCode() {
         unchecked // Overflow is fine, just wrap
         {
            int hashCode = 41;
            if (this.Message != null)
               hashCode = hashCode * 59 + this.Message.GetHashCode();
            if (this.Time != null)
               hashCode = hashCode * 59 + this.Time.GetHashCode();
            return hashCode;
         }
      }
   }
}
