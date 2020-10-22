// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using VMware.IdentityModel.Tokens;
using VMware.ScriptRuntimeService.WsTrust.SecurityContext;
using VMware.ScriptRuntimeService.WsTrust.Types;
using VMware.ServiceModel.Security;

namespace VMware.ScriptRuntimeService.WsTrust
{
   /// <summary>
   ///    Message interceptor that modifies the message to include the
   ///    WS-Security protocol headers.
   /// </summary>
   public sealed class WsTrustClientMessageInspector : IClientMessageInspector
   {
      private const string SOAP_ENVELOPE_NS = "http://schemas.xmlsoap.org/soap/envelope/";
      private const string SOAP_ENVELOPE_ELEMENT_NAME = "Envelope";
      private const string SOAP_HEADER_ELEMENT_NAME = "Header";

      private const string SECURITY_TOKEN_ID_PREFIX = "SecurityToken";
      private const string TIMESTAMP_ID_PREFIX = "Timestamp";
      private const string BODY_ID_PREFIX = "Id";

      /// <summary>
      ///    Headers are buffered.
      ///    The maximum size in bytes of a header.
      /// </summary>
      private const int MAX_SIZE_OF_HEADERS = 1 * 1024 * 1024; //1MB should be enough to hold a token.

      /// <summary>
      ///    The time span a request is considered valid.
      /// </summary>
      private static readonly TimeSpan RequestTimestampValidPeriod = new TimeSpan(0, 0, 5, 0);

      private readonly WSSecurityTokenSerializer _tokenSerializer =
         new WSSecurityTokenSerializer(SecurityVersion.WSSecurity10, true);

      public WsTrustClientMessageInspector()
      {
      }

      private static string CreateElementId(string prefix)
      {
         return string.Format("{0}-{1:D}", prefix, Guid.NewGuid());
      }

      #region IClientMessageInspector

      public void AfterReceiveReply(ref Message reply, object correlationState)
      {
         // nothing needs to be done on reply.
      }

      public object BeforeSendRequest(ref Message request, IClientChannel channel)
      {

         // Retrieve the security context if specified
         var securityContext = WsSecurityContext.GetProperties(request);

         if (securityContext == null)
         {
            // exit with unmodified message
            return null;
         }

         ValidateSecurityContext(securityContext);

         // Working with the message in XML form
         XmlDocument soapRequest = GetXmlMessage(request);

         // Setting an ID to the body to be able to reference it in the signature (to be able to sign it)
         string bodyId = CreateElementId(BODY_ID_PREFIX);
         SetIdToBodyElement(soapRequest.DocumentElement, bodyId);

         SecurityHeader wsSecurityHeader = GetBasicUnsignedHeader();

         if (!String.IsNullOrEmpty(securityContext.Credentials.User))
         {
            AppendUserNameToken(
               wsSecurityHeader,
               securityContext.Credentials.User,
               securityContext.Credentials.Password);
         }

         // A SecurityTokenReference XmlElement which contains the reference to signing client certificate.
         XmlElement keyIdentifierClause = null;

         if (securityContext.Credentials.BearerToken != null)
         {
            // No signature is used for Bearer token security context
            wsSecurityHeader.SamlToken = securityContext.Credentials.BearerToken;

         }
         else if (securityContext.Credentials.HolderOfKeyToken != null)
         {
            // For HoK token context, the signing certificate is contained within the HoK SAML token
            XmlElement hokToken = securityContext.Credentials.HolderOfKeyToken;
            wsSecurityHeader.SamlToken = hokToken;
            keyIdentifierClause = GetKeyIdentifierClause(hokToken);

         }
         else if (securityContext.Credentials.ClientCertificate != null)
         {
            // Append local certificate info as a BinarySecurityToken child  
            // element of the WS Security header and save a reference to it.
            keyIdentifierClause = AppendBinarySecurityToken(wsSecurityHeader, securityContext.Credentials.ClientCertificate);
         }

         // Converting the sso header from object to xml to easily assign xml values to properties.
         XmlElement securityHeaderXml = Util.ToXmlElement(wsSecurityHeader);

         // Attaching the header without the signature
         var ssoHeaderElement = MergeMessageWithHeader(
            soapRequest.DocumentElement,
            securityHeaderXml);

         if (keyIdentifierClause != null)
         {

            // This should already be ensured by ValidateSecurityContext() method
            Debug.Assert(securityContext.SigningKey != null);

            // Compute signature for timestamp and body elements and add the signature element to the security header.           

            var signature = Util.ComputeSignature(
               soapRequest,
               keyIdentifierClause,
               securityContext.SigningKey,
               bodyId,
               wsSecurityHeader.Timestamp.Id);

            ssoHeaderElement.AppendChild(signature);
         }

         // Convert the SOAP request back to a message that replaces the original message
         request = ToMessage(soapRequest, request.Version, request.Headers.Action, request.Properties);


         // No need to correlate requests with replays for this inspector.
         return null;
      }

