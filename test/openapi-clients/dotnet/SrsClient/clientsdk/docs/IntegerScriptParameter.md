# IO.Swagger.Model.IntegerScriptParameter
## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Value** | **Object** | Object that will be passed as an argument to a given parameter. Value, script, or both can be provided as an  argument. If only value is provided without script the object is passed to the script&#x27;s parameter as is. | [optional] 
**Name** | **string** | Name of the parameter. When a parameter is specified on a script execution create the name should match  the name of the parameter that is defined in the script. | 
**Script** | **string** | Script to be executed for this parameter. Value produced by the script will be the argument for the parameter.    In case a script is specified as an argument for a script parameter the service runs the script of the  parameter before running the requested script. The value that is produced as an output is used  as an argument for the script parameter.  If both script and value are specified for a script parameter the script is executed with single argument  with value specified in the value field. The object that is produced as an output is used as an argument  for the script parameter. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)

