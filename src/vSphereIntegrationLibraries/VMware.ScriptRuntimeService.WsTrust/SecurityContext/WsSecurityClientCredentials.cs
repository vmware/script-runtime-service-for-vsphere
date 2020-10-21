// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace VMware.ScriptRuntimeService.WsTrust.SecurityContext {
   public class WsSecurityClientCredentials {
      // The user name presented to the remote service.
      // This field is used in the UsernameToken section of the WS-Security header.
      public string User { get; set; }

      // The password presented to the remote service.
      // This field is used in the UsernameToken section of the WS-Security header.
      public SecureString Password { get; set; }

      // The client certificate presented to the remote service.
      // This field is used in the BinarySecurityToken section of the WS-Security header.      
      public X509Certificate2 ClientCertificate { get; set; }

      // SAML 2.0 bearer token presented to the remote service.
      // It is included as a <SAML2:Assertion> element in the WS-Security header of an operation.
      public XmlElement BearerToken { get; set; }

      // SAML 2.0 holder-of-key (HoK) token presented to the remote service.
      // It is included as <SAML2:Assertion> element in the WS-Security header of an operation.
      public XmlElement HolderOfKeyToken { get; set; }
   }
}
