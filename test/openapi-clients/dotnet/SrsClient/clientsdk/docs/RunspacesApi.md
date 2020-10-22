# IO.Swagger.Api.RunspacesApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**CreateRunspace**](RunspacesApi.md#createrunspace) | **POST** /api/runspaces | Starts a runspace creation
[**DeleteRunspace**](RunspacesApi.md#deleterunspace) | **DELETE** /api/runspaces/{id} | Deletes a runspace
[**GetRunspace**](RunspacesApi.md#getrunspace) | **GET** /api/runspaces/{id} | Retrieve a runspace
[**ListRunspaces**](RunspacesApi.md#listrunspaces) | **GET** /api/runspaces | List all runspaces

<a name="createrunspace"></a>
# **CreateRunspace**
> Runspace CreateRunspace (Runspace body = null)

Starts a runspace creation

### Create a runspace  Runspace creation and preparation time depends on requested runspace details.  If connection to VCenter Servers is requested the operation is going to create a PowerShell instance, load and connect a PowerCLI module to the VCenter.  ### Returns  When request is accepted **202 Accepted** - response code, with **Location** header is returned in the response that leads you to the **runspace** resource that is in creation state initially.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class CreateRunspaceExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new RunspacesApi();
            var body = new Runspace(); // Runspace | Desired runspace resource. (optional) 

            try
            {
                // Starts a runspace creation
                Runspace result = apiInstance.CreateRunspace(body);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling RunspacesApi.CreateRunspace: " + e.Message );
            }
        }
    }
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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="deleterunspace"></a>
# **DeleteRunspace**
> void DeleteRunspace (string id)

Deletes a runspace

### Deletes a runspace  Deletes the PowerShell instance that is prepresented by this **runspace** resource.  Running script in the PowerShell won't prevent the operation.  ### Returns  When requesting the Id of a runspace that has been deleted or doesn't exist **404 NotFound** is returned.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class DeleteRunspaceExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new RunspacesApi();
            var id = id_example;  // string | 

            try
            {
                // Deletes a runspace
                apiInstance.DeleteRunspace(id);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling RunspacesApi.DeleteRunspace: " + e.Message );
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

void (empty response body)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="getrunspace"></a>
# **GetRunspace**
> Runspace GetRunspace (string id)

Retrieve a runspace

### Retrieve a runspace  Retrieves the details of a runspace. One only needs to supply the unique runspace identifier that was returned on runspace creation to retrieve runspace details.  ### Returns  Returns a **runspace** resource instance if a valid identifier was provided.  When requesting the Id of a runspace that has been deleted or doesn't exist **404 NotFound** is returned.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetRunspaceExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new RunspacesApi();
            var id = id_example;  // string | 

            try
            {
                // Retrieve a runspace
                Runspace result = apiInstance.GetRunspace(id);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling RunspacesApi.GetRunspace: " + e.Message );
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

[**Runspace**](Runspace.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="listrunspaces"></a>
# **ListRunspaces**
> List<Runspace> ListRunspaces ()

List all runspaces

### List all runspaces  ### Returns  Returns a list of your runspaces.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ListRunspacesExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new RunspacesApi();

            try
            {
                // List all runspaces
                List&lt;Runspace&gt; result = apiInstance.ListRunspaces();
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling RunspacesApi.ListRunspaces: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**List<Runspace>**](Runspace.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
