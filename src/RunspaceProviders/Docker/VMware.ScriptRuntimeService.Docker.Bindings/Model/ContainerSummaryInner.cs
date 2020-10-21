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
    /// ContainerSummaryInner
    /// </summary>
    [DataContract]
    public partial class ContainerSummaryInner :  IEquatable<ContainerSummaryInner>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerSummaryInner" /> class.
        /// </summary>
        /// <param name="id">The ID of this container.</param>
        /// <param name="names">The names that this container has been given.</param>
        /// <param name="image">The name of the image used when creating this container.</param>
        /// <param name="imageID">The ID of the image that this container was created from.</param>
        /// <param name="command">Command to run when starting the container.</param>
        /// <param name="created">When the container was created.</param>
        /// <param name="ports">The ports exposed by this container.</param>
        /// <param name="sizeRw">The size of files that have been created or changed by this container.</param>
        /// <param name="sizeRootFs">The total size of all the files in this container.</param>
        /// <param name="labels">User-defined key/value metadata..</param>
        /// <param name="state">The state of this container (e.g. &#x60;Exited&#x60;).</param>
        /// <param name="status">Additional human-readable status of this container (e.g. &#x60;Exit 0&#x60;).</param>
        /// <param name="hostConfig">hostConfig.</param>
        /// <param name="networkSettings">networkSettings.</param>
        /// <param name="mounts">mounts.</param>
        public ContainerSummaryInner(string id = default(string), List<string> names = default(List<string>), string image = default(string), string imageID = default(string), string command = default(string), long? created = default(long?), List<Port> ports = default(List<Port>), long? sizeRw = default(long?), long? sizeRootFs = default(long?), Dictionary<string, string> labels = default(Dictionary<string, string>), string state = default(string), string status = default(string), ContainerSummaryInnerHostConfig hostConfig = default(ContainerSummaryInnerHostConfig), ContainerSummaryInnerNetworkSettings networkSettings = default(ContainerSummaryInnerNetworkSettings), List<Mount> mounts = default(List<Mount>))
        {
            this.Id = id;
            this.Names = names;
            this.Image = image;
            this.ImageID = imageID;
            this.Command = command;
            this.Created = created;
            this.Ports = ports;
            this.SizeRw = sizeRw;
            this.SizeRootFs = sizeRootFs;
            this.Labels = labels;
            this.State = state;
            this.Status = status;
            this.HostConfig = hostConfig;
            this.NetworkSettings = networkSettings;
            this.Mounts = mounts;
        }
        
        /// <summary>
        /// The ID of this container
        /// </summary>
        /// <value>The ID of this container</value>
        [DataMember(Name="Id", EmitDefaultValue=false)]
        public string Id { get; set; }

        /// <summary>
        /// The names that this container has been given
        /// </summary>
        /// <value>The names that this container has been given</value>
        [DataMember(Name="Names", EmitDefaultValue=false)]
        public List<string> Names { get; set; }

        /// <summary>
        /// The name of the image used when creating this container
        /// </summary>
        /// <value>The name of the image used when creating this container</value>
        [DataMember(Name="Image", EmitDefaultValue=false)]
        public string Image { get; set; }

        /// <summary>
        /// The ID of the image that this container was created from
        /// </summary>
        /// <value>The ID of the image that this container was created from</value>
        [DataMember(Name="ImageID", EmitDefaultValue=false)]
        public string ImageID { get; set; }

        /// <summary>
        /// Command to run when starting the container
        /// </summary>
        /// <value>Command to run when starting the container</value>
        [DataMember(Name="Command", EmitDefaultValue=false)]
        public string Command { get; set; }

        /// <summary>
        /// When the container was created
        /// </summary>
        /// <value>When the container was created</value>
        [DataMember(Name="Created", EmitDefaultValue=false)]
        public long? Created { get; set; }

        /// <summary>
        /// The ports exposed by this container
        /// </summary>
        /// <value>The ports exposed by this container</value>
        [DataMember(Name="Ports", EmitDefaultValue=false)]
        public List<Port> Ports { get; set; }

        /// <summary>
        /// The size of files that have been created or changed by this container
        /// </summary>
        /// <value>The size of files that have been created or changed by this container</value>
        [DataMember(Name="SizeRw", EmitDefaultValue=false)]
        public long? SizeRw { get; set; }

        /// <summary>
        /// The total size of all the files in this container
        /// </summary>
        /// <value>The total size of all the files in this container</value>
        [DataMember(Name="SizeRootFs", EmitDefaultValue=false)]
        public long? SizeRootFs { get; set; }

        /// <summary>
        /// User-defined key/value metadata.
        /// </summary>
        /// <value>User-defined key/value metadata.</value>
        [DataMember(Name="Labels", EmitDefaultValue=false)]
        public Dictionary<string, string> Labels { get; set; }

        /// <summary>
        /// The state of this container (e.g. &#x60;Exited&#x60;)
        /// </summary>
        /// <value>The state of this container (e.g. &#x60;Exited&#x60;)</value>
        [DataMember(Name="State", EmitDefaultValue=false)]
        public string State { get; set; }

        /// <summary>
        /// Additional human-readable status of this container (e.g. &#x60;Exit 0&#x60;)
        /// </summary>
        /// <value>Additional human-readable status of this container (e.g. &#x60;Exit 0&#x60;)</value>
        [DataMember(Name="Status", EmitDefaultValue=false)]
        public string Status { get; set; }

        /// <summary>
        /// Gets or Sets HostConfig
        /// </summary>
        [DataMember(Name="HostConfig", EmitDefaultValue=false)]
        public ContainerSummaryInnerHostConfig HostConfig { get; set; }

        /// <summary>
        /// Gets or Sets NetworkSettings
        /// </summary>
        [DataMember(Name="NetworkSettings", EmitDefaultValue=false)]
        public ContainerSummaryInnerNetworkSettings NetworkSettings { get; set; }

        /// <summary>
        /// Gets or Sets Mounts
        /// </summary>
        [DataMember(Name="Mounts", EmitDefaultValue=false)]
        public List<Mount> Mounts { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ContainerSummaryInner {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Names: ").Append(Names).Append("\n");
            sb.Append("  Image: ").Append(Image).Append("\n");
            sb.Append("  ImageID: ").Append(ImageID).Append("\n");
            sb.Append("  Command: ").Append(Command).Append("\n");
            sb.Append("  Created: ").Append(Created).Append("\n");
            sb.Append("  Ports: ").Append(Ports).Append("\n");
            sb.Append("  SizeRw: ").Append(SizeRw).Append("\n");
            sb.Append("  SizeRootFs: ").Append(SizeRootFs).Append("\n");
            sb.Append("  Labels: ").Append(Labels).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  HostConfig: ").Append(HostConfig).Append("\n");
            sb.Append("  NetworkSettings: ").Append(NetworkSettings).Append("\n");
            sb.Append("  Mounts: ").Append(Mounts).Append("\n");
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
            return this.Equals(input as ContainerSummaryInner);
        }

        /// <summary>
        /// Returns true if ContainerSummaryInner instances are equal
        /// </summary>
        /// <param name="input">Instance of ContainerSummaryInner to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ContainerSummaryInner input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Id == input.Id ||
                    (this.Id != null &&
                    this.Id.Equals(input.Id))
                ) && 
                (
                    this.Names == input.Names ||
                    this.Names != null &&
                    this.Names.SequenceEqual(input.Names)
                ) && 
                (
                    this.Image == input.Image ||
                    (this.Image != null &&
                    this.Image.Equals(input.Image))
                ) && 
                (
                    this.ImageID == input.ImageID ||
                    (this.ImageID != null &&
                    this.ImageID.Equals(input.ImageID))
                ) && 
                (
                    this.Command == input.Command ||
                    (this.Command != null &&
                    this.Command.Equals(input.Command))
                ) && 
                (
                    this.Created == input.Created ||
                    (this.Created != null &&
                    this.Created.Equals(input.Created))
                ) && 
                (
                    this.Ports == input.Ports ||
                    this.Ports != null &&
                    this.Ports.SequenceEqual(input.Ports)
                ) && 
                (
                    this.SizeRw == input.SizeRw ||
                    (this.SizeRw != null &&
                    this.SizeRw.Equals(input.SizeRw))
                ) && 
                (
                    this.SizeRootFs == input.SizeRootFs ||
                    (this.SizeRootFs != null &&
                    this.SizeRootFs.Equals(input.SizeRootFs))
                ) && 
                (
                    this.Labels == input.Labels ||
                    this.Labels != null &&
                    this.Labels.SequenceEqual(input.Labels)
                ) && 
                (
                    this.State == input.State ||
                    (this.State != null &&
                    this.State.Equals(input.State))
                ) && 
                (
                    this.Status == input.Status ||
                    (this.Status != null &&
                    this.Status.Equals(input.Status))
                ) && 
                (
                    this.HostConfig == input.HostConfig ||
                    (this.HostConfig != null &&
                    this.HostConfig.Equals(input.HostConfig))
                ) && 
                (
                    this.NetworkSettings == input.NetworkSettings ||
                    (this.NetworkSettings != null &&
                    this.NetworkSettings.Equals(input.NetworkSettings))
                ) && 
                (
                    this.Mounts == input.Mounts ||
                    this.Mounts != null &&
                    this.Mounts.SequenceEqual(input.Mounts)
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
                if (this.Id != null)
                    hashCode = hashCode * 59 + this.Id.GetHashCode();
                if (this.Names != null)
                    hashCode = hashCode * 59 + this.Names.GetHashCode();
                if (this.Image != null)
                    hashCode = hashCode * 59 + this.Image.GetHashCode();
                if (this.ImageID != null)
                    hashCode = hashCode * 59 + this.ImageID.GetHashCode();
                if (this.Command != null)
                    hashCode = hashCode * 59 + this.Command.GetHashCode();
                if (this.Created != null)
                    hashCode = hashCode * 59 + this.Created.GetHashCode();
                if (this.Ports != null)
                    hashCode = hashCode * 59 + this.Ports.GetHashCode();
                if (this.SizeRw != null)
                    hashCode = hashCode * 59 + this.SizeRw.GetHashCode();
                if (this.SizeRootFs != null)
                    hashCode = hashCode * 59 + this.SizeRootFs.GetHashCode();
                if (this.Labels != null)
                    hashCode = hashCode * 59 + this.Labels.GetHashCode();
                if (this.State != null)
                    hashCode = hashCode * 59 + this.State.GetHashCode();
                if (this.Status != null)
                    hashCode = hashCode * 59 + this.Status.GetHashCode();
                if (this.HostConfig != null)
                    hashCode = hashCode * 59 + this.HostConfig.GetHashCode();
                if (this.NetworkSettings != null)
                    hashCode = hashCode * 59 + this.NetworkSettings.GetHashCode();
                if (this.Mounts != null)
                    hashCode = hashCode * 59 + this.Mounts.GetHashCode();
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
