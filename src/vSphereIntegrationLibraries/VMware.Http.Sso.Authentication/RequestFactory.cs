// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace VMware.Http.Sso.Authentication {
   public static class RequestFactory {
      private class SignAuthenticationRequest : IRequest {
         public SignAuthenticationRequest(HttpMethod method, string uri, string hostName, int port, string body) {
            Method = method;
            RequestUri = uri;
            HostName = hostName;
            Port = port;
            Body = body;
         }
         public HttpMethod Method { get; }
         public string RequestUri { get; }
         public string HostName { get; }
         public int Port { get; }
         public string Body { get; }
      }
      public static IRequest Create(HttpMethod method, string uri, string hostName, int port, string body) {
         return new SignAuthenticationRequest(method, uri, hostName, port, body);
      }
   }
}
