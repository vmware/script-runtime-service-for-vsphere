# IO.Swagger.Model.ScriptExecution
## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **string** | Unique identifier for the object. | [optional] 
**RunspaceId** | **string** | Unique identifier of the runspace where script execution is performed. | 
**Name** | **string** | Name of the script execution. It is optional to give a name of the script execution on create request. If name was not specified on script execution creation the field has null value. | [optional] 
**Script** | **string** | Content of the script. | 
**ScriptParameters** | [**List&lt;AnyOfScriptExecutionScriptParametersItems&gt;**](.md) | List of arguments that will be passed to the script.  If script content defines parameters argument can be provided.  The parameter names defined in the script content should match the names specified in this list. | [optional] 
**OutputObjectsFormat** | **OutputObjectsFormat** |  | [optional] 
**State** | **ScriptExecutionState** |  | [optional] 
**Reason** | **string** | Reason for the current script execution state. In most of the cases reason field will be empty. In case  of an error or cancellation reason will contain information about the reason that caused script execution to  become in this state. | [optional] 
**StartTime** | **DateTime?** | Time at which the script execution was started. String representing time in format ISO 8601. | [optional] 
**EndTime** | **DateTime?** | Time at which the script execution was finished. String representing time in format ISO 8601. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)

