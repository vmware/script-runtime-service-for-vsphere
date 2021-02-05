// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using VMware.ScriptRuntimeService.APIGateway.Runspace;
using VMware.ScriptRuntimeService.Runspace.Types;

namespace VMware.ScriptRuntimeService.APIGateway.DataTypes
{
   /// <summary>
   /// WebConsole resource allowscreation and removal of accessible through browser PowerShell consoles.
   /// The API allows you to create, read, and delete web consoles.
   /// </summary>
   [DataContract(Name = "webconsole")]
   public class WebConsole {
      public WebConsole() { }
      public WebConsole(IWebConsoleData webConsoleData) {
         if (webConsoleData == null) {
            throw new ArgumentNullException(nameof(webConsoleData));
         }
         Id = webConsoleData.Id;
         CreationTime = webConsoleData.CreationTime;
         State = webConsoleData.State;         
         ErrorDetails = webConsoleData.ErrorDetails;
      }

      /// <summary>
      /// Unique identifier for the object.
      /// </summary>
      [DataMember(Name = "id")]
      [ReadOnly(true)]
      public string Id { get; private set; }

      /// <summary>
      /// State of the web console resource.
      /// </summary>
      [DataMember(Name = "state", IsRequired = false)]
      public WebConsoleState State { get; }

      /// <summary>
      /// Details about the error that has occured when the web console state is 'error'.
      /// </summary>
      [DataMember(Name = "error_details", IsRequired = false)]
      [ReadOnly(true)]
      public ErrorDetails ErrorDetails { get; }

      /// <summary>
      /// The time at which the object was created. String representing time in format ISO 8601.
      /// </summary>
      [DataMember(Name = "creation_time", IsRequired = false)]
      public DateTime CreationTime { get; }
   }
}
