// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VMware.IdentityModel
{
   internal static class SecurityUtils
   {
      public static DateTime MaxUtcDateTime {
         get {
            // + and -  TimeSpan.TicksPerDay is to compensate the DateTime.ParseExact (to localtime) overflow.
            return new DateTime(DateTime.MaxValue.Ticks - TimeSpan.TicksPerDay, DateTimeKind.Utc);
         }
      }

      public static DateTime MinUtcDateTime {
         get {
            // + and -  TimeSpan.TicksPerDay is to compensate the DateTime.ParseExact (to localtime) overflow.
            return new DateTime(DateTime.MinValue.Ticks + TimeSpan.TicksPerDay, DateTimeKind.Utc);
         }
      }
   }

   internal static class EmptyReadOnlyCollection<T>
   {
      public static ReadOnlyCollection<T> Instance = new ReadOnlyCollection<T>(new List<T>());
   }
}
