// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VMware.ScriptRuntimeService.WsTrust.Types
{
   [Serializable]
   [XmlType(Namespace = WssXmlConstants.WssSecurityUtility10Namespace)]
   public class AttributedDateTime
   {
      [XmlAttribute(Form = XmlSchemaForm.Qualified, DataType = "ID")]
      public string Id { get; set; }

      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr { get; set; }

      [XmlText]
      public string StringValue { get; set; }

      [XmlIgnore]
      public DateTime Value {
         set { StringValue = Util.ToXmlDateTime(value); }
      }
   }
}
