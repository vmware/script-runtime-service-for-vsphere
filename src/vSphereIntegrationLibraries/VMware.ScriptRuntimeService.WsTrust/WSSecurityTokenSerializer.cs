// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Xml;
using VMware.IdentityModel.Tokens;
using VMware.ServiceModel.Security;

namespace VMware.ScriptRuntimeService.WsTrust
{
   /// <summary>
   /// The code in this class is extracted from WCF (System.ServiceModel.Security.WSSecurityTokenSerializer) as 
   /// the class is not ported at the time. Only the needed functionality has been extracted.
   /// The interface is the same to make switching back to wcf version easier.
   /// </summary>
   public class WSSecurityTokenSerializer
   {
      private const string XD_SecurityJan2004Dictionary_Prefix_Value = "o";
      private const string XD_SecurityJan2004Dictionary_UserNameTokenElement = "UsernameToken";

      private const string XD_SecurityJan2004Dictionary_Namespace =
         "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

      private const string XD_SecurityJan2004Dictionary_UserNameElement = "Username";
      private const string XD_SecurityJan2004Dictionary_PasswordElement = "Password";
      private const string XD_SecurityJan2004Dictionary_TypeAttribute = "Type";

      private const string SecurityJan2004Strings_UPTokenPasswordTextValue =
         "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";

      private const string XD_UtilityDictionary_Prefix_Value = "u";
      private const string XD_UtilityDictionary_IdAttribute = "Id";

      private const string XD_UtilityDictionary_Namespace =
         "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

      private const string XD_SecurityJan2004Dictionary_BinarySecurityToken = "BinarySecurityToken";
      private const string XD_SecurityJan2004Dictionary_EncodingType = "EncodingType";

      private const string SecurityJan2004Strings_EncodingTypeValueBase64Binary =
         "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";

      private const string SecurityJan2004Strings_X509TokenType =
         "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";

      private const string XD_SecurityJan2004Dictionary_ValueType = "ValueType";

      private const string XD_SecurityJan2004Dictionary_SecurityTokenReference = "SecurityTokenReference";
      private const string XD_SecurityJan2004Dictionary_Reference = "Reference";
      private const string XD_SecurityJan2004Dictionary_URI = "URI";

      private const string X509TokenType =
         "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";

      /// <summary>
      /// Dummy constructor used to resemble System.ServiceModel.Security.WSSecurityTokenSerializer.
      /// In order to make switching back to wcf version easier.
      /// </summary>
      /// <param name="securityVersion"></param>
      /// <param name="emitBspRequiredAttributes"></param>
      public WSSecurityTokenSerializer(SecurityVersion securityVersion, bool emitBspRequiredAttributes)
      {
         if (securityVersion != SecurityVersion.WSSecurity10)
         {
            throw new NotSupportedException("No other security version other than WSSecurity10 is supported.");
         }

         if (!emitBspRequiredAttributes)
         {
            throw new NotSupportedException("Always emits bspRequiredAttributes.");
         }
      }

      private static void SerializeUsernamePasswordToken(XmlDictionaryWriter dictionaryWriter,
         UserNameSecurityToken token)
      {
         dictionaryWriter.WriteStartElement(XD_SecurityJan2004Dictionary_Prefix_Value,
            XD_SecurityJan2004Dictionary_UserNameTokenElement,
            XD_SecurityJan2004Dictionary_Namespace); // <wsse:UsernameToken
         dictionaryWriter.WriteAttributeString(XD_UtilityDictionary_Prefix_Value, XD_UtilityDictionary_IdAttribute,
            XD_UtilityDictionary_Namespace, token.Id); // wsu:Id="..."
         dictionaryWriter.WriteElementString(XD_SecurityJan2004Dictionary_Prefix_Value,
            XD_SecurityJan2004Dictionary_UserNameElement,
            XD_SecurityJan2004Dictionary_Namespace, token.UserName); // ><wsse:Username>...</wsse:Username>
         if (token.Password != null)
         {
            dictionaryWriter.WriteStartElement(XD_SecurityJan2004Dictionary_Prefix_Value,
               XD_SecurityJan2004Dictionary_PasswordElement,
               XD_SecurityJan2004Dictionary_Namespace);
            // tokenSerializer EmitBspRequiredAttributes is set to true
            dictionaryWriter.WriteAttributeString(XD_SecurityJan2004Dictionary_TypeAttribute, null,
               SecurityJan2004Strings_UPTokenPasswordTextValue);
            // emitBspRequiredAtribute
            dictionaryWriter.WriteString(token.Password); // <wsse:Password>...</wsse:Password>
            dictionaryWriter.WriteEndElement();
         }
         dictionaryWriter.WriteEndElement(); // </wsse:UsernameToken>
      }

