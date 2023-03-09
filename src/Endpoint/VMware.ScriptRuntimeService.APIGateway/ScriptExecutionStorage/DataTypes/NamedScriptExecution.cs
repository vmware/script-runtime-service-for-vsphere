// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.ScriptExecutionStorage.DataTypes {
   public class NamedScriptExecution : INamedScriptExecution {
      public NamedScriptExecution() { }

      public NamedScriptExecution(string scriptName, IScriptExecutionResult scriptResult) {
         Name = scriptName;
         if (scriptResult != null) {
            Id = scriptResult.Id;
            Reason = scriptResult.Reason;
            State = scriptResult.State;
            OutputObjectsFormat = scriptResult.OutputObjectsFormat;
            StarTime = scriptResult.StarTime;
            EndTime = scriptResult.EndTime;
         }
      }

      public string Name { get; set;  }
      #region IScriptExecutionResult
      public string Id { get; set; }
      public ScriptState State { get; set; }
      public string Reason { get; set; }
      public OutputObjectsFormat OutputObjectsFormat { get; set; }
      public DateTime? StarTime { get; set; }
      public DateTime? EndTime { get; set; }
      public bool IsSystem { get; set; }

      #endregion

      public override bool Equals(object obj) {
         bool isInterface = true;
         INamedScriptExecution source = obj as INamedScriptExecution;
         if (source == null) {
            isInterface = false;
         }

         return isInterface &&
                source.Id == Id &&
                source.Name == Name &&
                source.State == State &&
                source.OutputObjectsFormat == OutputObjectsFormat &&
                source.StarTime == StarTime &&
                source.EndTime == EndTime &&
                source.IsSystem == IsSystem;
      }

      public override int GetHashCode() {
         unchecked // Overflow is fine, just wrap
         {
            int hashCode = 41;
            if (this.Id != null) {
               hashCode = hashCode * 59 + this.Id.GetHashCode();
            }
            if (this.Name != null) {
               hashCode = hashCode * 59 + this.Name.GetHashCode();
            }
            if (this.StarTime != null) {
               hashCode = hashCode * 59 + this.StarTime.GetHashCode();
            }
            if (this.EndTime != null) {
               hashCode = hashCode * 59 + this.EndTime.GetHashCode();
            }
            hashCode = hashCode * 59 + this.State.GetHashCode();
            hashCode = hashCode * 59 + this.OutputObjectsFormat.GetHashCode();
            hashCode = hashCode * 59 + this.IsSystem.GetHashCode();
            return hashCode;
         }
      }
   }
}
