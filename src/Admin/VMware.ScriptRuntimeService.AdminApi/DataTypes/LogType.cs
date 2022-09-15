// **************************************************************************
//  Copyright 2020-2022 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VMware.ScriptRuntimeService.AdminApi.DataTypes {
   [System.Text.Json.Serialization.JsonConverter(typeof(StringEnumConverter))]
   [ModelBinder(typeof(LogTypeJsonModelBinder))]
   [DataContract(Name = "log_type")]
   [ReadOnly(true)]
   [Flags]
   public enum LogType {
      [EnumMember(Value = "none")]
      None = 0,
      
      [EnumMember(Value = "setup")]
      Setup = 1,

      [EnumMember(Value = "api-gateway")]
      ApiGateway = 2,

      [EnumMember(Value = "admin-api")]
      AdminApi = 4,

      [EnumMember(Value = "all")]
      All = 255
   }

   public class LogTypeJsonModelBinder : IModelBinder {
      public Task BindModelAsync(ModelBindingContext bindingContext) {
         string rawData = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
         rawData = JsonConvert.SerializeObject(rawData); //turns value to valid json
         try {
            LogType result = JsonConvert.DeserializeObject<LogType>(rawData); //manually deserializing value
            bindingContext.Result = ModelBindingResult.Success(result);
         } catch (JsonSerializationException) {
            //do nothing since "failed" result is set by default
         }

         return Task.CompletedTask;
      }
   }
}
