// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// PlaceholderValueList object represents list of argument script placeholder values.
   /// </summary>
   public class PlaceholderValueList {
      /// <summary>
      /// List of script placeholder values.
      /// </summary>
      [Required]
      [DataMember(Name = "values", IsRequired = true)]
      public PlaceholderValue[] Values { get; set; }
   }
}
