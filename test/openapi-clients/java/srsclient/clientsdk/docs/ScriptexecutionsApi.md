# ScriptexecutionsApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**cancelScriptExecution**](ScriptexecutionsApi.md#cancelScriptExecution) | **POST** /api/script-executions/{id}/cancel | Cancels a script execution
[**createScriptExecution**](ScriptexecutionsApi.md#createScriptExecution) | **POST** /api/script-executions | Creates a script execution
[**getScriptExecution**](ScriptexecutionsApi.md#getScriptExecution) | **GET** /api/script-executions/{id} | Retrieve a script execution
[**getScriptExecutionOutput**](ScriptexecutionsApi.md#getScriptExecutionOutput) | **GET** /api/script-executions/{id}/output | Retrieves output objects produced by a script execution.
[**getScriptExecutionStream**](ScriptexecutionsApi.md#getScriptExecutionStream) | **GET** /api/script-executions/{id}/streams/{stream-type} | Retrieves list of stream records received during script execution.
[**listScriptExecutions**](ScriptexecutionsApi.md#listScriptExecutions) | **GET** /api/script-executions | List all script executions

<a name="cancelScriptExecution"></a>
# **cancelScriptExecution**
> cancelScriptExecution(id)

Cancels a script execution

### Cancel a script execution  This operation is equivalent of pressing Ctrl+C in the PowerShell console. If the script is cancellable it will be cancelled.  The state of the **script execution** will become cancelled after this operation. The operation is asynchronous. Cancel request  is sent to the runtime.    ### Returns  The operation doesn&#x27;t return value. **200 Ok** will be returned if the request is successful.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ScriptexecutionsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ScriptexecutionsApi apiInstance = new ScriptexecutionsApi();
String id = "id_example"; // String | The id of the script execution
try {
    apiInstance.cancelScriptExecution(id);
} catch (ApiException e) {
    System.err.println("Exception when calling ScriptexecutionsApi#cancelScriptExecution");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**| The id of the script execution |

### Return type

null (empty response body)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="createScriptExecution"></a>
# **createScriptExecution**
> ScriptExecution createScriptExecution(body)

Creates a script execution

### Create a script execution  **Script execution** represents asynchronous execution of a script in a specified **runspace**  When created **script execution** starts running in the **runspace**. To monitor the script execution  progress polling of the resource by its identifier should be used.  ### Retruns  When request is accepted **202 Accepted** with **Location** header is returned in the response that leads you to the **script execution** resource that is in running state initially.  When script execution is requested with non existing runspace  **404 Not Found** is returned.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ScriptexecutionsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ScriptexecutionsApi apiInstance = new ScriptexecutionsApi();
ScriptExecution body = new ScriptExecution(); // ScriptExecution | Desired script execution resource.
try {
    ScriptExecution result = apiInstance.createScriptExecution(body);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ScriptexecutionsApi#createScriptExecution");
    e.printStackTrace();
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

<a name="getScriptExecution"></a>
# **getScriptExecution**
> ScriptExecution getScriptExecution(id)

Retrieve a script execution

### Retrieve a script execution  Retrieves the details of a **script execution**. You need only supply the unique script execution identifier that was returned on script execution creation.        ### Returns  Returns a **script execution** resource instance if a valid identifier was provided.  When requesting the Id of a script execution that doesn&#x27;t exist **404 NotFound** is returned.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ScriptexecutionsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ScriptexecutionsApi apiInstance = new ScriptexecutionsApi();
String id = "id_example"; // String | 
try {
    ScriptExecution result = apiInstance.getScriptExecution(id);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ScriptexecutionsApi#getScriptExecution");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**|  |

### Return type

[**ScriptExecution**](ScriptExecution.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="getScriptExecutionOutput"></a>
# **getScriptExecutionOutput**
> List&lt;String&gt; getScriptExecutionOutput(id)

Retrieves output objects produced by a script execution.

### Retrieves output objects produced by a script execution ###  Output object could be in different format depending on the requested output object format on **script execution** request.  Text and json are currently supported.  When text is requested the objects that are produces ad an output by the script execution are formatted in table, the same way Format-Table formats the objects in PowerShell. Each item in the list of string represents single line of formatted output.  When output is formatted in json custom json formatting is used to serialize the objects produced by the script execution. The json object contain type name and full name of the interfaces the object implements. This is suitable if you want to present the objects in some context.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ScriptexecutionsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ScriptexecutionsApi apiInstance = new ScriptexecutionsApi();
String id = "id_example"; // String | Unique identifier of the script execution
try {
    List<String> result = apiInstance.getScriptExecutionOutput(id);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ScriptexecutionsApi#getScriptExecutionOutput");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**| Unique identifier of the script execution |

### Return type

**List&lt;String&gt;**

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="getScriptExecutionStream"></a>
# **getScriptExecutionStream**
> List&lt;StreamRecord&gt; getScriptExecutionStream(id, streamType)

Retrieves list of stream records received during script execution.

### Retrieves list of stream records received during script execution  During execution of a script the script execution engine collects streams that are produced by the script execution.  There are five types of stream: information, error, warning, debug, verbose.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ScriptexecutionsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ScriptexecutionsApi apiInstance = new ScriptexecutionsApi();
String id = "id_example"; // String | Unique identifier of the script execution
StreamType streamType = new StreamType(); // StreamType | Type of the stream for which records to be rterieved
try {
    List<StreamRecord> result = apiInstance.getScriptExecutionStream(id, streamType);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ScriptexecutionsApi#getScriptExecutionStream");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**| Unique identifier of the script execution |
 **streamType** | [**StreamType**](.md)| Type of the stream for which records to be rterieved |

### Return type

[**List&lt;StreamRecord&gt;**](StreamRecord.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="listScriptExecutions"></a>
# **listScriptExecutions**
> List&lt;ScriptExecution&gt; listScriptExecutions()

List all script executions

### List all script executions        ### Returns  Returns a list of your script executions.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ScriptexecutionsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ScriptexecutionsApi apiInstance = new ScriptexecutionsApi();
try {
    List<ScriptExecution> result = apiInstance.listScriptExecutions();
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ScriptexecutionsApi#listScriptExecutions");
    e.printStackTrace();
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**List&lt;ScriptExecution&gt;**](ScriptExecution.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

