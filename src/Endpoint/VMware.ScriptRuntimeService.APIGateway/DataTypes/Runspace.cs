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
   /// Runspace object allows you to create and delete isolated PowerShell instance for your script executions.
   /// The API allows you to create, read, and delete runspaces.
   /// </summary>
   [DataContract(Name = "runspace")]
   public class Runspace {
      public Runspace() { }
      public Runspace(IRunspaceData runspaceData) {
         if (runspaceData == null) {
            throw new ArgumentNullException(nameof(runspaceData));
         }
         Id = runspaceData.Id;
         CreationTime = runspaceData.CreationTime;
         Name = runspaceData.Name;
         RunVcConnectionScript = runspaceData.RunVcConnectionScript;
         State = runspaceData.State;
         VcConnectionScriptId = runspaceData.VcConnectionScriptId;
         ErrorDetails = runspaceData.ErrorDetails;
      }

      /// <summary>
      /// Name of the runspace. It is optional to give a name of the runspace on create request. If name was not specified on runspace creation the field has null value.
      /// </summary>
      [DataMember(Name = "name", IsRequired = false)]
      public string Name { get; set; }

      /// <summary>
      /// Unique identifier for the object.
      /// </summary>
      [DataMember(Name = "id")]
      [ReadOnly(true)]
      public string Id { get; private set; }

      /// <summary>
      /// State of the runspace resource.
      /// </summary>
      [DataMember(Name = "state", IsRequired = false)]
      public RunspaceState State { get; }

      /// <summary>
      /// Details about the error that has occured when the runspaces state is 'error'.
      /// </summary>
      [DataMember(Name = "error_details", IsRequired = false)]
      [ReadOnly(true)]
      public ErrorDetails ErrorDetails { get; }

      /// <summary>
      /// Boolean that indicates whether PowerCLI connection has to be initialized when a Runspace creation is requested.
      /// 
      /// It is optional to request VCenter Servers connection to be initialized on create request.
      /// If true is specified PowerCLI Connect-VIServer cmdlet will be called in the create PowerShell instance.
      /// Connect-VIServer uses the authorized user's SSO SAML token to connect to the vCenter Servers.
      /// Connection to all linked vCetner Servers will be established. When requested the runspace creation will be slower
      /// with the time needed for PowerCLI modules loading and Connect-VIServer execution time. _vc_connection_script_state
      /// field will hold state of the script completion. In case errors have occured during connect script execution
      /// _vc_connection_script_error_records will be populated.
      /// 
      /// Default value is false.
      /// </summary>
      [DataMember(Name = "run_vc_connection_script", IsRequired = false)]
      public bool RunVcConnectionScript { get; set; }

      /// <summary>
      /// Id of the vc connection script if it was requested on the runspace create request.
      /// You can use this id to retrieve all the details of the script execution.
      /// The script could fail and the details about the script failure would be available in the scriptexecutions API.
      /// </summary>
      [DataMember(Name = "vc_connection_script_id", IsRequired = false)]
      public string VcConnectionScriptId { get; }

      /// <summary>
      /// The time at which the object was created. String representing time in format ISO 8601.
      /// </summary>
      [DataMember(Name = "creation_time", IsRequired = false)]
      public DateTime CreationTime { get; }
   }
}
