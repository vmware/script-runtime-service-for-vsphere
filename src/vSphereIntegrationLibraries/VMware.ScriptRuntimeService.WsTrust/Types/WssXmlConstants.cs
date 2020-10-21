// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace VMware.ScriptRuntimeService.WsTrust.Types
{
   public static class WssXmlConstants
   {
      public const string WssSecurityExtension10Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

      public const string WssSecurityUtility10Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

      public const string WssSecurityExtension11Namespace = "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd";

      public const string WssSamlTokenProfile11SamlVersion20 = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";

      public const string WssSamlTokenProfile11SamlId = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID";

      public const string Timestamp = "Timestamp";

      public const string Security = "Security";

      public const string UsernameToken = "UsernameToken";

      public const string BinarySecurityToken = "BinarySecurityToken";

      public const string Assertion = "Assertion";

      public const string Saml20AssertionNamespace = "urn:oasis:names:tc:SAML:2.0:assertion";

      public const string SecurityTokenReference = "SecurityTokenReference";

      public const string KeyIdentifier = "KeyIdentifier";

      public const string ValueType = "ValueType";
   }
}
