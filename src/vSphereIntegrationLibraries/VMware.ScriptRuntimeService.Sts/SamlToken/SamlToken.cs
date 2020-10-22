// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using VMware.ScriptRuntimeService.Sts.Properties;

namespace VMware.ScriptRuntimeService.Sts.SamlToken {
   public class SamlToken : ISamlToken {

      public SamlToken(string rawXml) {
         Parse(rawXml);
         RawXml = rawXml;
      }

      public DateTime StartTime { get; private set; }

      public DateTime ExpirationTime { get; private set; }

      public ConfirmationType ConfirmationType { get; private set; }

      public string RawXml { get; }

      public XmlElement RawXmlElement { get; private set; }

      public string SubjectNameId { get; private set; }

      public string Id { get; private set; }

      public X509Certificate2 ConfirmationCertificate{ get; private set; }

      private void Parse(string rawXml) {
         try {
            var samlTokenXml = new XmlDocument();

            // Load SAML Token in XmlDocument
            samlTokenXml.LoadXml(rawXml);

            RawXmlElement = samlTokenXml.DocumentElement;

            Id = GetSamlTokenNodeValue(samlTokenXml, "/saml2:Assertion/@ID");

            SubjectNameId = GetSamlTokenNodeValue(samlTokenXml, "/saml2:Assertion/saml2:Subject/saml2:NameID");

            StartTime = DateTime.Parse(
               GetSamlTokenNodeValue(samlTokenXml, "/saml2:Assertion/saml2:Conditions/@NotBefore"));

            ExpirationTime = DateTime.Parse(
               GetSamlTokenNodeValue(samlTokenXml, "/saml2:Assertion/saml2:Conditions/@NotOnOrAfter"));

            ConfirmationType = GetSamlConfirmationType(
               GetSamlTokenNodeValue(samlTokenXml, "/saml2:Assertion/saml2:Subject/saml2:SubjectConfirmation/@Method"));

            ConfirmationCertificate = GetConfirmationCertificate(
               GetSamlTokenNodeValue(
                  samlTokenXml,
                  "/saml2:Assertion/saml2:Subject/saml2:SubjectConfirmation/saml2:SubjectConfirmationData/ds:KeyInfo/ds:X509Data/ds:X509Certificate",
                  false));

         } catch (InvalidSamlTokenException) {
            throw;
         } catch (Exception exc) {
            throw new InvalidSamlTokenException(Resources.SamlToken_Parse_UnexpectedError, exc);
         }
      }

      private X509Certificate2 GetConfirmationCertificate(string rawCertificate) {
         X509Certificate2 result = null;

         if (!string.IsNullOrEmpty(rawCertificate)) {
            result = new X509Certificate2(Convert.FromBase64String(rawCertificate));
         }

         return result;
      }

      private string GetSamlTokenNodeValue(XmlDocument samlTokenXml, string xPath, bool nodeMustPresent = true) {
         var nsmgr = new XmlNamespaceManager(samlTokenXml.NameTable);
         nsmgr.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
         nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

         var nodes =  samlTokenXml.SelectNodes(xPath, nsmgr);
         string result = null;

         if (nodes != null && nodes.Count > 0) {
            result = nodes[0].InnerText;
         }

         if (result == null && nodeMustPresent) {
            throw new InvalidSamlTokenException(
               string.Format(Resources.SamlToken_XML_MissingNode, xPath));
         }

         return result;
      }

      private ConfirmationType GetSamlConfirmationType(string confirmationType) {
         if (confirmationType == "urn:oasis:names:tc:SAML:2.0:cm:holder-of-key") {
            return ConfirmationType.HolderOfKey;
         }

         if (confirmationType == "urn:oasis:names:tc:SAML:2.0:cm:bearer") {
            return ConfirmationType.Bearer;
         }

         throw new InvalidSamlTokenException(
            string.Format(Resources.SamlToken_UnknownConfirmationType, confirmationType));
      }
   }
}
