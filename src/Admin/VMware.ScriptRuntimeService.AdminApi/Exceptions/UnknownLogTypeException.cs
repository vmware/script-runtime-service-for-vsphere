// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;

namespace VMware.ScriptRuntimeService.AdminApi.Exceptions {
   [Serializable]
   internal class UnknownLogTypeException : Exception {
      private readonly LogType _logType;

      public UnknownLogTypeException() {
      }

      public UnknownLogTypeException(LogType logType) {
         _logType = logType;
      }
      
      public UnknownLogTypeException(LogType logType, string message) : base(message) {
         _logType = logType;
      }

      public UnknownLogTypeException(string message) : base(message) {
      }

      public UnknownLogTypeException(string message, Exception innerException) : base(message, innerException) {
      }

      protected UnknownLogTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {
      }

      public LogType LogType {
         get {
            return _logType;
         }
      }

      public override string ToString() {
         return base.ToString() + $"LogType '{_logType}' is not assosiated with any log source.";
      }
   }
}
