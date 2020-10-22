// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace VMware.ScriptRuntimeService.Sts.SamlToken {
   public interface ISamlToken {
      DateTime StartTime { get; }

      DateTime ExpirationTime { get; }
      
      ConfirmationType ConfirmationType { get; }

      string RawXml { get; }

      XmlElement RawXmlElement { get; }

      string SubjectNameId { get; }

      string Id { get; }

      X509Certificate2 ConfirmationCertificate { get; }
   }
}
