// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace VMware.ServiceModel.Security
{
   public abstract class SecurityVersion
   {
      internal SecurityVersion()
      {
      }
      public static SecurityVersion WSSecurity10 {
         get { return SecurityVersion10.Instance; }
      }

      internal class SecurityVersion10 : SecurityVersion
      {
         private static readonly SecurityVersion10 s_instance = new SecurityVersion10();

         protected SecurityVersion10()
         {
         }

         public static SecurityVersion10 Instance {
            get { return s_instance; }
         }

         public override string ToString()
         {
            return "WSSecurity10";
         }
      }
   }
}
