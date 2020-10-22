# IO.Swagger.Api.ScriptexecutionsApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**CancelScriptExecution**](ScriptexecutionsApi.md#cancelscriptexecution) | **POST** /api/script-executions/{id}/cancel | Cancels a script execution
[**CreateScriptExecution**](ScriptexecutionsApi.md#createscriptexecution) | **POST** /api/script-executions | Creates a script execution
[**GetScriptExecution**](ScriptexecutionsApi.md#getscriptexecution) | **GET** /api/script-executions/{id} | Retrieve a script execution
[**GetScriptExecutionOutput**](ScriptexecutionsApi.md#getscriptexecutionoutput) | **GET** /api/script-executions/{id}/output | Retrieves output objects produced by a script execution.
[**GetScriptExecutionStream**](ScriptexecutionsApi.md#getscriptexecutionstream) | **GET** /api/script-executions/{id}/streams/{stream-type} | Retrieves list of stream records received during script execution.
[**ListScriptExecutions**](ScriptexecutionsApi.md#listscriptexecutions) | **GET** /api/script-executions | List all script executions

<a name="cancelscriptexecution"></a>
# **CancelScriptExecution**
> void CancelScriptExecution (string id)

Cancels a script execution

### Cancel a script execution  This operation is equivalent of pressing Ctrl+C in the PowerShell console. If the script is cancellable it will be cancelled.  The state of the **script execution** will become cancelled after this operation. The operation is asynchronous. Cancel request  is sent to the runtime.    ### Returns  The operation doesn't return value. **200 Ok** will be returned if the request is successful.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class CancelScriptExecutionExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ScriptexecutionsApi();
            var id = id_example;  // string | The id of the script execution

            try
            {
                // Cancels a script execution
                apiInstance.CancelScriptExecution(id);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ScriptexecutionsApi.CancelScriptExecution: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **string**| The id of the script execution | 

### Return type

void (empty response body)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="createscriptexecution"></a>
# **CreateScriptExecution**
> ScriptExecution CreateScriptExecution (ScriptExecution body = null)

Creates a script execution

### Create a script execution  **Script execution** represents asynchronous execution of a script in a specified **runspace**  When created **script execution** starts running in the **runspace**. To monitor the script execution  progress polling of the resource by its identifier should be used.  ### Retruns  When request is accepted **202 Accepted** with **Location** header is returned in the response that leads you to the **script execution** resource that is in running state initially.  When script execution is requested with non existing runspace  **404 Not Found** is returned.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class CreateScriptExecutionExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ScriptexecutionsApi();
            var body = new ScriptExecution(); // ScriptExecution | Desired script execution resource. (optional) 

            try
            {
                // Creates a script execution
                ScriptExecution result = apiInstance.CreateScriptExecution(body);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ScriptexecutionsApi.CreateScriptExecution: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **body** | [**ScriptExecution**](ScriptExecution.md)| Desired script execution resource. | [optional] 

### Return type

[**ScriptExecution**](ScriptExecution.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="getscriptexecution"></a>
# **GetScriptExecution**
> ScriptExecution GetScriptExecution (string id)

Retrieve a script execution

### Retrieve a script execution  Retrieves the details of a **script execution**. You need only supply the unique script execution identifier that was returned on script execution creation.        ### Returns  Returns a **script execution** resource instance if a valid identifier was provided.  When requesting the Id of a script execution that doesn't exist **404 NotFound** is returned.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetScriptExecutionExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ScriptexecutionsApi();
            var id = id_example;  // string | 

            try
            {
                // Retrieve a script execution
                ScriptExecution result = apiInstance.GetScriptExecution(id);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ScriptexecutionsApi.GetScriptExecution: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **string**|  | 

### Return type

[**ScriptExecution**](ScriptExecution.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="getscriptexecutionoutput"></a>
# **GetScriptExecutionOutput**
> List<string> GetScriptExecutionOutput (string id)

Retrieves output objects produced by a script execution.

### Retrieves output objects produced by a script execution ###  Output object could be in different format depending on the requested output object format on **script execution** request.  Text and json are currently supported.  When text is requested the objects that are produces ad an output by the script execution are formatted in table, the same way Format-Table formats the objects in PowerShell. Each item in the list of string represents single line of formatted output.  When output is formatted in json custom json formatting is used to serialize the objects produced by the script execution. The json object contain type name and full name of the interfaces the object implements. This is suitable if you want to present the objects in some context.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetScriptExecutionOutputExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ScriptexecutionsApi();
            var id = id_example;  // string | Unique identifier of the script execution

            try
            {
                // Retrieves output objects produced by a script execution.
                List&lt;string&gt; result = apiInstance.GetScriptExecutionOutput(id);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ScriptexecutionsApi.GetScriptExecutionOutput: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **string**| Unique identifier of the script execution | 

### Return type

**List<string>**

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="getscriptexecutionstream"></a>
# **GetScriptExecutionStream**
> List<StreamRecord> GetScriptExecutionStream (string id, StreamType streamType)

Retrieves list of stream records received during script execution.

### Retrieves list of stream records received during script execution  During execution of a script the script execution engine collects streams that are produced by the script execution.  There are five types of stream: information, error, warning, debug, verbose.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetScriptExecutionStreamExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ScriptexecutionsApi();
            var id = id_example;  // string | Unique identifier of the script execution
            var streamType = new StreamType(); // StreamType | Type of the stream for which records to be rterieved

            try
            {
                // Retrieves list of stream records received during script execution.
                List&lt;StreamRecord&gt; result = apiInstance.GetScriptExecutionStream(id, streamType);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ScriptexecutionsApi.GetScriptExecutionStream: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **string**| Unique identifier of the script execution | 
 **streamType** | [**StreamType**](StreamType.md)| Type of the stream for which records to be rterieved | 

### Return type

[**List<StreamRecord>**](StreamRecord.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="listscriptexecutions"></a>
# **ListScriptExecutions**
> List<ScriptExecution> ListScriptExecutions ()

List all script executions

### List all script executions        ### Returns  Returns a list of your script executions.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ListScriptExecutionsExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ScriptexecutionsApi();

            try
            {
                // List all script executions
                List&lt;ScriptExecution&gt; result = apiInstance.ListScriptExecutions();
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ScriptexecutionsApi.ListScriptExecutions: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**List<ScriptExecution>**](ScriptExecution.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
