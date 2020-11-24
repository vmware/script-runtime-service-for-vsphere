// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes {
   /// <summary>
   /// ArgumentScript object represents a script produced from a given script template and placeholder values.
   /// </summary>
   [DataContract(Name = "argument_script")]
   public class ArgumentScript {
      public ArgumentScript() : this(null) { }

      public ArgumentScript(string resultScript) {
         Script = resultScript;
      }
      /// <summary>
      /// Unique identifier for the argument template script.
      /// </summary>
      [Required]
      [DataMember(Name = "template_id", IsRequired = true)]
      public string TemplateId { get; set; }

      /// <summary>
      /// Placeholder value list which are used to create script from script template.
      /// 
      /// Single template_placeholder_value_list produces script by the given template replacing placeholder with the given values.
      /// Multiple items for template_placeholder_value_list produce a script of scripts which can produce an array of objects. Each template_placeholder_value_list item is used to produce script from template. Scripts are then combined in a multi-line script where each line produces result object.
      /// </summary>
      [Required]
      [DataMember(Name = "template_placeholder_value_list", IsRequired = true)]
      public PlaceholderValueList[] PlaceholderValueList { get; set; }

      /// <summary>
      /// Script result produced by the service based on given template_id and template_placeholder_parameters
      /// </summary>
      [DataMember(Name = "script")]
      [ReadOnly(true)]
      public string Script { get; private set; }
   }
}
