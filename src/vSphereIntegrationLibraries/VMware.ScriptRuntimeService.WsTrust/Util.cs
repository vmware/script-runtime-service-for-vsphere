// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VMware.ScriptRuntimeService.WsTrust
{
   internal class Util
   {

      #region XML Utils

      /// <summary>
      ///    Converts a <see cref="DateTime" /> <paramref name="value" />
      ///    to its XML representation with a precision of 3 digits after
      ///    the decimal point of the seconds part.
      /// </summary>
      public static string ToXmlDateTime(DateTime value)
      {
         // '-', ':', and '.' are special date time format symbols and need to be escaped (wrapped in '')
         // to prevent the .Net runtime from replacing them with characters from the Operating System locale settings
         string result = value.ToUniversalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'ss'.'fffZ");
         return result;
      }

      /// <summary>
      /// Converts the specified <paramref name="value"/> to <see cref="XmlElement"/>.
      /// </summary>
      ///
      /// <param name="value">
      /// Value to serialize. If the declaring type specifies an <see cref="XmlTypeAttribute.Namespace"/>
      /// attribute value, it is set as default namespace to the returned root XML element.
      /// </param>
      /// <param name="includeStandardNamespaceDeclarations">
      /// Specifies wether the returned root XML element will explicitly declare standard XML namespaces,
      /// such as the XML schema and instance namespaces.
      /// </param>
      public static XmlElement ToXmlElement(object value, bool includeStandardNamespaceDeclarations = true)
      {

         Type typeToSerialize = value.GetType();

         // Retrive the default XML namespace from the XmlType attribute: [XmlType(Namespace = ...)]
         string defaultNamespace =
            (typeToSerialize.GetCustomAttributes(typeof(XmlTypeAttribute), true)
               .FirstOrDefault() as XmlTypeAttribute)
               ?.Namespace;

         XmlSerializer serializer = (defaultNamespace != null)
            ? new XmlSerializer(typeToSerialize, defaultNamespace)
            : new XmlSerializer(typeToSerialize);

         using (XmlDocumentWriterHelper documentWriterHelper = new XmlDocumentWriterHelper())
         {
            XmlWriter xmlWriter = documentWriterHelper.CreateDocumentWriter();

            if (includeStandardNamespaceDeclarations)
            {
               serializer.Serialize(xmlWriter, value);

            }
            else
            {
               // Suppress adding the default XML instance and schema namespaces
               // by specifying the namespace table explicitly.
               var ns = new XmlSerializerNamespaces();

               // Append the default namespace to the namespace table
               if (defaultNamespace != null)
               {
                  ns.Add("", defaultNamespace);
               }

               serializer.Serialize(xmlWriter, value, ns);
            }

            return documentWriterHelper.ReadDocument().DocumentElement;
         }
      }

      /// <summary>
      ///    Creates a new XmlAttribute with a non-empty namespace prefix on
      ///    a <paramref name="element" />.
      /// </summary>
      /// <param name="element">The element where to add the attribute.</param>
      /// <param name="preferedPrefix">
      ///    A prefered prefix in case the namespace has no
      ///    declaration in scope.
      /// </param>
      /// <param name="attributeName">The name of the attribute.</param>
      /// <param name="namespaceUri">The namespace of the attribute.</param>
      /// <param name="value">The value of the attribute.</param>
      public static XmlAttribute CreateXmlAttribute(
         XmlElement element,
         string preferedPrefix,
         string attributeName,
         string namespaceUri,
         string value)
      {

         XmlDocument ownerDocument = element.OwnerDocument;
         Debug.Assert(ownerDocument != null);

         string prefix = element.GetPrefixOfNamespace(namespaceUri);

         if (String.IsNullOrEmpty(prefix))
         {
            // Namespace hasn't been declared so adding a declaration
            XmlAttribute nsDeclaration = ownerDocument.CreateAttribute(
               "xmlns", preferedPrefix, "http://www.w3.org/2000/xmlns/");

            nsDeclaration.Value = namespaceUri;

            element.Attributes.Append(nsDeclaration);

            prefix = preferedPrefix;
         }

         XmlAttribute attribute =
            ownerDocument.CreateAttribute(prefix, attributeName, namespaceUri);
         attribute.Value = value;
         return element.Attributes.Append(attribute);
      }

      /// <summary>
      ///    Creates a <see cref="XmlAttribute" />.
      /// </summary>
      /// <param name="name">The name of the attribute.</param>
      /// <param name="ns">The namespace of the attribute</param>
      /// <param name="value">The value of the attribute.</param>
      public static XmlAttribute CreateXmlAttribute(string name, string ns, string value)
      {
         XmlDocument tempDocument = new XmlDocument();

         XmlAttribute attribute = tempDocument.CreateAttribute(name, ns);
         attribute.Value = value;

         return attribute;
      }

      /// <summary>
      ///    Create a WS-Security signature.
      /// </summary>
      /// <param name="xmlRequestToSign">The XML document that contains the elements to sign.</param>
      /// <param name="keyIdentiifer">A XML element, that should identify the key.</param>
      /// <param name="signingKey">The key, used to sign the specified elements.</param>
      /// <param name="elementsToSign">
      ///    The IDs of the elements to sign. Can include
      ///    WS-Security IDs.
      /// </param>
      /// <returns>A XML element that contains the signature.</returns>
      public static XmlElement ComputeSignature(
         XmlDocument xmlRequestToSign,
         XmlElement keyIdentiifer,
         AsymmetricAlgorithm signingKey,
         params string[] elementsToSign)
      {

         WsuSignedXml signedXml = new WsuSignedXml(xmlRequestToSign);
         signedXml.SigningKey = signingKey;
         signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

         // Add a reference for each XML element that needs to be signed.
         foreach (var elementId in elementsToSign)
         {
            var reference = new Reference(String.Format("#{0}", elementId));
            reference.AddTransform(new XmlDsigExcC14NTransform(false));
            signedXml.AddReference(reference);
         }

         signedXml.KeyInfo.AddClause(new KeyInfoNode(keyIdentiifer));

         signedXml.ComputeSignature();

         return signedXml.GetXml();
      }

      /// <summary>
      ///    Reads a <see cref="XmlDocument" /> out of something writable
      ///    to a <see cref="XmlWriter" />.
      /// </summary>
      public static XmlDocument ReadDocument(Action<XmlWriter> writeAction)
      {
         using (Stream bufferStream = ToStream(writeAction))
         {
            using (XmlDictionaryReader xmlReader = GetXmlReader(bufferStream, true))
            {
               return ReadDocument(xmlReader);
            }
         }
      }

      /// <summary>
      ///    Gets a <see cref="XmlDictionaryReader" /> for the specified
      ///    <paramref name="document" />.
      /// </summary>
      public static XmlDictionaryReader GetXmlReader(XmlDocument document)
      {
         Stream bufferStream = ToStream(document.WriteTo);

         return GetXmlReader(bufferStream, true);
      }


      /// <summary>
      /// Reads the content of a <paramref name="stream"/> to a <see cref="XmlDocument"/>.
      /// </summary>
      /// <param name="stream">A stream that holds the XML to be read.</param>
      /// <param name="ownStream">Specifies if the stream should be closed when the reader is closed.</param>
      /// <returns></returns>
      public static XmlDocument ReadDocument(Stream stream, bool ownStream)
      {
         using (var xmlReader = GetXmlReader(stream, ownStream))
         {
            return ReadDocument(xmlReader);
         }
      }

      /// <summary>
      ///    Reads out of something writable to a <see cref="XmlWriter" /> to stream buffer.
      /// </summary>
      /// <param name="writeAction">
      ///    An action that writes the sources of the data
      ///    to a <see cref="XmlWriter" />
      /// </param>
      /// <param name="encoding">The encoding of the text written to the stream buffer.</param>
      private static Stream ToStream(Action<XmlWriter> writeAction, Encoding encoding = null)
      {
         encoding = encoding ?? Encoding.UTF8;

         MemoryStream buffer = new MemoryStream();

         using (
            XmlWriter xmlWriter = XmlDictionaryWriter.CreateTextWriter(
               buffer,
               encoding,
               false))
         {
            writeAction(xmlWriter);
         }

         buffer.Seek(0, SeekOrigin.Begin);

         return buffer;
      }

      /// <summary>
      /// Creates a <see cref="XmlDictionaryReader"/> for the specified <paramref name="stream"/>
      /// </summary>
      /// <param name="stream">A stream that holds the XML to be read.</param>
      /// <param name="ownStream">Specifies if the stream should be closed when the reader is closed.</param>
      /// <param name="encoding">The encoding of the text stored in <paramref name="stream" /></param>
      private static XmlDictionaryReader GetXmlReader(Stream stream, bool ownStream, Encoding encoding = null)
      {
         encoding = encoding ?? Encoding.UTF8;

         OnXmlDictionaryReaderClose onClose = ownStream ?
            (r => stream.Close()) : (OnXmlDictionaryReaderClose)null;

         return XmlDictionaryReader.CreateTextReader(
            stream,
            encoding,
            new XmlDictionaryReaderQuotas(),
            onClose);
      }

      /// <summary>
      ///    Creates a <see cref="XmlDocument" /> from the contents of <paramref name="xmlReader" />.
      /// </summary>
      private static XmlDocument ReadDocument(XmlDictionaryReader xmlReader)
      {
         // Preserving whitespace not to corrupt any signed elements
         XmlDocument result = new XmlDocument { PreserveWhitespace = true };
         result.Load(xmlReader);

         return result;
      }

      #endregion XML Utils

      #region SecureString Utils

      /// <summary>
      ///    Converts a secure string to a plain string.
      /// </summary>
      public static string ConvertSecureStringToString(SecureString secureString)
      {
         if (secureString == null)
         {
            return null;
         }

         IntPtr stringAsBstr = Marshal.SecureStringToBSTR(secureString);

         try
         {
            return Marshal.PtrToStringBSTR(stringAsBstr);
         }
         finally
         {
            Marshal.ZeroFreeBSTR(stringAsBstr);
         }
      }

      #endregion SecureString Utils
   }
}
