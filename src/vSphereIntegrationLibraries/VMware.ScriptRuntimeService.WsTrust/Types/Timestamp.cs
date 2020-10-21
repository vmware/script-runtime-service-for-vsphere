// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VMware.ScriptRuntimeService.WsTrust.Types
{
   [Serializable]
   [XmlType(
      Namespace = WssXmlConstants.WssSecurityUtility10Namespace,
      TypeName = WssXmlConstants.Timestamp)]
   public class Timestamp
   {
      [XmlElement(Order = 0)]
      public AttributedDateTime Created { get; set; }

      [XmlElement(Order = 1)]
      public AttributedDateTime Expires { get; set; }

      [XmlAnyElement(Order = 2)]
      public XmlElement[] Items { get; set; }

      [XmlAttribute(Form = XmlSchemaForm.Qualified, DataType = "ID")]
      public string Id { get; set; }

      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr { get; set; }
   }
}
