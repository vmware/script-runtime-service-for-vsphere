// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.RunspaceEndpoint.DataTypes {
   [DataContract(Name = "error_response")]
   public class ErrorResponse {
      public ErrorResponse(int code, string message, string details = null) {
         Code = code;
         Message = message;
         Details = details;
      }

      [DataMember(Name = "code")]
      public int Code { get; set; }


      [DataMember(Name = "error_message")]
      public string Message { get; set; }

      [DataMember(Name = "details")]
      public string Details { get; set; }
   }
}
