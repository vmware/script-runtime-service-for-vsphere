// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using VMware.Http.Sso.Authentication;

namespace VMware.ScriptRuntimeService.APIGateway.Authentication.Sign {
   public class SignAuthenticationRequest : IRequest {
      public SignAuthenticationRequest(HttpRequest httpRequest, string forwardedHost, string forwardedScheme) {

         var body = "";
         var req = httpRequest;

         // Allows using several time the stream
         req.EnableBuffering();        

         using (StreamReader reader
            = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true)) {
            body = reader.ReadToEnd();
         }

         // Rewind, so the core is not lost when it looks the body for the request
         req.Body.Position = 0;

         var forwardedPort = 0;
         if (!string.IsNullOrEmpty(forwardedScheme)) {
            // default port for forwarded scheme
            if (string.Compare(forwardedScheme, "https", StringComparison.OrdinalIgnoreCase) == 0) {
               forwardedPort = 443;
            }

            if (string.Compare(forwardedScheme, "http", StringComparison.OrdinalIgnoreCase) == 0) {
               forwardedPort = 80;
            }
         }
         
         Method = Enum.Parse<HttpMethod>(httpRequest.Method);
         RequestUri = httpRequest.Path;
         HostName = string.IsNullOrEmpty(forwardedHost) ? httpRequest.Host.Host : forwardedHost;
         Port = httpRequest.Host.Port ?? forwardedPort;
         Body = body;
      }

      public HttpMethod Method { get; }
      public string RequestUri { get; }
      public string HostName { get; }
      public int Port { get; }
      public string Body { get; }
   }
}
