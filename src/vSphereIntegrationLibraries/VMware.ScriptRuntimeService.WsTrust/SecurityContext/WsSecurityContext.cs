// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace VMware.ScriptRuntimeService.WsTrust.SecurityContext {
   /// <summary>
   /// Contains WS Security context information such as client credentials, etc. as well
   /// as proxy methods for invoking a WCF operation within this security context.
   /// </summary>
   public class WsSecurityContext {
      internal const string WSS_CONTEXT_KEY = "VMware WS-Security Context";

      // WCF client channel instance
      public IClientChannel ClientChannel;

      // Properties that will be used to create a temporary WS-Security context for the current client channel and operation
      public WsSecurityContextProperties Properties { get; set; }

      public WsSecurityContext()
      {
         // Initialize property placeholder object
         Properties = new WsSecurityContextProperties();
      }

      #region Operation Invocation

      /// <summary>
      /// Creates a temporary operation context scope on the current <see cref="IClientChannel"/>
      /// and invokes the specified <paramref name="operation"/> within this context.
      /// </summary>
      /// <typeparam name="T">
      /// The return type of the specified <paramref name="operation"/></typeparam>
      /// <param name="operation">
      /// Reference to a target method that will be invoked.
      /// All <see cref="IClientChannel"/> operations that are invoked internally from this target method
      /// will receive the current security context as a property of the generated WCF <see cref="Message"/>.
      /// Use the <see cref="GetProperties(Message)"/> static method of this class to extract the security 
      /// context from the generated WCF message.
      /// </param>
      /// <returns>
      /// The result from the execution of the specified <paramref name="operation"/>.</returns>
      public T InvokeOperation<T>(
         Func<T> operation)
      {

         if (ClientChannel == null)
         {
            throw new InvalidOperationException("Client channel is not set.");
         }

         using (new OperationContextScope(ClientChannel))
         {
            OperationContext.Current.OutgoingMessageProperties.Add(WSS_CONTEXT_KEY, Properties);

            // Invoke target operation
            return operation();
         }
      }

      /// <summary>
      /// Creates a temporary operation context scope on the current <see cref="IClientChannel"/>
      /// and invokes the specified <paramref name="operation"/> within this context.
      /// </summary>
      /// <param name="operation">
      /// Reference to a target method that will be invoked.
      /// All <see cref="IClientChannel"/> operations that are invoked internally from this target method
      /// will receive the current security context as a property of the generated WCF <see cref="Message"/>.
      /// Use the <see cref="GetProperties(Message)"/> static method of this class to extract the security 
      /// context from the generated WCF message.
      /// </param>
      public void InvokeOperation(
         Action operation)
      {

         if (ClientChannel == null)
         {
            throw new InvalidOperationException("Client channel is not set.");
         }

         using (new OperationContextScope(ClientChannel))
         {
            OperationContext.Current.OutgoingMessageProperties.Add(WSS_CONTEXT_KEY, Properties);

            // Invoke target operation
            operation();
         }
      }

      #endregion Operation Invocation

      #region Context Retrieval

      /// <summary>
      /// Retrieves the WS-Security related properties from the specified <paramref name="message"/>
      /// </summary>
      /// <param name="message">
      /// WCF message created from a client channel operation that is executed within the current context.
      /// </param>
      /// <returns>
      /// <see cref="WsSecurityContextProperties"/> instance or null if the specified 
      /// <paramref name="message"/> does not have a security context.
      /// </returns>
      public static WsSecurityContextProperties GetProperties(Message message)
      {
         WsSecurityContextProperties result = null;

         if (message.Properties.ContainsKey(WSS_CONTEXT_KEY))
         {
            // If the value type is not compatible, return null
            result = message.Properties[WSS_CONTEXT_KEY] as WsSecurityContextProperties;
         }

         return result;
      }

      #endregion Context Retrieval
   }

   /// <summary>
   /// Data structure with <see cref="WsSecurityContext"> related properties.
   /// </summary>
   public class WsSecurityContextProperties
   {

      // Contains client credentials that will be presented as part of the WS-Security header.
      public WsSecurityClientCredentials Credentials { get; set; }

      // The private key used to sign particular section elements of the WS-Security header.
      // Note: This field is required for Holder-Of-Key related operations (such as acquire and present token).
      public AsymmetricAlgorithm SigningKey { get; set; }

      public WsSecurityContextProperties()
      {
         Credentials = new WsSecurityClientCredentials();
      }
   }
}
