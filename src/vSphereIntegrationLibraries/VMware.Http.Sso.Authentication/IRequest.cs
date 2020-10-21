// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMware.Http.Sso.Authentication {
   public interface IRequest {
      HttpMethod Method { get; }

      string RequestUri { get; }

      string HostName { get; }

      int Port { get; }

      string Body { get; }
   }
}
