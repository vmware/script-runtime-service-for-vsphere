// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************
using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace VMware.ScriptRuntimeService.WsTrust
{
   /// <summary>
   ///    Endpoint behavior that implements the WS-Security protocol for
   ///    VMware SSO authentication.
   /// </summary>
   public class WsTrustBehavior : IEndpointBehavior
   {
      public void Validate(ServiceEndpoint endpoint) { }

      public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

      public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

      public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
      {
         // The protocol is implemented as a message inspector (in this case interceptor).
         clientRuntime.ClientMessageInspectors.Add(new WsTrustClientMessageInspector());
      }
   }
}
