// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.Properties;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// ErrorDetails object gives information for an API error.
   /// </summary>
   [DataContract(Name = "error_details")]
   [ReadOnly(true)]
   public class ErrorDetails {
      public ErrorDetails(int code, string message, string details = null) {
         Code = code;
         Message = message;
         Details = details;
      }

      public ErrorDetails(Exception exc) {
         Code = ApiErrorCodes.Unknown;
         Message = exc?.Message;
         Details = exc?.ToString();
      }

      /// <summary>
      /// Code of the internal service error. It is useful when error is reported to service maintainers.
      /// </summary>
      [DataMember(Name = "code")]
      public int Code { get; set; }

      /// <summary>
      /// Error message.
      /// </summary>
      [DataMember(Name = "error_message")]
      public string Message { get; set; }

      /// <summary>
      /// Details of the error.
      /// </summary>
      [DataMember(Name = "details")]
      public string Details { get; set; }
   }
}
