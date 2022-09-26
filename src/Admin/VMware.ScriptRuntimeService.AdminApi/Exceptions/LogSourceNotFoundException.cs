// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;

namespace VMware.ScriptRuntimeService.AdminApi.Exceptions {
   [Serializable]
   internal class LogSourceNotFoundException : Exception {
      private readonly LogType _podType;
      private readonly string _label;

      public LogSourceNotFoundException() {
      }

      public LogSourceNotFoundException(string message) : base(message) {
      }

      public LogSourceNotFoundException(string message, Exception innerException) : base(message, innerException) {
      }

      public LogSourceNotFoundException(LogType podType, string label) {
         _podType = podType;
         _label = label;
      }

      protected LogSourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
      }

      public override string ToString() {
         return base.ToString() + $" No log source with type '{_podType}' and label '{_label}' found";
      }
   }
}
