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
   /// PlaceholderValue object represents a single value for a placeholder in template script.
   /// </summary>
   [DataContract(Name="placeholder_value")]
   public class PlaceholderValue {
      /// <summary>
      /// The name of the placeholder in the script template which will be replaced on the creation of an argument script.
      /// </summary>
      [Required]
      [DataMember(Name = "placeholder_name", IsRequired = true)]
      public string PlaceholderName { get; set; }

      /// <summary>
      /// Value that will be used to replace the placeholder on the creation of an argument script.
      /// </summary>
      [Required]
      [DataMember(Name = "value", IsRequired = true)]
      public string[] Value { get; set; }
   }
}
