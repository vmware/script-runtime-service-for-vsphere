// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VMware.ScriptRuntimeService.WsTrust.Types;

namespace VMware.ScriptRuntimeService.WsTrust.Types
{
   [Serializable]
   [XmlType(
   Namespace = WssXmlConstants.WssSecurityExtension10Namespace,
   TypeName = WssXmlConstants.Security)]
public class SecurityHeader
{
   [XmlElement(Namespace = WssXmlConstants.WssSecurityUtility10Namespace)]
   public Timestamp Timestamp { get; set; }

   [XmlAnyElement(Name = WssXmlConstants.UsernameToken)]
   public XmlElement UsernameToken { get; set; }

   [XmlAnyElement(Name = WssXmlConstants.BinarySecurityToken)]
   public XmlElement BinarySecurityToken { get; set; }

   [XmlAnyElement(Name = WssXmlConstants.Assertion, Namespace = WssXmlConstants.Saml20AssertionNamespace)]
   public XmlElement SamlToken { get; set; }

   [XmlAnyAttribute]
   public XmlAttribute[] AnyAttr { get; set; }
}
}
