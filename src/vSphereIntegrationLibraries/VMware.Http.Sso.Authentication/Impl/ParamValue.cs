// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMware.Http.Sso.Authentication.Impl {
   public class ParamValue {
      public enum Param {
         token,
         nonce,
         bodyhash,
         signature_alg,
         signature
      }

      public ParamValue(Param key, string value) {
         if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
         Key = key;
         Value = value;
      }

      public override int GetHashCode() {
         return 31 * Key.GetHashCode() + Value.GetHashCode();
      }

      public override bool Equals(object obj) {
         return obj is ParamValue compareTo &&
                Key.Equals(compareTo.Key) &&
                Value.Equals(compareTo.Value);
      }

      public override string ToString() {
         return $"Param({Key}, {Value})";
      }

      public ParamValue TrimLeft(int chars) {
         return chars >= Value.Length ? 
            null :
            chars == 0 ? 
               this : 
               new ParamValue(Key, Value.Substring(chars));
      }

      public ParamValue First(int chars) {
         return chars >= Value.Length ? 
            this : 
            chars == 0 ? 
               null : 
               new ParamValue(Key, Value.Substring(0, chars));
      }

      public ParamValue Concat(ParamValue param) {
         return new ParamValue(Key, Value + param.Value);
      }

      public Param Key { get; }

      public string Value { get; }
   }

}
