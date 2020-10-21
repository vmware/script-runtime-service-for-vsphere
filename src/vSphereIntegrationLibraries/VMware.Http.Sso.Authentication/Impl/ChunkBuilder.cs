// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Text;

namespace VMware.Http.Sso.Authentication.Impl {
   public class ChunkBuilder {
      private const char SP = ' ';
      private const char COMMA = ',';
      private int _maxLength;
      private StringBuilder _sb = new StringBuilder();
      private bool _firstParam = true;

      public ChunkBuilder(int maxLength, String prefix) {
         _maxLength = maxLength;
         if (prefix != null) {
            _sb.Append(prefix);
         }
      }

      public ChunkBuilder(int maxLength): this(maxLength, null) { }

      public ParamValue Append(ParamValue param) {
         int maxParamLength = _maxLength - _sb.Length - (_firstParam ? 0 : 2);
         int valueLength = maxParamLength < 0 ? 0 : ParamFormatter.ValueLength(param, maxParamLength);

         ParamValue toAppend = param.First(valueLength);
         if (toAppend != null) {
            if (!_firstParam) {
               _sb.Append(COMMA);
               _sb.Append(SP);
            }
            ParamFormatter.Format(toAppend, _sb);
         }
         _firstParam = false;
         return param.TrimLeft(valueLength);
      }

      public string GetValue() {
         return _sb.ToString();
      }
   }
}
