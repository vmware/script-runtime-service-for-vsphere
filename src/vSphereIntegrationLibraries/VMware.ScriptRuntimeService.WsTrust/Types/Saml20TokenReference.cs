// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace VMware.ScriptRuntimeService.WsTrust.Types
{
   // <summary>
   /// Custom type that is used for the serialization of a SAML token reference.
   /// Produces a SecurityTokenReference XML element.
   /// </summary>
   [Serializable]
   [XmlType(
      Namespace = WssXmlConstants.WssSecurityExtension10Namespace,
      TypeName = WssXmlConstants.SecurityTokenReference)]
   public class Saml20TokenReference
   {

      [XmlAttribute(Namespace = WssXmlConstants.WssSecurityExtension11Namespace)]
      public string TokenType { get; set; }

      [XmlElement(
         ElementName = WssXmlConstants.KeyIdentifier)]
      public SamlTokenIdentifier SamlTokenId { get; set; }

      // Default constructor required by XmlSerializer
      public Saml20TokenReference()
      {
         TokenType = WssXmlConstants.WssSamlTokenProfile11SamlVersion20;
      }

      public Saml20TokenReference(string assertionId) : this()
      {
         SamlTokenId = new SamlTokenIdentifier { Id = assertionId };
      }
   }

   /// <summary>
   /// Custom type that is used for the serialization of the
   /// KeyIdentifier section of a SAML token reference element.
   /// </summary>
   [Serializable]
   public class SamlTokenIdentifier
   {
      [XmlAttribute(AttributeName = WssXmlConstants.ValueType)]
      public string Type { get; set; }

      [XmlText]
      public string Id { get; set; }

      public SamlTokenIdentifier()
      {
         Type = WssXmlConstants.WssSamlTokenProfile11SamlId;
      }
   }
}
