// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using VMware.ScriptRuntimeService.WsTrust.Types;

namespace VMware.ScriptRuntimeService.WsTrust
{
   /// <summary>
   ///    A <see cref="SignedXml" /> implementation that can
   ///    reference elements identified by an WS-Security Id.
   /// </summary>
   internal class WsuSignedXml : SignedXml
   {

      public WsuSignedXml(XmlDocument xml)
         : base(xml) { }

      public WsuSignedXml(XmlElement xmlElement)
         : base(xmlElement) { }

      public override XmlElement GetIdElement(XmlDocument document, string idValue)
      {
         // Check if there is a standard ID attribute
         XmlElement idElem = base.GetIdElement(document, idValue);

         if (idElem == null)
         {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("wsu", WssXmlConstants.WssSecurityUtility10Namespace);

            // An xpath that matches elements with the specified wsd:Id attribute value.
            string xpath = string.Format("//*[@wsu:Id=\"{0}\"]", idValue);

            idElem = document.SelectSingleNode(xpath, nsManager) as XmlElement;
         }

         return idElem;
      }
   }
}
