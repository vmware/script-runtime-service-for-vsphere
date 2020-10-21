/* 
 * Docker Engine API
 *
 * The Engine API is an HTTP API served by Docker Engine. It is the API the Docker client uses to communicate with the Engine, so everything the Docker client can do can be done with the API.  Most of the client's commands map directly to API endpoints (e.g. `docker ps` is `GET /containers/json`). The notable exception is running containers, which consists of several API calls.  # Errors  The API uses standard HTTP status codes to indicate the success or failure of the API call. The body of the response will be JSON in the following format:  ``` {   \"message\": \"page not found\" } ```  # Versioning  The API is usually changed in each release, so API calls are versioned to ensure that clients don't break. To lock to a specific version of the API, you prefix the URL with its version, for example, call `/v1.30/info` to use the v1.30 version of the `/info` endpoint. If the API version specified in the URL is not supported by the daemon, a HTTP `400 Bad Request` error message is returned.  If you omit the version-prefix, the current version of the API (v1.39) is used. For example, calling `/info` is the same as calling `/v1.39/info`. Using the API without a version-prefix is deprecated and will be removed in a future release.  Engine releases in the near future should support this version of the API, so your client will continue to work even if it is talking to a newer Engine.  The API uses an open schema model, which means server may add extra properties to responses. Likewise, the server will ignore any extra query parameters and request body properties. When you write clients, you need to ignore additional properties in responses to ensure they do not break when talking to newer daemons.   # Authentication  Authentication for registries is handled client side. The client has to send authentication details to various endpoints that need to communicate with registries, such as `POST /images/(name)/push`. These are sent as `X-Registry-Auth` header as a Base64 encoded (JSON) string with the following structure:  ``` {   \"username\": \"string\",   \"password\": \"string\",   \"email\": \"string\",   \"serveraddress\": \"string\" } ```  The `serveraddress` is a domain/IP without a protocol. Throughout this structure, double quotes are required.  If you have already got an identity token from the [`/auth` endpoint](#operation/SystemAuth), you can just pass this instead of credentials:  ``` {   \"identitytoken\": \"9cbaf023786cd7...\" } ``` 
 *
 * OpenAPI spec version: 1.39
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
using SwaggerDateConverter = VMware.ScriptRuntimeService.Docker.Bindings.Client.SwaggerDateConverter;

namespace VMware.ScriptRuntimeService.Docker.Bindings.Model
{
    /// <summary>
    /// TaskSpecContainerSpecConfigs
    /// </summary>
    [DataContract]
    public partial class TaskSpecContainerSpecConfigs :  IEquatable<TaskSpecContainerSpecConfigs>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSpecContainerSpecConfigs" /> class.
        /// </summary>
        /// <param name="file">file.</param>
        /// <param name="configID">ConfigID represents the ID of the specific config that we&#39;re referencing..</param>
        /// <param name="configName">ConfigName is the name of the config that this references, but this is just provided for lookup/display purposes. The config in the reference will be identified by its ID. .</param>
        public TaskSpecContainerSpecConfigs(TaskSpecContainerSpecFile file = default(TaskSpecContainerSpecFile), string configID = default(string), string configName = default(string))
        {
            this.File = file;
            this.ConfigID = configID;
            this.ConfigName = configName;
        }
        
        /// <summary>
        /// Gets or Sets File
        /// </summary>
        [DataMember(Name="File", EmitDefaultValue=false)]
        public TaskSpecContainerSpecFile File { get; set; }

        /// <summary>
        /// ConfigID represents the ID of the specific config that we&#39;re referencing.
        /// </summary>
        /// <value>ConfigID represents the ID of the specific config that we&#39;re referencing.</value>
        [DataMember(Name="ConfigID", EmitDefaultValue=false)]
        public string ConfigID { get; set; }

        /// <summary>
        /// ConfigName is the name of the config that this references, but this is just provided for lookup/display purposes. The config in the reference will be identified by its ID. 
        /// </summary>
        /// <value>ConfigName is the name of the config that this references, but this is just provided for lookup/display purposes. The config in the reference will be identified by its ID. </value>
        [DataMember(Name="ConfigName", EmitDefaultValue=false)]
        public string ConfigName { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TaskSpecContainerSpecConfigs {\n");
            sb.Append("  File: ").Append(File).Append("\n");
            sb.Append("  ConfigID: ").Append(ConfigID).Append("\n");
            sb.Append("  ConfigName: ").Append(ConfigName).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as TaskSpecContainerSpecConfigs);
        }

        /// <summary>
        /// Returns true if TaskSpecContainerSpecConfigs instances are equal
        /// </summary>
        /// <param name="input">Instance of TaskSpecContainerSpecConfigs to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TaskSpecContainerSpecConfigs input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.File == input.File ||
                    (this.File != null &&
                    this.File.Equals(input.File))
                ) && 
                (
                    this.ConfigID == input.ConfigID ||
                    (this.ConfigID != null &&
                    this.ConfigID.Equals(input.ConfigID))
                ) && 
                (
                    this.ConfigName == input.ConfigName ||
                    (this.ConfigName != null &&
                    this.ConfigName.Equals(input.ConfigName))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.File != null)
                    hashCode = hashCode * 59 + this.File.GetHashCode();
                if (this.ConfigID != null)
                    hashCode = hashCode * 59 + this.ConfigID.GetHashCode();
                if (this.ConfigName != null)
                    hashCode = hashCode * 59 + this.ConfigName.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
