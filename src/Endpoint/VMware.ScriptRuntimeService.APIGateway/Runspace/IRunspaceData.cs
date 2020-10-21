// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMware.ScriptRuntimeService.APIGateway.DataTypes;
using VMware.ScriptRuntimeService.RunspaceProviders.Types;

namespace VMware.ScriptRuntimeService.APIGateway.Runspace {
   public interface IRunspaceData : IRunspaceInfo {
      /// <summary>
      /// Name of the runspace. It is optional to give a name of the runspace on create request. If name was not specified on runspace creation the field has null value.
      /// </summary>      
      public string Name { get; set; }
      

      /// <summary>
      /// State of the runspace resource.
      /// </summary>
      public RunspaceState State { get; set; }

      /// <summary>
      /// Details about the error that has occured when the runspaces state is 'error'.
      /// </summary>
      public ErrorDetails ErrorDetails { get; set; }

      /// <summary>
      /// Boolean that indicates whether PowerCLI connection has to be initialized when a Runspace creation is requested.
      /// 
      /// It is optional to request VCenter Servers connection to be initialized on create request.
      /// If true is specified PowerCLI Connect-VIServer cmdlet will be called in the create PowerShell instance.
      /// Connect-VIServer uses the authorized user's SSO SAML token to connect to VCenter servers.
      /// Connection to all linked VCetner servers will be established. When requested the runspace creation will be slower
      /// with the time needed for PowerCLI modules loading and Connect-VIServer execution time. _vc_connection_script_state
      /// field will hold state of the script completion. In case errors have occured during connect script execution
      /// _vc_connection_script_error_records will be populated.
      /// 
      /// Default value is false.
      /// </summary>
      public bool RunVcConnectionScript { get; set; }

      /// <summary>
      /// Id of vc connection script if it was requested on runspace create request.
      /// You can use this id to retrieve all the details of the script execution.
      /// The script could fail and the details about the script failure would be available in scriptexecutions API.
      /// </summary>
      public string VcConnectionScriptId { get; set; }

      /// <summary>
      /// Time at which the object was created. String representing time in format ISO 8601.
      /// </summary>
      public DateTime CreationTime { get; set; }
   }
}
