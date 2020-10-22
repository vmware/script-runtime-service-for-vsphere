// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;

namespace VMware.ScriptRuntimeService.APIGateway.SystemScripts {
   public class PCLIScriptsGenerator {
      private string _templateId;
      private PlaceholderValueList[] _placeholderValueList;

      public PCLIScriptsGenerator(string templateId, PlaceholderValueList[] placeholderValueListList) {
         _templateId = templateId;
         _placeholderValueList = placeholderValueListList;
      }

      public ArgumentScript Generate() {
         var result = new ArgumentScript() {
            TemplateId = _templateId,
            PlaceholderValueList = _placeholderValueList
         };
         if (_placeholderValueList?.Length > 0) {
            var resultBuilder = new StringBuilder();
            foreach (var argumentScriptParameters in _placeholderValueList) {
               if (argumentScriptParameters.Values?.Length > 0) {
                  string singleResult = _templateId;
                  foreach (var argumentScriptParameter in argumentScriptParameters.Values) {
                     // Replacing "<param_name>" with value in the templateId.
                     // Value is represented as on line array of strings
                     singleResult = singleResult.Replace(
                        $"<{argumentScriptParameter.PlaceholderName}>", 
                        ("'" + string.Join("','", argumentScriptParameter.Value) + "'"));
                  }

                  resultBuilder.AppendLine(singleResult);
               }
            }

            result = new ArgumentScript(resultBuilder.ToString().Trim());
            result.TemplateId = _templateId;
            result.PlaceholderValueList = _placeholderValueList;
         }
         return result;
      }
   }
}
