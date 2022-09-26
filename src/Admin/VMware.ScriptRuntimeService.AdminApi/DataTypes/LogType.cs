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
   public enum LogType {      
      [EnumMember(Value = "setup")]
      Setup = 0,

      [EnumMember(Value = "api-gateway")]
      ApiGateway = 1,

      [EnumMember(Value = "admin-api")]
      AdminApi = 2
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
