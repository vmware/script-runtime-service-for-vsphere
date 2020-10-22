/*
 * Script Runtime Service for vSphere
 * # Script Runtime Service API    Script Runtime Service for vSphere (SRS) allows running PowerShell and PowerCLI scripts. SRS is a VC add-on that is deployed separately from VCSA. SRS can be accessed via REST API that allows you to create PowerShell instances and run PowerShell and PowerCLI scripts within. No Connect-VIServer is required to run PowerCLI against VC(s) SRS is registered to.    ## Authetication    SRS uses VC SSO as Identity and Authentication Server. Two types of authentication are supported. SIGN and Basic. SIGN authentication is purposed for Service-To-Service access to SRS resources. For convenience of the end-users SRS supports basic authentication passing username and password which are used to acquire SAML HoK token for SRS solution. When basic is used SRS exchanges the username and password for SAML HoK token from the SSO server. SRS uses the SAML token to Connect PowerCLI to VC services in further operations.   On successful authentication SRS returns API Key which is required to authorize further SRS API calls.
 *
 * OpenAPI spec version: 1.0-oas3
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */

package io.swagger.client.model;

import java.util.Objects;
import java.util.Arrays;
import com.google.gson.TypeAdapter;
import com.google.gson.annotations.JsonAdapter;
import com.google.gson.annotations.SerializedName;
import com.google.gson.stream.JsonReader;
import com.google.gson.stream.JsonWriter;
import io.swagger.client.model.OutputObjectsFormat;
import io.swagger.client.model.ScriptExecutionState;
import io.swagger.v3.oas.annotations.media.Schema;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import org.threeten.bp.OffsetDateTime;
/**
 * Script Execution object allows you to run a script in a runspace.  The API allows you to create, cancel, and retrieve script executions.
 */
@Schema(description = "Script Execution object allows you to run a script in a runspace.  The API allows you to create, cancel, and retrieve script executions.")
@javax.annotation.Generated(value = "io.swagger.codegen.v3.generators.java.JavaClientCodegen", date = "2020-10-09T07:53:20.504Z[GMT]")
public class ScriptExecution {
  @SerializedName("id")
  private String id = null;

  @SerializedName("runspace_id")
  private String runspaceId = null;

  @SerializedName("name")
  private String name = null;

  @SerializedName("script")
  private String script = null;

  @SerializedName("script_parameters")
  private List<AnyOfScriptExecutionScriptParametersItems> scriptParameters = null;

  @SerializedName("output_objects_format")
  private OutputObjectsFormat outputObjectsFormat = null;

  @SerializedName("state")
  private ScriptExecutionState state = null;

  @SerializedName("reason")
  private String reason = null;

  @SerializedName("start_time")
  private OffsetDateTime startTime = null;

  @SerializedName("end_time")
  private OffsetDateTime endTime = null;

   /**
   * Unique identifier for the object.
   * @return id
  **/
  @Schema(description = "Unique identifier for the object.")
  public String getId() {
    return id;
  }

  public ScriptExecution runspaceId(String runspaceId) {
    this.runspaceId = runspaceId;
    return this;
  }

   /**
   * Unique identifier of the runspace where script execution is performed.
   * @return runspaceId
  **/
  @Schema(required = true, description = "Unique identifier of the runspace where script execution is performed.")
  public String getRunspaceId() {
    return runspaceId;
  }

  public void setRunspaceId(String runspaceId) {
    this.runspaceId = runspaceId;
  }

  public ScriptExecution name(String name) {
    this.name = name;
    return this;
  }

   /**
   * Name of the script execution. It is optional to give a name of the script execution on create request. If name was not specified on script execution creation the field has null value.
   * @return name
  **/
  @Schema(description = "Name of the script execution. It is optional to give a name of the script execution on create request. If name was not specified on script execution creation the field has null value.")
  public String getName() {
    return name;
  }

  public void setName(String name) {
    this.name = name;
  }

  public ScriptExecution script(String script) {
    this.script = script;
    return this;
  }

   /**
   * Content of the script.
   * @return script
  **/
  @Schema(required = true, description = "Content of the script.")
  public String getScript() {
    return script;
  }

  public void setScript(String script) {
    this.script = script;
  }

  public ScriptExecution scriptParameters(List<AnyOfScriptExecutionScriptParametersItems> scriptParameters) {
    this.scriptParameters = scriptParameters;
    return this;
  }

  public ScriptExecution addScriptParametersItem(AnyOfScriptExecutionScriptParametersItems scriptParametersItem) {
    if (this.scriptParameters == null) {
      this.scriptParameters = new ArrayList<AnyOfScriptExecutionScriptParametersItems>();
    }
    this.scriptParameters.add(scriptParametersItem);
    return this;
  }

