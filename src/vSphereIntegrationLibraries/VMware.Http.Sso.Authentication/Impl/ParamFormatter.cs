// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VMware.Http.Sso.Authentication.Impl {
   public class ParamFormatter {
      private const char EQ = '=';
      private const char QUOTE = '"';
      private const string KEY_GROUP_NAME= "key";
      private const string VALUE_GROUP_NAME = "value";

      private static string PARAM_REG_EX = 
         $"(?<{KEY_GROUP_NAME}>{ParamValue.Param.nonce}|{ParamValue.Param.token}|{ParamValue.Param.bodyhash}|{ParamValue.Param.signature_alg}|{ParamValue.Param.signature})=\"(?<{VALUE_GROUP_NAME}>" + "[\\u0020-\\u007E&^\\u0022&^\\u005C]+)\"";

      public static StringBuilder Format(ParamValue param, StringBuilder sb) {
         sb.Append(param.Key);
         sb.Append(EQ);
         sb.Append(QUOTE);
         sb.Append(param.Value);
         sb.Append(QUOTE);
         return sb;
      }

      public static ParamValue Parse(string param) {
         Regex regex = new Regex(PARAM_REG_EX);

         var match = regex.Match(param);

         ParamValue result = null;
         if (Enum.TryParse<ParamValue.Param>(match.Groups[KEY_GROUP_NAME].Value, true, out var key)) {
            var value = match.Groups[VALUE_GROUP_NAME].Value;
            result = new ParamValue(key, value);
         }
         

         return result;
      }

      public static int ValueLength(ParamValue param, int txtMaxLength) {
         int supplementaryCharsNb = param.Key.ToString().Length + 3;
         int maxValueLength = Math.Max(0, txtMaxLength - supplementaryCharsNb);

         int valueLength = Math.Min(maxValueLength, param.Value.Length);

         return valueLength;
      }
   }
}