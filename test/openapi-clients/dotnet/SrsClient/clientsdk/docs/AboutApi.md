# IO.Swagger.Api.AboutApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**GetAbout**](AboutApi.md#getabout) | **GET** /api/about | Retrieves about information for the product

<a name="getabout"></a>
# **GetAbout**
> About GetAbout ()

Retrieves about information for the product

### Retrieve about information  ### Returns  **about** resource with information for the product.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class GetAboutExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");
            // Configure HTTP basic authorization: basicAuth
            Configuration.Default.Username = "YOUR_USERNAME";
            Configuration.Default.Password = "YOUR_PASSWORD";
            // Configure HTTP basic authorization: signAuth
            Configuration.Default.Username = "YOUR_USERNAME";
            Configuration.Default.Password = "YOUR_PASSWORD";

            var apiInstance = new AboutApi();

            try
            {
                // Retrieves about information for the product
                About result = apiInstance.GetAbout();
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling AboutApi.GetAbout: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**About**](About.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth), [basicAuth](../README.md#basicAuth), [signAuth](../README.md#signAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