      private void ValidateSecurityContext(WsSecurityContextProperties securityContext)
      {
         var credentials = securityContext.Credentials;

         // Only one of the following must be present
         var specifiedPropertyCount =
            new object[] {
               credentials.BearerToken,
               credentials.ClientCertificate,
               credentials.HolderOfKeyToken
            }.Count(property => property != null);

         if (specifiedPropertyCount > 1)
         {
            string message =
               string.Format("Invalid security context. Exactly one of the following security context attributes must be present: {0}, {1} or {2}",
                  nameof(credentials.BearerToken),
                  nameof(credentials.ClientCertificate),
                  nameof(credentials.HolderOfKeyToken));

            throw new InvalidOperationException(message);
         }

         // If a valid user name is specified, password must also be present
         if (credentials.User != null)
         {
            if (string.IsNullOrWhiteSpace(credentials.User))
            {
               throw new InvalidOperationException("Invalid security context. Invalid user name credential.");
            }

            if (credentials.Password == null)
            {
               //throw new InvalidOperationException("Invalid security context. Invalid password credential.");
            }
         }

         // If Either client certificate or HoK token is present, a signing key must also be present.
         if ((credentials.ClientCertificate != null || credentials.HolderOfKeyToken != null) && (securityContext.SigningKey == null))
         {
            throw new InvalidOperationException(
               "Invalid security context. When a client certificate credential or a holder-of-key token " +
               "credential is specified, the corresponding signing key must also be specified.");
         }
      }

      #endregion

      private XmlElement SerializeToken(SecurityToken token)
      {
         using (XmlDocumentWriterHelper documentWriterHelper = new XmlDocumentWriterHelper())
         {
            _tokenSerializer.WriteToken(documentWriterHelper.CreateDocumentWriter(), token);
            XmlDocument xmlDocument = documentWriterHelper.ReadDocument();

            return xmlDocument.DocumentElement;
         }
      }

      private Message ToMessage(
         XmlDocument xmlRequest,
         MessageVersion version,
         string action,
         MessageProperties messageProperties)
      {

         var xmlReader = Util.GetXmlReader(xmlRequest);

         var message = Message.CreateMessage(xmlReader, MAX_SIZE_OF_HEADERS, version);
         message.Headers.Action = action;
         message.Properties.CopyProperties(messageProperties);
         return message;
      }

      private static XmlDocument GetXmlMessage(Message request)
      {
         using (XmlDocumentWriterHelper documentWriterHelper = new XmlDocumentWriterHelper())
         {
            request.WriteMessage(documentWriterHelper.CreateDocumentWriter());
            return documentWriterHelper.ReadDocument();
         }
      }

      private static XmlElement MergeMessageWithHeader(
         XmlElement xmlRequest,
         XmlElement ssoHeaderXml)
      {
         Debug.Assert(xmlRequest.OwnerDocument != null);

         var headersNode = GetOrCreateHeadersNode(xmlRequest);

         var ssoHeaderNode = xmlRequest.OwnerDocument.ImportNode(ssoHeaderXml, true);

         return (XmlElement)headersNode.AppendChild(ssoHeaderNode);
      }

      private static XmlElement GetOrCreateHeadersNode(XmlElement envelopeElement)
      {
         Debug.Assert(envelopeElement != null);
         Debug.Assert(
            envelopeElement.LocalName == SOAP_ENVELOPE_ELEMENT_NAME
            && envelopeElement.NamespaceURI == SOAP_ENVELOPE_NS,
            "Expected an Envelope element.");

         XmlNodeList headerElements =
            envelopeElement.GetElementsByTagName(SOAP_HEADER_ELEMENT_NAME, SOAP_ENVELOPE_NS);

         Debug.Assert(headerElements.Count <= 1, "Found multiple Header elements in the SOAP envelope.");

         XmlElement headerElement;
         if (headerElements.Count == 0)
         {
            XmlDocument ownerDocument = envelopeElement.OwnerDocument;
            Debug.Assert(ownerDocument != null);

            string soapPrefix = envelopeElement.GetPrefixOfNamespace(SOAP_ENVELOPE_NS);
            Debug.Assert(soapPrefix != null);

            headerElement = ownerDocument.CreateElement(soapPrefix, SOAP_HEADER_ELEMENT_NAME, SOAP_ENVELOPE_NS);
            envelopeElement.AppendChild(headerElement);
         }
         else
         {
            headerElement = (XmlElement)headerElements[0];
         }

         return headerElement;
      }

