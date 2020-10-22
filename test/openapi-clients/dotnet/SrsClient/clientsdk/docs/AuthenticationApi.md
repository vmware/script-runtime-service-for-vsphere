# IO.Swagger.Api.AuthenticationApi

All URIs are relative to */*

Method | HTTP request | Description
------------- | ------------- | -------------
[**Login**](AuthenticationApi.md#login) | **POST** /api/auth/login | Exchanges client credentials or SIGN token for SRS access key
[**Logout**](AuthenticationApi.md#logout) | **POST** /api/auth/logout | Revokes SRS access key

<a name="login"></a>
# **Login**
> void Login ()

Exchanges client credentials or SIGN token for SRS access key

Uses VCenter SSO as Identity and Authentication Server.  Two types of authentication are supported SIGN and Basic.  When Basic authentication is used service exchanges username and password for SAML from VCenter SSO.  When SIGN authentication is used service exchanges the SSO SAML token from the SIGN token for another SAML token on behalf of the user from VCenter SSO.  On successful authentication with SSO the service issues **X-SRS-API-KEY** token are returns it the response headers. **X-SRS-API-KEY** token is used to authorize access to service resources.   The service associates **X-SRS-API-KEY** token to acquired from SSO SAML token. The associated SSO SAML token is used to authorize PowerCLI to VCenter services.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class LoginExample
    {
        public void main()
        {
            // Configure HTTP basic authorization: basicAuth
            Configuration.Default.Username = "YOUR_USERNAME";
            Configuration.Default.Password = "YOUR_PASSWORD";
            // Configure HTTP basic authorization: signAuth
            Configuration.Default.Username = "YOUR_USERNAME";
            Configuration.Default.Password = "YOUR_PASSWORD";

            var apiInstance = new AuthenticationApi();

            try
            {
                // Exchanges client credentials or SIGN token for SRS access key
                apiInstance.Login();
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling AuthenticationApi.Login: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

void (empty response body)

### Authorization

[basicAuth](../README.md#basicAuth), [signAuth](../README.md#signAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
<a name="logout"></a>
# **Logout**
> void Logout ()

Revokes SRS access key

The service revokes **X-SRS-API-KEY** and deletes all non-active **runspace** resources associated with it.  Active runspaces will be deletely immediately after the completion of the scripts they run.

### Example
```csharp
using System;
using System.Diagnostics;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace Example
{
    public class LogoutExample
    {
        public void main()
        {
            // Configure API key authorization: apiKeyAuth
            Configuration.Default.AddApiKey("X-SRS-API-KEY", "YOUR_API_KEY");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("X-SRS-API-KEY", "Bearer");

            var apiInstance = new AuthenticationApi();

            try
            {
                // Revokes SRS access key
                apiInstance.Logout();
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling AuthenticationApi.Logout: " + e.Message );
            }
        }
    }
}
```

### Parameters
This endpoint does not need any parameter.

### Return type

void (empty response body)

### Authorization

[apiKeyAuth](../README.md#apiKeyAuth)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)
