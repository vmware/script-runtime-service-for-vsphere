// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Runtime.Serialization;
using VMware.ScriptRuntimeService.AdminApi.DataTypes;

namespace VMware.ScriptRuntimeService.AdminApi.Exceptions {
   [Serializable]
   internal class PodNotFoundException : Exception {
      private PodType _podType;
      private string _label;

      public PodNotFoundException() {
      }

      public PodNotFoundException(string message) : base(message) {
      }

      public PodNotFoundException(string message, Exception innerException) : base(message, innerException) {
      }

      public PodNotFoundException(PodType podType, string label) {
         _podType = podType;
         _label = label;
      }

      protected PodNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {
      }

      public override string ToString() {
         return base.ToString() + $" Pod type: {_podType}. Label: {_label}";
      }
   }
}
