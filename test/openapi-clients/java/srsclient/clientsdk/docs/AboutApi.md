# AboutApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**getAbout**](AboutApi.md#getAbout) | **GET** /api/about | Retrieves about information for the product

<a name="getAbout"></a>
# **getAbout**
> About getAbout()

Retrieves about information for the product

### Retrieve about information  ### Returns  **about** resource with information for the product.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.AboutApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");
// Configure HTTP basic authorization: basicAuth
HttpBasicAuth basicAuth = (HttpBasicAuth) defaultClient.getAuthentication("basicAuth");
basicAuth.setUsername("YOUR USERNAME");
basicAuth.setPassword("YOUR PASSWORD");
// Configure HTTP basic authorization: signAuth
HttpBasicAuth signAuth = (HttpBasicAuth) defaultClient.getAuthentication("signAuth");
signAuth.setUsername("YOUR USERNAME");
signAuth.setPassword("YOUR PASSWORD");

AboutApi apiInstance = new AboutApi();
try {
    About result = apiInstance.getAbout();
    System.out.println(result);
} catch (ApiException e) {
    System.err.println("Exception when calling AboutApi#getAbout");
    e.printStackTrace();
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

[**About**](About.md)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)[basicAuth](../README.md#basicAuth)[signAuth](../README.md#signAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

