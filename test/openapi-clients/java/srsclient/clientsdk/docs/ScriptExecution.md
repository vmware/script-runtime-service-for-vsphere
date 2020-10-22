# ScriptExecution

## Properties
Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **String** | Unique identifier for the object. |  [optional]
**runspaceId** | **String** | Unique identifier of the runspace where script execution is performed. | 
**name** | **String** | Name of the script execution. It is optional to give a name of the script execution on create request. If name was not specified on script execution creation the field has null value. |  [optional]
**script** | **String** | Content of the script. | 
**scriptParameters** | **List&lt;AnyOfScriptExecutionScriptParametersItems&gt;** | List of arguments that will be passed to the script.  If script content defines parameters argument can be provided.  The parameter names defined in the script content should match the names specified in this list. |  [optional]
**outputObjectsFormat** | [**OutputObjectsFormat**](OutputObjectsFormat.md) |  |  [optional]
**state** | [**ScriptExecutionState**](ScriptExecutionState.md) |  |  [optional]
**reason** | **String** | Reason for the current script execution state. In most of the cases reason field will be empty. In case  of an error or cancellation reason will contain information about the reason that caused script execution to  become in this state. |  [optional]
**startTime** | [**OffsetDateTime**](OffsetDateTime.md) | Time at which the script execution was started. String representing time in format ISO 8601. |  [optional]
**endTime** | [**OffsetDateTime**](OffsetDateTime.md) | Time at which the script execution was finished. String representing time in format ISO 8601. |  [optional]
