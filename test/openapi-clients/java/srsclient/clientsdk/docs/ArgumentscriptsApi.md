# ArgumentscriptsApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**createArgumentScriptsScript**](ArgumentscriptsApi.md#createArgumentScriptsScript) | **POST** /api/argument-scripts/script | Creates scripts for a given script template id and placeholder values
[**getArgumentScriptsTemplate**](ArgumentscriptsApi.md#getArgumentScriptsTemplate) | **GET** /api/argument-scripts/templates/{id} | Retrieves argument script template by given unique template identifier
[**listArgumentScriptsTemplates**](ArgumentscriptsApi.md#listArgumentScriptsTemplates) | **GET** /api/argument-scripts/templates | List available argument script templates

<a name="createArgumentScriptsScript"></a>
# **createArgumentScriptsScript**
> ArgumentScript createArgumentScriptsScript(body)

Creates scripts for a given script template id and placeholder values

### Creates scripts for a given script template id and placeholder values  Replaces the placeholders in a given argument transformation script template with given values on the placeholder_value_list field  The result script can be provided to a **script execution** parameter that expects specific script runtime type    ### Example  If the template argument transformation script is    Get-VM -Id &lt;vm-id&gt; -Server &lt;server&gt;    The result of this operation with given Id &#x27;vm-1&#x27; and Server &#x27;server-1&#x27; would be    Get-VM -Id &#x27;vm-1&#x27; -Server &#x27;server-1&#x27;

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ArgumentscriptsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ArgumentscriptsApi apiInstance = new ArgumentscriptsApi();
ArgumentScript body = new ArgumentScript(); // ArgumentScript | The argument script create request
try {
    ArgumentScript result = apiInstance.createArgumentScriptsScript(body);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ArgumentscriptsApi#createArgumentScriptsScript");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **body** | [**ArgumentScript**](ArgumentScript.md)| The argument script create request | [optional]

### Return type

[**ArgumentScript**](ArgumentScript.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json

<a name="getArgumentScriptsTemplate"></a>
# **getArgumentScriptsTemplate**
> ArgumentScriptTemplate getArgumentScriptsTemplate(id)

Retrieves argument script template by given unique template identifier

### Retrieves argument script template by given unique template identifier  This operation returns argument script template for the specified template id.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ArgumentscriptsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ArgumentscriptsApi apiInstance = new ArgumentscriptsApi();
String id = "id_example"; // String | The Id of the argument script template
try {
    ArgumentScriptTemplate result = apiInstance.getArgumentScriptsTemplate(id);
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ArgumentscriptsApi#getArgumentScriptsTemplate");
    e.printStackTrace();
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **String**| The Id of the argument script template |

### Return type

[**ArgumentScriptTemplate**](ArgumentScriptTemplate.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="listArgumentScriptsTemplates"></a>
# **listArgumentScriptsTemplates**
> List&lt;ArgumentScriptTemplate&gt; listArgumentScriptsTemplates()

List available argument script templates

### LList available argument script templates  Argument script templates are scripts with placeholders. When placeholders are replaced by values script can be executed in a given script runtime.  Argument script templates are designed to help to convert simple type values to objects of types that can only be produced in a given script runtime. Those object can be used as arguments to scripts&#x27; parameters.    This operation retrieves the available argument script templates.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.ArgumentscriptsApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

ArgumentscriptsApi apiInstance = new ArgumentscriptsApi();
try {
    List<ArgumentScriptTemplate> result = apiInstance.listArgumentScriptsTemplates();
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling ArgumentscriptsApi#listArgumentScriptsTemplates");
    e.printStackTrace();
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**List&lt;ArgumentScriptTemplate&gt;**](ArgumentScriptTemplate.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

