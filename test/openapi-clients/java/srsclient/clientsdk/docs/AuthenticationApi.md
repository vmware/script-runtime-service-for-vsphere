# AuthenticationApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**login**](AuthenticationApi.md#login) | **POST** /api/auth/login | Exchanges client credentials or SIGN token for SRS access key
[**logout**](AuthenticationApi.md#logout) | **POST** /api/auth/logout | Revokes SRS access key

<a name="login"></a>
# **login**
> login()

Exchanges client credentials or SIGN token for SRS access key

Uses VCenter SSO as Identity and Authentication Server.  Two types of authentication are supported SIGN and Basic.  When Basic authentication is used service exchanges username and password for SAML from VCenter SSO.  When SIGN authentication is used service exchanges the SSO SAML token from the SIGN token for another SAML token on behalf of the user from VCenter SSO.  On successful authentication with SSO the service issues **X-SRS-API-KEY** token are returns it the response headers. **X-SRS-API-KEY** token is used to authorize access to service resources.   The service associates **X-SRS-API-KEY** token to acquired from SSO SAML token. The associated SSO SAML token is used to authorize PowerCLI to VCenter services.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.AuthenticationApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();
// Configure HTTP basic authorization: basicAuth
HttpBasicAuth basicAuth = (HttpBasicAuth) defaultClient.getAuthentication("basicAuth");
basicAuth.setUsername("YOUR USERNAME");
basicAuth.setPassword("YOUR PASSWORD");
// Configure HTTP basic authorization: signAuth
HttpBasicAuth signAuth = (HttpBasicAuth) defaultClient.getAuthentication("signAuth");
signAuth.setUsername("YOUR USERNAME");
signAuth.setPassword("YOUR PASSWORD");

AuthenticationApi apiInstance = new AuthenticationApi();
try {
    apiInstance.login();
} catch (ApiException e) {
    System.err.println("Exception when calling AuthenticationApi#login");
    e.printStackTrace();
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

null (empty response body)

### Authorization

[basicAuth](../README.md#basicAuth)[signAuth](../README.md#signAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

<a name="logout"></a>
# **logout**
> logout()

Revokes SRS access key

The service revokes **X-SRS-API-KEY** and deletes all non-active **runspace** resources associated with it.  Active runspaces will be deletely immediately after the completion of the scripts they run.

### Example
```java
// Import classes:
//import io.swagger.client.ApiClient;
//import io.swagger.client.ApiException;
//import io.swagger.client.Configuration;
//import io.swagger.client.auth.*;
//import io.swagger.client.api.AuthenticationApi;

ApiClient defaultClient = Configuration.getDefaultApiClient();

// Configure API key authorization: apiKeyAuth
ApiKeyAuth apiKeyAuth = (ApiKeyAuth) defaultClient.getAuthentication("apiKeyAuth");
apiKeyAuth.setApiKey("YOUR API KEY");
// Uncomment the following line to set a prefix for the API key, e.g. "Token" (defaults to null)
//apiKeyAuth.setApiKeyPrefix("Token");

AuthenticationApi apiInstance = new AuthenticationApi();
try {
    apiInstance.logout();
} catch (ApiException e) {
    System.err.println("Exception when calling AuthenticationApi#logout");
    e.printStackTrace();
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

null (empty response body)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

