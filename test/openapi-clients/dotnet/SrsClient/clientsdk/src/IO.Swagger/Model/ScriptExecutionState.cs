/* 
 * Script Runtime Service for vSphere
 *
 * # Script Runtime Service API    Script Runtime Service for vSphere (SRS) allows running PowerShell and PowerCLI scripts. SRS is a VC add-on that is deployed separately from VCSA. SRS can be accessed via REST API that allows you to create PowerShell instances and run PowerShell and PowerCLI scripts within. No Connect-VIServer is required to run PowerCLI against VC(s) SRS is registered to.    ## Authetication    SRS uses VC SSO as Identity and Authentication Server. Two types of authentication are supported. SIGN and Basic. SIGN authentication is purposed for Service-To-Service access to SRS resources. For convenience of the end-users SRS supports basic authentication passing username and password which are used to acquire SAML HoK token for SRS solution. When basic is used SRS exchanges the username and password for SAML HoK token from the SSO server. SRS uses the SAML token to Connect PowerCLI to VC services in further operations.   On successful authentication SRS returns API Key which is required to authorize further SRS API calls.
 *
 * OpenAPI spec version: 1.0-oas3
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using SwaggerDateConverter = IO.Swagger.Client.SwaggerDateConverter;

namespace IO.Swagger.Model
{
    /// <summary>
    /// ScriptExecutionState defines possible script execution states.
    /// </summary>
    /// <value>ScriptExecutionState defines possible script execution states.</value>
    [JsonConverter(typeof(StringEnumConverter))]
        public enum ScriptExecutionState
    {
        /// <summary>
        /// Enum Success for value: success
        /// </summary>
        [EnumMember(Value = "success")]
        Success = 1,
        /// <summary>
        /// Enum Error for value: error
        /// </summary>
        [EnumMember(Value = "error")]
        Error = 2,
        /// <summary>
        /// Enum Running for value: running
        /// </summary>
        [EnumMember(Value = "running")]
        Running = 3,
        /// <summary>
        /// Enum Canceled for value: canceled
        /// </summary>
        [EnumMember(Value = "canceled")]
        Canceled = 4    }
}
