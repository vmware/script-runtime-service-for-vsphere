// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VMware.Http.Sso.Authentication.Properties;

namespace VMware.Http.Sso.Authentication.Impl {
   public class Nonce : IComparable<Nonce> {
      private const char DELIMITER = ':';
      private DateTime _createdOn;
      private string _appender;
      private static Random _random = new Random();

      #region Constructors

      public Nonce() { }

      private Nonce(DateTime createdOn): this(createdOn, Math.Abs(_random.Next()).ToString()) {
      }

      private Nonce(DateTime createdOn, string appender) {
         // Trim Ticks in milliseconds
         _createdOn = new DateTime(
            createdOn.Year, 
            createdOn.Month, 
            createdOn.Day, 
            createdOn.Hour, 
            createdOn.Minute, 
            createdOn.Second, 
            createdOn.Millisecond);
         _appender = appender;
      }
      
      public static Nonce FromNow() {
         return FromDate(DateTime.Now);
      }

      public static Nonce FromDate(DateTime createOn) {
         return new Nonce(createOn);
      }

      public static Nonce FromString(string nonce) {
         if (string.IsNullOrEmpty(nonce)) {
            throw new AuthException(Resources.Incorrect_Nonce_String_Value);

         }

         var parts = nonce.Split(DELIMITER);
         if (parts == null || 
             parts.Length != 2 || 
             string.IsNullOrEmpty(parts[0]) || 
             string.IsNullOrEmpty(parts[1])) {
            throw new AuthException(Resources.Incorrect_Nonce_String_Value);
         }
         var unixTimeCreatedOn = long.Parse(parts[0]);
         var createdOn = FromUnixTime(unixTimeCreatedOn);
         return new Nonce(createdOn, parts[1]);
      }
      #endregion

      #region Overrides
      public override int GetHashCode() {
         return _createdOn.GetHashCode() * 13 + _appender.GetHashCode();
      }

      public override bool Equals(object obj) {
         var compareTo = obj as Nonce;
         var result = false;

         if (compareTo != null) {
            result = _createdOn.Equals(compareTo._createdOn) &&
                     _appender.Equals(compareTo._appender);
         }

         return result;
      }

      public override string ToString() {
         return $"{GetCreatedOnUnixTime()}{DELIMITER}{_appender}";
      } 
      #endregion

      private long GetCreatedOnUnixTime() {
         return ((DateTimeOffset)_createdOn.ToUniversalTime()).ToUnixTimeMilliseconds();
      }
      private static DateTime FromUnixTime(long unixTime) {
         return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).LocalDateTime;
      }

      public int CompareTo(Nonce other) {
         if (ReferenceEquals(this, other)) return 0;
         if (ReferenceEquals(null, other)) return 1;
         var createdOnComparison = _createdOn.CompareTo(other._createdOn);
         return createdOnComparison;
      }
   }
}