      private static void SerializeX509SecurityToken(XmlDictionaryWriter dictionaryWriter, X509SecurityToken token)
      {
         string id = token.Id;
         byte[] rawData = token.Certificate.GetRawCertData();

         if (rawData == null)
         {
            throw new ArgumentNullException("rawData");
         }

         dictionaryWriter.WriteStartElement(XD_SecurityJan2004Dictionary_Prefix_Value,
            XD_SecurityJan2004Dictionary_BinarySecurityToken,
            XD_SecurityJan2004Dictionary_Namespace);
         if (id != null)
         {
            dictionaryWriter.WriteAttributeString(XD_UtilityDictionary_Prefix_Value, XD_UtilityDictionary_IdAttribute,
               XD_UtilityDictionary_Namespace, id);
         }
         dictionaryWriter.WriteAttributeString(XD_SecurityJan2004Dictionary_ValueType, null,
            SecurityJan2004Strings_X509TokenType);
         // tokenSerializer EmitBspRequiredAttributes is set to true
         dictionaryWriter.WriteAttributeString(XD_SecurityJan2004Dictionary_EncodingType, null,
            SecurityJan2004Strings_EncodingTypeValueBase64Binary);
         //EmitBspRequiredAttributes
         dictionaryWriter.WriteBase64(rawData, 0, rawData.Length);
         dictionaryWriter.WriteEndElement(); // BinarySecurityToken
      }

      internal static void SerializeLocalIdKeyIdentifierClause(XmlDictionaryWriter dictionaryWriter,
         LocalIdKeyIdentifierClause localIdClause)
      {
         dictionaryWriter.WriteStartElement(XD_SecurityJan2004Dictionary_Prefix_Value,
            XD_SecurityJan2004Dictionary_SecurityTokenReference,
            XD_SecurityJan2004Dictionary_Namespace);
         dictionaryWriter.WriteStartElement(XD_SecurityJan2004Dictionary_Prefix_Value,
            XD_SecurityJan2004Dictionary_Reference,
            XD_SecurityJan2004Dictionary_Namespace);
         // tokenSerializer EmitBspRequiredAttributes is set to true
         dictionaryWriter.WriteAttributeString(XD_SecurityJan2004Dictionary_ValueType, null, X509TokenType);
         //end EmitBspRequiredAttributes
         dictionaryWriter.WriteAttributeString(XD_SecurityJan2004Dictionary_URI, null, "#" + localIdClause.LocalId);
         dictionaryWriter.WriteEndElement();
         dictionaryWriter.WriteEndElement();
      }

      public void WriteToken(XmlWriter writer, SecurityToken token)
      {
         XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);

         if (token is X509SecurityToken)
         {
            SerializeX509SecurityToken(dictionaryWriter, token as X509SecurityToken);
         }
         else if (token is UserNameSecurityToken)
         {
            SerializeUsernamePasswordToken(dictionaryWriter, token as UserNameSecurityToken);
         }
         else
         {
            throw new NotSupportedException("token's type not supported.");
         }

         dictionaryWriter.Flush();
      }

      public void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
      {
         XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);

         if (keyIdentifierClause is LocalIdKeyIdentifierClause)
         {
            SerializeLocalIdKeyIdentifierClause(dictionaryWriter, keyIdentifierClause as LocalIdKeyIdentifierClause);
         }
         else
         {
            throw new NotSupportedException("keyIdentifierClause's type not supported.");
         }

         dictionaryWriter.Flush();
      }
   }
}
