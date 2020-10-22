# RunspacesApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**createRunspace**](RunspacesApi.md#createRunspace) | **POST** /api/runspaces | Starts a runspace creation
[**deleteRunspace**](RunspacesApi.md#deleteRunspace) | **DELETE** /api/runspaces/{id} | Deletes a runspace
[**getRunspace**](RunspacesApi.md#getRunspace) | **GET** /api/runspaces/{id} | Retrieve a runspace
[**listRunspaces**](RunspacesApi.md#listRunspaces) | **GET** /api/runspaces | List all runspaces

<a name="createRunspace"></a>
# **createRunspace**
> Runspace createRunspace(body)

Starts a runspace creation

### Create a runspace  Runspace creation and preparation time depends on requested runspace details.  If connection to VCenter Servers is requested the operation is going to create a PowerShell instance, load and connect a PowerCLI module to the VCenter.  ### Returns  When request is accepted **202 Accepted** - response code, with **Location** header is returned in the response that leads you to the **runspace** resource that is in creation state initially.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.RunspacesApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

RunspacesApi apiInstance = new RunspacesApi();
Runspace body = new Runspace(); // Runspace | Desired runspace resource.
try {
    Runspace result = apiInstance.createRunspace(body);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling RunspacesApi#createRunspace");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **body** | [**Runspace**](Runspace.md)| Desired runspace resource. | [optional]

### Return type

[**Runspace**](Runspace.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json

<a name="deleteRunspace"></a>
# **deleteRunspace**
> deleteRunspace(id)

Deletes a runspace

### Deletes a runspace  Deletes the PowerShell instance that is prepresented by this **runspace** resource.  Running script in the PowerShell won&#x27;t prevent the operation.  ### Returns  When requesting the Id of a runspace that has been deleted or doesn&#x27;t exist **404 NotFound** is returned.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.RunspacesApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

RunspacesApi apiInstance = new RunspacesApi();
String id = "id_example"; // String | 
try {
    apiInstance.deleteRunspace(id);
} catch (ApiException e) {
    System.err.println("Exception when calling RunspacesApi#deleteRunspace");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**|  |

### Return type

null (empty response body)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="getRunspace"></a>
# **getRunspace**
> Runspace getRunspace(id)

Retrieve a runspace

### Retrieve a runspace  Retrieves the details of a runspace. One only needs to supply the unique runspace identifier that was returned on runspace creation to retrieve runspace details.  ### Returns  Returns a **runspace** resource instance if a valid identifier was provided.  When requesting the Id of a runspace that has been deleted or doesn&#x27;t exist **404 NotFound** is returned.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.RunspacesApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

RunspacesApi apiInstance = new RunspacesApi();
String id = "id_example"; // String | 
try {
    Runspace result = apiInstance.getRunspace(id);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling RunspacesApi#getRunspace");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**|  |

### Return type

[**Runspace**](Runspace.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="listRunspaces"></a>
# **listRunspaces**
> List&lt;Runspace&gt; listRunspaces()

List all runspaces

### List all runspaces  ### Returns  Returns a list of your runspaces.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.RunspacesApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

RunspacesApi apiInstance = new RunspacesApi();
try {
    List<Runspace> result = apiInstance.listRunspaces();
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling RunspacesApi#listRunspaces");
    e.printStackTrace();
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**List&lt;Runspace&gt;**](Runspace.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