      private SecurityHeader GetBasicUnsignedHeader()
      {

         // The header should have a mustUnderstand attribute value of 1
         var mustUnderstandAttribute =
            Util.CreateXmlAttribute("mustUnderstand", SOAP_ENVELOPE_NS, "1");

         DateTime createdTime = DateTime.Now;
         DateTime expiresTime = createdTime + RequestTimestampValidPeriod;

         string timestampId = CreateElementId(TIMESTAMP_ID_PREFIX);

         SecurityHeader ssoHeader = new SecurityHeader
         {
            Timestamp = new Timestamp
            {
               Id = timestampId,
               Created =
                  new AttributedDateTime
                  {
                     Value = createdTime
                  },
               Expires =
                  new AttributedDateTime
                  {
                     Value = expiresTime
                  }
            },
            AnyAttr = new[] { mustUnderstandAttribute }
         };

         return ssoHeader;
      }

      private void AppendUserNameToken(SecurityHeader wsSecurityHeader, string userName, SecureString password)
      {

         // Reusing the WCF UserNameSecurityToken as there is a built-in serializer for this type.

         UserNameSecurityToken userNameToken = null;

         string tokenId = CreateElementId(SECURITY_TOKEN_ID_PREFIX);
         userNameToken = new UserNameSecurityToken(userName, Util.ConvertSecureStringToString(password), tokenId);

         wsSecurityHeader.UsernameToken = SerializeToken(userNameToken);
      }

      /// <summary>
      /// Appends client certificate information as a BinarySecurityToken XML element 
      /// under the root Security element.
      /// </summary>
      /// <returns>
      /// A SecurityTokenReference element which references to the appended client certificate.
      /// </returns>
      private XmlElement AppendBinarySecurityToken(SecurityHeader wsSecurityHeader, X509Certificate2 clientCertificate)
      {

         XmlElement result = null;

         // Reusing the WCF X509SecurityToken as there is a built-in serializer for this type.

         string tokenId = CreateElementId(SECURITY_TOKEN_ID_PREFIX);
         using (var certificateToken = new X509SecurityToken(clientCertificate, tokenId))
         {
            var binarySecurityToken = SerializeToken(certificateToken);
            wsSecurityHeader.BinarySecurityToken = binarySecurityToken;

            result = GetKeyIdentifierClause(certificateToken);
         }

         return result;
      }

      private static void SetIdToBodyElement(XmlElement envelopeElement, string bodyId)
      {
         Debug.Assert(envelopeElement != null);
         Debug.Assert(
            envelopeElement.LocalName == SOAP_ENVELOPE_ELEMENT_NAME
            && envelopeElement.NamespaceURI == SOAP_ENVELOPE_NS,
            "Expected an Envelope element.");

         XmlNodeList elements = envelopeElement.GetElementsByTagName("Body", SOAP_ENVELOPE_NS);
         Debug.Assert(elements.Count == 1, "Expected one Body element in SOAP envelope.");

         var bodyElement = (XmlElement)elements[0];
         SetIdToElement(bodyElement, bodyId, WssXmlConstants.WssSecurityUtility10Namespace);
      }

      private static void SetIdToElement(
         XmlElement element,
         string value,
         string namespaceUri)
      {
         const string attributeName = "Id";

         XmlAttribute attributeNode = element.GetAttributeNode(attributeName, namespaceUri);

         if (attributeNode == null)
         {
            // Note: Creating an attribute without a prefix would cause underterministic
            // serialization of the XmlDocument to text and cause issues with signature
            // validity.
            attributeNode =
               Util.CreateXmlAttribute(element, "wsu", attributeName, namespaceUri, value);
         }

         attributeNode.Value = value;
      }

      /// <summary>
      /// Returns a SecurityTokenReference element which contains a reference 
      /// by id to a client certificate that is located in the same XML document.
      /// </summary>
      private XmlElement GetKeyIdentifierClause(X509SecurityToken certToken)
      {
         var keyIdentifierClause =
            certToken.CreateKeyIdentifierClause<LocalIdKeyIdentifierClause>();

         Debug.Assert(keyIdentifierClause != null);

         using (XmlDocumentWriterHelper documentWriterHelper = new XmlDocumentWriterHelper())
         {
            _tokenSerializer.WriteKeyIdentifierClause(
               documentWriterHelper.CreateDocumentWriter(), keyIdentifierClause);

            XmlDocument xmlDocument = documentWriterHelper.ReadDocument();

            return xmlDocument.DocumentElement;
         }
      }

      /// <summary>
      /// Returns a SecurityTokenReference element which contains a reference 
      /// by id to the parent SAML token with a client certicate inside.
      /// </summary>
      private XmlElement GetKeyIdentifierClause(XmlElement samlToken)
      {
         // Retrieve the id of the provided SAML token XML element

         XmlNode idAttribute = samlToken.Attributes.GetNamedItem("ID");

         if (idAttribute == null)
         {
            throw new InvalidDataException("The specified SAML token does not have an ID attribute.");
         }

         string samlTokenId = idAttribute.Value;

         // There are no built-in classes in .net core that can be used to serialize SAML token reference.
         // Using custom user types to perform the serialization.
         var keyIdentifierClause = new Saml20TokenReference(samlTokenId);

         return
            Util.ToXmlElement(keyIdentifierClause, false);
      }
   }
}
