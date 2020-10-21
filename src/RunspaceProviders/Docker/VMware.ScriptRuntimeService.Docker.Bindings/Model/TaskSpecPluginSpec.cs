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
    /// Plugin spec for the service.  *(Experimental release only.)*  &lt;p&gt;&lt;br /&gt;&lt;/p&gt;  &gt; **Note**: ContainerSpec, NetworkAttachmentSpec, and PluginSpec are &gt; mutually exclusive. PluginSpec is only used when the Runtime field &gt; is set to &#x60;plugin&#x60;. NetworkAttachmentSpec is used when the Runtime &gt; field is set to &#x60;attachment&#x60;. 
    /// </summary>
    [DataContract]
    public partial class TaskSpecPluginSpec :  IEquatable<TaskSpecPluginSpec>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskSpecPluginSpec" /> class.
        /// </summary>
        /// <param name="name">The name or &#39;alias&#39; to use for the plugin..</param>
        /// <param name="remote">The plugin image reference to use..</param>
        /// <param name="disabled">Disable the plugin once scheduled..</param>
        /// <param name="pluginPrivilege">pluginPrivilege.</param>
        public TaskSpecPluginSpec(string name = default(string), string remote = default(string), bool? disabled = default(bool?), List<Body> pluginPrivilege = default(List<Body>))
        {
            this.Name = name;
            this.Remote = remote;
            this.Disabled = disabled;
            this.PluginPrivilege = pluginPrivilege;
        }
        
        /// <summary>
        /// The name or &#39;alias&#39; to use for the plugin.
        /// </summary>
        /// <value>The name or &#39;alias&#39; to use for the plugin.</value>
        [DataMember(Name="Name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// The plugin image reference to use.
        /// </summary>
        /// <value>The plugin image reference to use.</value>
        [DataMember(Name="Remote", EmitDefaultValue=false)]
        public string Remote { get; set; }

        /// <summary>
        /// Disable the plugin once scheduled.
        /// </summary>
        /// <value>Disable the plugin once scheduled.</value>
        [DataMember(Name="Disabled", EmitDefaultValue=false)]
        public bool? Disabled { get; set; }

        /// <summary>
        /// Gets or Sets PluginPrivilege
        /// </summary>
        [DataMember(Name="PluginPrivilege", EmitDefaultValue=false)]
        public List<Body> PluginPrivilege { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TaskSpecPluginSpec {\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Remote: ").Append(Remote).Append("\n");
            sb.Append("  Disabled: ").Append(Disabled).Append("\n");
            sb.Append("  PluginPrivilege: ").Append(PluginPrivilege).Append("\n");
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
            return this.Equals(input as TaskSpecPluginSpec);
        }

        /// <summary>
        /// Returns true if TaskSpecPluginSpec instances are equal
        /// </summary>
        /// <param name="input">Instance of TaskSpecPluginSpec to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TaskSpecPluginSpec input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.Remote == input.Remote ||
                    (this.Remote != null &&
                    this.Remote.Equals(input.Remote))
                ) && 
                (
                    this.Disabled == input.Disabled ||
                    (this.Disabled != null &&
                    this.Disabled.Equals(input.Disabled))
                ) && 
                (
                    this.PluginPrivilege == input.PluginPrivilege ||
                    this.PluginPrivilege != null &&
                    this.PluginPrivilege.SequenceEqual(input.PluginPrivilege)
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
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Remote != null)
                    hashCode = hashCode * 59 + this.Remote.GetHashCode();
                if (this.Disabled != null)
                    hashCode = hashCode * 59 + this.Disabled.GetHashCode();
                if (this.PluginPrivilege != null)
                    hashCode = hashCode * 59 + this.PluginPrivilege.GetHashCode();
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
