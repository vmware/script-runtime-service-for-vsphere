// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Runtime;

namespace VMware.IdentityModel.Tokens
{
   public abstract class SecurityToken
   {
      public abstract string Id { get; }
      public abstract ReadOnlyCollection<SecurityKey> SecurityKeys { get; }
      public abstract DateTime ValidFrom { get; }
      public abstract DateTime ValidTo { get; }


      public virtual T CreateKeyIdentifierClause<T>() where T : SecurityKeyIdentifierClause
      {
         if (typeof(T) == typeof(LocalIdKeyIdentifierClause) && this.CanCreateLocalKeyIdentifierClause())
         {
            return new LocalIdKeyIdentifierClause(this.Id, base.GetType()) as T;
         }
         throw new NotSupportedException(String.Format("\'{0}\' does not support \'{1}\' creation.", new object[]
         {
            base.GetType().Name,
            typeof(T).Name
         }));
      }

      // System.IdentityModel.Tokens.SecurityToken
      private bool CanCreateLocalKeyIdentifierClause()
      {
         return this.Id != null;
      }
   }
}
