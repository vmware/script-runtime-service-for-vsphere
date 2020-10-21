// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using VMware.ScriptRuntimeService.Setup.ConfigFileWriters;

namespace VMware.ScriptRuntimeService.Setup.SelfSignedCertificates
{
   public interface ISelfSignedCertificateGeneratorFactory
   {
      /// <summary>
      /// Creates <see cref="ISelfSignedCertificateGenerator"/> instance
      /// </summary>
      /// <param name="commonName">Certificate common name</param>
      /// <param name="certificateFileWriter"><see cref="IConfigFileWriter"/> instance to output the generated certificate files</param>
      /// <returns></returns>
      ISelfSignedCertificateGenerator Create(string commonName, IConfigWriter certificateFileWriter);
   }
}
