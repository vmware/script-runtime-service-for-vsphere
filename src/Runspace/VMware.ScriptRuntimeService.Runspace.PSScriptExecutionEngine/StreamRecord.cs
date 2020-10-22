// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.Runspace.PSScriptExecutionEngine {
   public class StreamRecord : IStreamRecord {
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