   /**
   * List of arguments that will be passed to the script.  If script content defines parameters argument can be provided.  The parameter names defined in the script content should match the names specified in this list.
   * @return scriptParameters
  **/
  @Schema(description = "List of arguments that will be passed to the script.  If script content defines parameters argument can be provided.  The parameter names defined in the script content should match the names specified in this list.")
  public List<AnyOfScriptExecutionScriptParametersItems> getScriptParameters() {
    return scriptParameters;
  }

  public void setScriptParameters(List<AnyOfScriptExecutionScriptParametersItems> scriptParameters) {
    this.scriptParameters = scriptParameters;
  }

  public ScriptExecution outputObjectsFormat(OutputObjectsFormat outputObjectsFormat) {
    this.outputObjectsFormat = outputObjectsFormat;
    return this;
  }

   /**
   * Get outputObjectsFormat
   * @return outputObjectsFormat
  **/
  @Schema(description = "")
  public OutputObjectsFormat getOutputObjectsFormat() {
    return outputObjectsFormat;
  }

  public void setOutputObjectsFormat(OutputObjectsFormat outputObjectsFormat) {
    this.outputObjectsFormat = outputObjectsFormat;
  }

  public ScriptExecution state(ScriptExecutionState state) {
    this.state = state;
    return this;
  }

   /**
   * Get state
   * @return state
  **/
  @Schema(description = "")
  public ScriptExecutionState getState() {
    return state;
  }

  public void setState(ScriptExecutionState state) {
    this.state = state;
  }

   /**
   * Reason for the current script execution state. In most of the cases reason field will be empty. In case  of an error or cancellation reason will contain information about the reason that caused script execution to  become in this state.
   * @return reason
  **/
  @Schema(description = "Reason for the current script execution state. In most of the cases reason field will be empty. In case  of an error or cancellation reason will contain information about the reason that caused script execution to  become in this state.")
  public String getReason() {
    return reason;
  }

   /**
   * Time at which the script execution was started. String representing time in format ISO 8601.
   * @return startTime
  **/
  @Schema(description = "Time at which the script execution was started. String representing time in format ISO 8601.")
  public OffsetDateTime getStartTime() {
    return startTime;
  }

   /**
   * Time at which the script execution was finished. String representing time in format ISO 8601.
   * @return endTime
  **/
  @Schema(description = "Time at which the script execution was finished. String representing time in format ISO 8601.")
  public OffsetDateTime getEndTime() {
    return endTime;
  }


  @Override
  public boolean equals(java.lang.Object o) {
    if (this == o) {
      return true;
    }
    if (o == null || getClass() != o.getClass()) {
      return false;
    }
    ScriptExecution scriptExecution = (ScriptExecution) o;
    return Objects.equals(this.id, scriptExecution.id) &&
        Objects.equals(this.runspaceId, scriptExecution.runspaceId) &&
        Objects.equals(this.name, scriptExecution.name) &&
        Objects.equals(this.script, scriptExecution.script) &&
        Objects.equals(this.scriptParameters, scriptExecution.scriptParameters) &&
        Objects.equals(this.outputObjectsFormat, scriptExecution.outputObjectsFormat) &&
        Objects.equals(this.state, scriptExecution.state) &&
        Objects.equals(this.reason, scriptExecution.reason) &&
        Objects.equals(this.startTime, scriptExecution.startTime) &&
        Objects.equals(this.endTime, scriptExecution.endTime);
  }

  @Override
  public int hashCode() {
    return Objects.hash(id, runspaceId, name, script, scriptParameters, outputObjectsFormat, state, reason, startTime, endTime);
  }


  @Override
  public String toString() {
    StringBuilder sb = new StringBuilder();
    sb.append("class ScriptExecution {\n");
    
    sb.append("    id: ").append(toIndentedString(id)).append("\n");
    sb.append("    runspaceId: ").append(toIndentedString(runspaceId)).append("\n");
    sb.append("    name: ").append(toIndentedString(name)).append("\n");
    sb.append("    script: ").append(toIndentedString(script)).append("\n");
    sb.append("    scriptParameters: ").append(toIndentedString(scriptParameters)).append("\n");
    sb.append("    outputObjectsFormat: ").append(toIndentedString(outputObjectsFormat)).append("\n");
    sb.append("    state: ").append(toIndentedString(state)).append("\n");
    sb.append("    reason: ").append(toIndentedString(reason)).append("\n");
    sb.append("    startTime: ").append(toIndentedString(startTime)).append("\n");
    sb.append("    endTime: ").append(toIndentedString(endTime)).append("\n");
    sb.append("}");
    return sb.toString();
  }

  /**
   * Convert the given object to string with each line indented by 4 spaces
   * (except the first line).
   */
  private String toIndentedString(java.lang.Object o) {
    if (o == null) {
      return "null";
    }
    return o.toString().replace("\n", "\n    ");
  }

}
