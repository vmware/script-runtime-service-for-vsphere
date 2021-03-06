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
    /// Represents generic information about swarm. 
    /// </summary>
    [DataContract]
    public partial class SwarmInfo :  IEquatable<SwarmInfo>, IValidatableObject
    {
        /// <summary>
        /// Gets or Sets LocalNodeState
        /// </summary>
        [DataMember(Name="LocalNodeState", EmitDefaultValue=false)]
        public LocalNodeState? LocalNodeState { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="SwarmInfo" /> class.
        /// </summary>
        /// <param name="nodeID">Unique identifier of for this node in the swarm. (default to &quot;&quot;).</param>
        /// <param name="nodeAddr">IP address at which this node can be reached by other nodes in the swarm.  (default to &quot;&quot;).</param>
        /// <param name="localNodeState">localNodeState.</param>
        /// <param name="controlAvailable">controlAvailable (default to false).</param>
        /// <param name="error">error (default to &quot;&quot;).</param>
        /// <param name="remoteManagers">List of ID&#39;s and addresses of other managers in the swarm. .</param>
        /// <param name="nodes">Total number of nodes in the swarm..</param>
        /// <param name="managers">Total number of managers in the swarm..</param>
        /// <param name="cluster">cluster.</param>
        public SwarmInfo(string nodeID = "", string nodeAddr = "", LocalNodeState? localNodeState = default(LocalNodeState?), bool? controlAvailable = false, string error = "", List<PeerNode> remoteManagers = default(List<PeerNode>), int? nodes = default(int?), int? managers = default(int?), ClusterInfo cluster = default(ClusterInfo))
        {
            // use default value if no "nodeID" provided
            if (nodeID == null)
            {
                this.NodeID = "";
            }
            else
            {
                this.NodeID = nodeID;
            }
            // use default value if no "nodeAddr" provided
            if (nodeAddr == null)
            {
                this.NodeAddr = "";
            }
            else
            {
                this.NodeAddr = nodeAddr;
            }
            this.LocalNodeState = localNodeState;
            // use default value if no "controlAvailable" provided
            if (controlAvailable == null)
            {
                this.ControlAvailable = false;
            }
            else
            {
                this.ControlAvailable = controlAvailable;
            }
            // use default value if no "error" provided
            if (error == null)
            {
                this.Error = "";
            }
            else
            {
                this.Error = error;
            }
            this.RemoteManagers = remoteManagers;
            this.Nodes = nodes;
            this.Managers = managers;
            this.Cluster = cluster;
        }
        
        /// <summary>
        /// Unique identifier of for this node in the swarm.
        /// </summary>
        /// <value>Unique identifier of for this node in the swarm.</value>
        [DataMember(Name="NodeID", EmitDefaultValue=false)]
        public string NodeID { get; set; }

        /// <summary>
        /// IP address at which this node can be reached by other nodes in the swarm. 
        /// </summary>
        /// <value>IP address at which this node can be reached by other nodes in the swarm. </value>
        [DataMember(Name="NodeAddr", EmitDefaultValue=false)]
        public string NodeAddr { get; set; }


        /// <summary>
        /// Gets or Sets ControlAvailable
        /// </summary>
        [DataMember(Name="ControlAvailable", EmitDefaultValue=false)]
        public bool? ControlAvailable { get; set; }

        /// <summary>
        /// Gets or Sets Error
        /// </summary>
        [DataMember(Name="Error", EmitDefaultValue=false)]
        public string Error { get; set; }

        /// <summary>
        /// List of ID&#39;s and addresses of other managers in the swarm. 
        /// </summary>
        /// <value>List of ID&#39;s and addresses of other managers in the swarm. </value>
        [DataMember(Name="RemoteManagers", EmitDefaultValue=false)]
        public List<PeerNode> RemoteManagers { get; set; }

        /// <summary>
        /// Total number of nodes in the swarm.
        /// </summary>
        /// <value>Total number of nodes in the swarm.</value>
        [DataMember(Name="Nodes", EmitDefaultValue=false)]
        public int? Nodes { get; set; }

        /// <summary>
        /// Total number of managers in the swarm.
        /// </summary>
        /// <value>Total number of managers in the swarm.</value>
        [DataMember(Name="Managers", EmitDefaultValue=false)]
        public int? Managers { get; set; }

        /// <summary>
        /// Gets or Sets Cluster
        /// </summary>
        [DataMember(Name="Cluster", EmitDefaultValue=false)]
        public ClusterInfo Cluster { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SwarmInfo {\n");
            sb.Append("  NodeID: ").Append(NodeID).Append("\n");
            sb.Append("  NodeAddr: ").Append(NodeAddr).Append("\n");
            sb.Append("  LocalNodeState: ").Append(LocalNodeState).Append("\n");
            sb.Append("  ControlAvailable: ").Append(ControlAvailable).Append("\n");
            sb.Append("  Error: ").Append(Error).Append("\n");
            sb.Append("  RemoteManagers: ").Append(RemoteManagers).Append("\n");
            sb.Append("  Nodes: ").Append(Nodes).Append("\n");
            sb.Append("  Managers: ").Append(Managers).Append("\n");
            sb.Append("  Cluster: ").Append(Cluster).Append("\n");
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
            return this.Equals(input as SwarmInfo);
        }

        /// <summary>
        /// Returns true if SwarmInfo instances are equal
        /// </summary>
        /// <param name="input">Instance of SwarmInfo to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SwarmInfo input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.NodeID == input.NodeID ||
                    (this.NodeID != null &&
                    this.NodeID.Equals(input.NodeID))
                ) && 
                (
                    this.NodeAddr == input.NodeAddr ||
                    (this.NodeAddr != null &&
                    this.NodeAddr.Equals(input.NodeAddr))
                ) && 
                (
                    this.LocalNodeState == input.LocalNodeState ||
                    (this.LocalNodeState != null &&
                    this.LocalNodeState.Equals(input.LocalNodeState))
                ) && 
                (
                    this.ControlAvailable == input.ControlAvailable ||
                    (this.ControlAvailable != null &&
                    this.ControlAvailable.Equals(input.ControlAvailable))
                ) && 
                (
                    this.Error == input.Error ||
                    (this.Error != null &&
                    this.Error.Equals(input.Error))
                ) && 
                (
                    this.RemoteManagers == input.RemoteManagers ||
                    this.RemoteManagers != null &&
                    this.RemoteManagers.SequenceEqual(input.RemoteManagers)
                ) && 
                (
                    this.Nodes == input.Nodes ||
                    (this.Nodes != null &&
                    this.Nodes.Equals(input.Nodes))
                ) && 
                (
                    this.Managers == input.Managers ||
                    (this.Managers != null &&
                    this.Managers.Equals(input.Managers))
                ) && 
                (
                    this.Cluster == input.Cluster ||
                    (this.Cluster != null &&
                    this.Cluster.Equals(input.Cluster))
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
                if (this.NodeID != null)
                    hashCode = hashCode * 59 + this.NodeID.GetHashCode();
                if (this.NodeAddr != null)
                    hashCode = hashCode * 59 + this.NodeAddr.GetHashCode();
                if (this.LocalNodeState != null)
                    hashCode = hashCode * 59 + this.LocalNodeState.GetHashCode();
                if (this.ControlAvailable != null)
                    hashCode = hashCode * 59 + this.ControlAvailable.GetHashCode();
                if (this.Error != null)
                    hashCode = hashCode * 59 + this.Error.GetHashCode();
                if (this.RemoteManagers != null)
                    hashCode = hashCode * 59 + this.RemoteManagers.GetHashCode();
                if (this.Nodes != null)
                    hashCode = hashCode * 59 + this.Nodes.GetHashCode();
                if (this.Managers != null)
                    hashCode = hashCode * 59 + this.Managers.GetHashCode();
                if (this.Cluster != null)
                    hashCode = hashCode * 59 + this.Cluster.GetHashCode();
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
