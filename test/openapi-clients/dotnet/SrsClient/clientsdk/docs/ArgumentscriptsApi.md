# IO.Swagger.Api.ArgumentscriptsApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**CreateArgumentScriptsScript**](ArgumentscriptsApi.md#createargumentscriptsscript) | **POST** /api/argument-scripts/script | Creates scripts for a given script template id and placeholder values
[**GetArgumentScriptsTemplate**](ArgumentscriptsApi.md#getargumentscriptstemplate) | **GET** /api/argument-scripts/templates/{id} | Retrieves argument script template by given unique template identifier
[**ListArgumentScriptsTemplates**](ArgumentscriptsApi.md#listargumentscriptstemplates) | **GET** /api/argument-scripts/templates | List available argument script templates

<a name="createargumentscriptsscript"></a>
# **CreateArgumentScriptsScript**
> ArgumentScript CreateArgumentScriptsScript (ArgumentScript body = null)

Creates scripts for a given script template id and placeholder values

### Creates scripts for a given script template id and placeholder values  Replaces the placeholders in a given argument transformation script template with given values on the placeholder_value_list field  The result script can be provided to a **script execution** parameter that expects specific script runtime type    ### Example  If the template argument transformation script is    Get-VM -Id <vm-id> -Server <server>    The result of this operation with given Id 'vm-1' and Server 'server-1' would be    Get-VM -Id 'vm-1' -Server 'server-1'

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class CreateArgumentScriptsScriptExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ArgumentscriptsApi();
            var body = new ArgumentScript(); // ArgumentScript | The argument script create request (optional) 

            try
            {
                // Creates scripts for a given script template id and placeholder values
                ArgumentScript result = apiInstance.CreateArgumentScriptsScript(body);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ArgumentscriptsApi.CreateArgumentScriptsScript: " + e.Message );
            }
        }
    }
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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="getargumentscriptstemplate"></a>
# **GetArgumentScriptsTemplate**
> ArgumentScriptTemplate GetArgumentScriptsTemplate (string id)

Retrieves argument script template by given unique template identifier

### Retrieves argument script template by given unique template identifier  This operation returns argument script template for the specified template id.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetArgumentScriptsTemplateExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ArgumentscriptsApi();
            var id = id_example;  // string | The Id of the argument script template

            try
            {
                // Retrieves argument script template by given unique template identifier
                ArgumentScriptTemplate result = apiInstance.GetArgumentScriptsTemplate(id);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ArgumentscriptsApi.GetArgumentScriptsTemplate: " + e.Message );
            }
        }
    }
}
```

### Parameters

Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **string**| The Id of the argument script template | 

### Return type

[**ArgumentScriptTemplate**](ArgumentScriptTemplate.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="listargumentscriptstemplates"></a>
# **ListArgumentScriptsTemplates**
> List<ArgumentScriptTemplate> ListArgumentScriptsTemplates ()

List available argument script templates

### LList available argument script templates  Argument script templates are scripts with placeholders. When placeholders are replaced by values script can be executed in a given script runtime.  Argument script templates are designed to help to convert simple type values to objects of types that can only be produced in a given script runtime. Those object can be used as arguments to scripts' parameters.    This operation retrieves the available argument script templates.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class ListArgumentScriptsTemplatesExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new ArgumentscriptsApi();

            try
            {
                // List available argument script templates
                List&lt;ArgumentScriptTemplate&gt; result = apiInstance.ListArgumentScriptsTemplates();
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling ArgumentscriptsApi.ListArgumentScriptsTemplates: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**List<ArgumentScriptTemplate>**](ArgumentScriptTemplate.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
