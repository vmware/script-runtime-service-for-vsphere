/*
 * Script Runtime Service for vSphere
 * # Script Runtime Service API    Script Runtime Service for vSphere (SRS) allows running PowerShell and PowerCLI scripts. SRS is a VC add-on that is deployed separately from VCSA. SRS can be accessed via REST API that allows you to create PowerShell instances and run PowerShell and PowerCLI scripts within. No Connect-VIServer is required to run PowerCLI against VC(s) SRS is registered to.    ## Authetication    SRS uses VC SSO as Identity and Authentication Server. Two types of authentication are supported. SIGN and Basic. SIGN authentication is purposed for Service-To-Service access to SRS resources. For convenience of the end-users SRS supports basic authentication passing username and password which are used to acquire SAML HoK token for SRS solution. When basic is used SRS exchanges the username and password for SAML HoK token from the SSO server. SRS uses the SAML token to Connect PowerCLI to VC services in further operations.   On successful authentication SRS returns API Key which is required to authorize further SRS API calls.
 *
 * OpenAPI spec version: 1.0-oas3
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */

package io.swagger.client.api;

import io.swagger.client.ApiCallback;
import io.swagger.client.ApiClient;
import io.swagger.client.ApiException;
import io.swagger.client.ApiResponse;
import io.swagger.client.Configuration;
import io.swagger.client.Pair;
import io.swagger.client.ProgressRequestBody;
import io.swagger.client.ProgressResponseBody;

import com.google.gson.reflect.TypeToken;

import java.io.IOException;


import io.swagger.client.model.ArgumentScript;
import io.swagger.client.model.ArgumentScriptTemplate;
import io.swagger.client.model.ErrorDetails;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class ArgumentscriptsApi {
    private ApiClient apiClient;

    public ArgumentscriptsApi() {
        this(Configuration.getDefaultApiClient());
    }

    public ArgumentscriptsApi(ApiClient apiClient) {
        this.apiClient = apiClient;
    }

    public ApiClient getApiClient() {
        return apiClient;
    }

    public void setApiClient(ApiClient apiClient) {
        this.apiClient = apiClient;
    }

    /**
     * Build call for createArgumentScriptsScript
     * @param body The argument script create request (optional)
     * @param progressListener Progress listener
     * @param progressRequestListener Progress request listener
     * @return Call to execute
     * @throws ApiException If fail to serialize the request body object
     */
    public com.squareup.okhttp.Call createArgumentScriptsScriptCall(ArgumentScript body, final ProgressResponseBody.ProgressListener progressListener, final ProgressRequestBody.ProgressRequestListener progressRequestListener) throws ApiException {
        Object localVarPostBody = body;
        
        // create path and map variables
        String localVarPath = "/api/argument-scripts/script";

        List<Pair> localVarQueryParams = new ArrayList<Pair>();
        List<Pair> localVarCollectionQueryParams = new ArrayList<Pair>();

        Map<String, String> localVarHeaderParams = new HashMap<String, String>();

        Map<String, Object> localVarFormParams = new HashMap<String, Object>();

        final String[] localVarAccepts = {
            "application/json"
        };
        final String localVarAccept = apiClient.selectHeaderAccept(localVarAccepts);
        if (localVarAccept != null) localVarHeaderParams.put("Accept", localVarAccept);

        final String[] localVarContentTypes = {
            "application/json"
        };
        final String localVarContentType = apiClient.selectHeaderContentType(localVarContentTypes);
        localVarHeaderParams.put("Content-Type", localVarContentType);

        if(progressListener != null) {
            apiClient.getHttpClient().networkInterceptors().add(new com.squareup.okhttp.Interceptor() {
                @Override
                public com.squareup.okhttp.Response intercept(com.squareup.okhttp.Interceptor.Chain chain) throws IOException {
                    com.squareup.okhttp.Response originalResponse = chain.proceed(chain.request());
                    return originalResponse.newBuilder()
                    .body(new ProgressResponseBody(originalResponse.body(), progressListener))
                    .build();
                }
            });
        }

        String[] localVarAuthNames = new String[] { "apiKeyAuth" };
        return apiClient.buildCall(localVarPath, "POST", localVarQueryParams, localVarCollectionQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarAuthNames, progressRequestListener);
    }
    
    @SuppressWarnings("rawtypes")
    private com.squareup.okhttp.Call createArgumentScriptsScriptValidateBeforeCall(ArgumentScript body, final ProgressResponseBody.ProgressListener progressListener, final ProgressRequestBody.ProgressRequestListener progressRequestListener) throws ApiException {
        
        com.squareup.okhttp.Call call = createArgumentScriptsScriptCall(body, progressListener, progressRequestListener);
        return call;

        
        
        
        
    }

    /**
     * Creates scripts for a given script template id and placeholder values
     * ### Creates scripts for a given script template id and placeholder values  Replaces the placeholders in a given argument transformation script template with given values on the placeholder_value_list field  The result script can be provided to a **script execution** parameter that expects specific script runtime type    ### Example  If the template argument transformation script is    Get-VM -Id &lt;vm-id&gt; -Server &lt;server&gt;    The result of this operation with given Id &#x27;vm-1&#x27; and Server &#x27;server-1&#x27; would be    Get-VM -Id &#x27;vm-1&#x27; -Server &#x27;server-1&#x27;
     * @param body The argument script create request (optional)
     * @return ArgumentScript
     * @throws ApiException If fail to call the API, e.g. server error or cannot deserialize the response body
     */
    public ArgumentScript createArgumentScriptsScript(ArgumentScript body) throws ApiException {
        ApiResponse<ArgumentScript> resp = createArgumentScriptsScriptWithHttpInfo(body);
        return resp.getData();
    }

    /**
     * Creates scripts for a given script template id and placeholder values
     * ### Creates scripts for a given script template id and placeholder values  Replaces the placeholders in a given argument transformation script template with given values on the placeholder_value_list field  The result script can be provided to a **script execution** parameter that expects specific script runtime type    ### Example  If the template argument transformation script is    Get-VM -Id &lt;vm-id&gt; -Server &lt;server&gt;    The result of this operation with given Id &#x27;vm-1&#x27; and Server &#x27;server-1&#x27; would be    Get-VM -Id &#x27;vm-1&#x27; -Server &#x27;server-1&#x27;
     * @param body The argument script create request (optional)
     * @return ApiResponse&lt;ArgumentScript&gt;
     * @throws ApiException If fail to call the API, e.g. server error or cannot deserialize the response body
     */
    public ApiResponse<ArgumentScript> createArgumentScriptsScriptWithHttpInfo(ArgumentScript body) throws ApiException {
        com.squareup.okhttp.Call call = createArgumentScriptsScriptValidateBeforeCall(body, null, null);
        Type localVarReturnType = new TypeToken<ArgumentScript>(){}.getType();
        return apiClient.execute(call, localVarReturnType);
    }

    /**
     * Creates scripts for a given script template id and placeholder values (asynchronously)
     * ### Creates scripts for a given script template id and placeholder values  Replaces the placeholders in a given argument transformation script template with given values on the placeholder_value_list field  The result script can be provided to a **script execution** parameter that expects specific script runtime type    ### Example  If the template argument transformation script is    Get-VM -Id &lt;vm-id&gt; -Server &lt;server&gt;    The result of this operation with given Id &#x27;vm-1&#x27; and Server &#x27;server-1&#x27; would be    Get-VM -Id &#x27;vm-1&#x27; -Server &#x27;server-1&#x27;
     * @param body The argument script create request (optional)
     * @param callback The callback to be executed when the API call finishes
     * @return The request call
     * @throws ApiException If fail to process the API call, e.g. serializing the request body object
     */
    public com.squareup.okhttp.Call createArgumentScriptsScriptAsync(ArgumentScript body, final ApiCallback<ArgumentScript> callback) throws ApiException {

        ProgressResponseBody.ProgressListener progressListener = null;
        ProgressRequestBody.ProgressRequestListener progressRequestListener = null;

        if (callback != null) {
            progressListener = new ProgressResponseBody.ProgressListener() {
                @Override
                public void update(long bytesRead, long contentLength, boolean done) {
                    callback.onDownloadProgress(bytesRead, contentLength, done);
                }
            };

            progressRequestListener = new ProgressRequestBody.ProgressRequestListener() {
                @Override
                public void onRequestProgress(long bytesWritten, long contentLength, boolean done) {
                    callback.onUploadProgress(bytesWritten, contentLength, done);
                }
            };
        }

        com.squareup.okhttp.Call call = createArgumentScriptsScriptValidateBeforeCall(body, progressListener, progressRequestListener);
        Type localVarReturnType = new TypeToken<ArgumentScript>(){}.getType();
        apiClient.executeAsync(call, localVarReturnType, callback);
        return call;
    }
    /**
     * Build call for getArgumentScriptsTemplate
     * @param id The Id of the argument script template (required)
     * @param progressListener Progress listener
     * @param progressRequestListener Progress request listener
     * @return Call to execute
     * @throws ApiException If fail to serialize the request body object
     */
    public com.squareup.okhttp.Call getArgumentScriptsTemplateCall(String id, final ProgressResponseBody.ProgressListener progressListener, final ProgressRequestBody.ProgressRequestListener progressRequestListener) throws ApiException {
        Object localVarPostBody = null;
        
        // create path and map variables
        String localVarPath = "/api/argument-scripts/templates/{id}"
            .replaceAll("\\{" + "id" + "\\}", apiClient.escapeString(id.toString()));

        List<Pair> localVarQueryParams = new ArrayList<Pair>();
        List<Pair> localVarCollectionQueryParams = new ArrayList<Pair>();

        Map<String, String> localVarHeaderParams = new HashMap<String, String>();

        Map<String, Object> localVarFormParams = new HashMap<String, Object>();

        final String[] localVarAccepts = {
            "application/json"
        };
        final String localVarAccept = apiClient.selectHeaderAccept(localVarAccepts);
        if (localVarAccept != null) localVarHeaderParams.put("Accept", localVarAccept);

        final String[] localVarContentTypes = {
            
        };
        final String localVarContentType = apiClient.selectHeaderContentType(localVarContentTypes);
        localVarHeaderParams.put("Content-Type", localVarContentType);

        if(progressListener != null) {
            apiClient.getHttpClient().networkInterceptors().add(new com.squareup.okhttp.Interceptor() {
                @Override
                public com.squareup.okhttp.Response intercept(com.squareup.okhttp.Interceptor.Chain chain) throws IOException {
                    com.squareup.okhttp.Response originalResponse = chain.proceed(chain.request());
                    return originalResponse.newBuilder()
                    .body(new ProgressResponseBody(originalResponse.body(), progressListener))
                    .build();
                }
            });
        }

        String[] localVarAuthNames = new String[] { "apiKeyAuth" };
        return apiClient.buildCall(localVarPath, "GET", localVarQueryParams, localVarCollectionQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarAuthNames, progressRequestListener);
    }
    
    @SuppressWarnings("rawtypes")
    private com.squareup.okhttp.Call getArgumentScriptsTemplateValidateBeforeCall(String id, final ProgressResponseBody.ProgressListener progressListener, final ProgressRequestBody.ProgressRequestListener progressRequestListener) throws ApiException {
        // verify the required parameter 'id' is set
        if (id == null) {
            throw new ApiException("Missing the required parameter 'id' when calling getArgumentScriptsTemplate(Async)");
        }
        
        com.squareup.okhttp.Call call = getArgumentScriptsTemplateCall(id, progressListener, progressRequestListener);
        return call;

        
        
        
        
    }

    /**
     * Retrieves argument script template by given unique template identifier
     * ### Retrieves argument script template by given unique template identifier  This operation returns argument script template for the specified template id.
     * @param id The Id of the argument script template (required)
     * @return ArgumentScriptTemplate
     * @throws ApiException If fail to call the API, e.g. server error or cannot deserialize the response body
     */
    public ArgumentScriptTemplate getArgumentScriptsTemplate(String id) throws ApiException {
        ApiResponse<ArgumentScriptTemplate> resp = getArgumentScriptsTemplateWithHttpInfo(id);
        return resp.getData();
    }

    /**
     * Retrieves argument script template by given unique template identifier
     * ### Retrieves argument script template by given unique template identifier  This operation returns argument script template for the specified template id.
     * @param id The Id of the argument script template (required)
     * @return ApiResponse&lt;ArgumentScriptTemplate&gt;
     * @throws ApiException If fail to call the API, e.g. server error or cannot deserialize the response body
     */
    public ApiResponse<ArgumentScriptTemplate> getArgumentScriptsTemplateWithHttpInfo(String id) throws ApiException {
        com.squareup.okhttp.Call call = getArgumentScriptsTemplateValidateBeforeCall(id, null, null);
        Type localVarReturnType = new TypeToken<ArgumentScriptTemplate>(){}.getType();
        return apiClient.execute(call, localVarReturnType);
    }

    /**
     * Retrieves argument script template by given unique template identifier (asynchronously)
     * ### Retrieves argument script template by given unique template identifier  This operation returns argument script template for the specified template id.
     * @param id The Id of the argument script template (required)
     * @param callback The callback to be executed when the API call finishes
     * @return The request call
     * @throws ApiException If fail to process the API call, e.g. serializing the request body object
     */
    public com.squareup.okhttp.Call getArgumentScriptsTemplateAsync(String id, final ApiCallback<ArgumentScriptTemplate> callback) throws ApiException {

        ProgressResponseBody.ProgressListener progressListener = null;
        ProgressRequestBody.ProgressRequestListener progressRequestListener = null;

        if (callback != null) {
            progressListener = new ProgressResponseBody.ProgressListener() {
                @Override
                public void update(long bytesRead, long contentLength, boolean done) {
                    callback.onDownloadProgress(bytesRead, contentLength, done);
                }
            };

            progressRequestListener = new ProgressRequestBody.ProgressRequestListener() {
                @Override
                public void onRequestProgress(long bytesWritten, long contentLength, boolean done) {
                    callback.onUploadProgress(bytesWritten, contentLength, done);
                }
            };
        }

        com.squareup.okhttp.Call call = getArgumentScriptsTemplateValidateBeforeCall(id, progressListener, progressRequestListener);
        Type localVarReturnType = new TypeToken<ArgumentScriptTemplate>(){}.getType();
        apiClient.executeAsync(call, localVarReturnType, callback);
        return call;
    }
    /**
     * Build call for listArgumentScriptsTemplates
     * @param progressListener Progress listener
     * @param progressRequestListener Progress request listener
     * @return Call to execute
     * @throws ApiException If fail to serialize the request body object
     */
    public com.squareup.okhttp.Call listArgumentScriptsTemplatesCall(final ProgressResponseBody.ProgressListener progressListener, final ProgressRequestBody.ProgressRequestListener progressRequestListener) throws ApiException {
        Object localVarPostBody = null;
        
        // create path and map variables
        String localVarPath = "/api/argument-scripts/templates";

        List<Pair> localVarQueryParams = new ArrayList<Pair>();
        List<Pair> localVarCollectionQueryParams = new ArrayList<Pair>();

        Map<String, String> localVarHeaderParams = new HashMap<String, String>();

        Map<String, Object> localVarFormParams = new HashMap<String, Object>();

        final String[] localVarAccepts = {
            "application/json"
        };
        final String localVarAccept = apiClient.selectHeaderAccept(localVarAccepts);
        if (localVarAccept != null) localVarHeaderParams.put("Accept", localVarAccept);

        final String[] localVarContentTypes = {
            
        };
        final String localVarContentType = apiClient.selectHeaderContentType(localVarContentTypes);
        localVarHeaderParams.put("Content-Type", localVarContentType);

        if(progressListener != null) {
            apiClient.getHttpClient().networkInterceptors().add(new com.squareup.okhttp.Interceptor() {
                @Override
                public com.squareup.okhttp.Response intercept(com.squareup.okhttp.Interceptor.Chain chain) throws IOException {
                    com.squareup.okhttp.Response originalResponse = chain.proceed(chain.request());
                    return originalResponse.newBuilder()
                    .body(new ProgressResponseBody(originalResponse.body(), progressListener))
                    .build();
                }
            });
        }

        String[] localVarAuthNames = new String[] { "apiKeyAuth" };
        return apiClient.buildCall(localVarPath, "GET", localVarQueryParams, localVarCollectionQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarAuthNames, progressRequestListener);
    }
    
    @SuppressWarnings("rawtypes")
    private com.squareup.okhttp.Call listArgumentScriptsTemplatesValidateBeforeCall(final ProgressResponseBody.ProgressListener progressListener, final ProgressRequestBody.ProgressRequestListener progressRequestListener) throws ApiException {
        
        com.squareup.okhttp.Call call = listArgumentScriptsTemplatesCall(progressListener, progressRequestListener);
        return call;

        
        
        
        
    }

    /**
     * List available argument script templates
     * ### LList available argument script templates  Argument script templates are scripts with placeholders. When placeholders are replaced by values script can be executed in a given script runtime.  Argument script templates are designed to help to convert simple type values to objects of types that can only be produced in a given script runtime. Those object can be used as arguments to scripts&#x27; parameters.    This operation retrieves the available argument script templates.
     * @return List&lt;ArgumentScriptTemplate&gt;
     * @throws ApiException If fail to call the API, e.g. server error or cannot deserialize the response body
     */
    public List<ArgumentScriptTemplate> listArgumentScriptsTemplates() throws ApiException {
        ApiResponse<List<ArgumentScriptTemplate>> resp = listArgumentScriptsTemplatesWithHttpInfo();
        return resp.getData();
    }

    /**
     * List available argument script templates
     * ### LList available argument script templates  Argument script templates are scripts with placeholders. When placeholders are replaced by values script can be executed in a given script runtime.  Argument script templates are designed to help to convert simple type values to objects of types that can only be produced in a given script runtime. Those object can be used as arguments to scripts&#x27; parameters.    This operation retrieves the available argument script templates.
     * @return ApiResponse&lt;List&lt;ArgumentScriptTemplate&gt;&gt;
     * @throws ApiException If fail to call the API, e.g. server error or cannot deserialize the response body
     */
    public ApiResponse<List<ArgumentScriptTemplate>> listArgumentScriptsTemplatesWithHttpInfo() throws ApiException {
        com.squareup.okhttp.Call call = listArgumentScriptsTemplatesValidateBeforeCall(null, null);
        Type localVarReturnType = new TypeToken<List<ArgumentScriptTemplate>>(){}.getType();
        return apiClient.execute(call, localVarReturnType);
    }

    /**
     * List available argument script templates (asynchronously)
     * ### LList available argument script templates  Argument script templates are scripts with placeholders. When placeholders are replaced by values script can be executed in a given script runtime.  Argument script templates are designed to help to convert simple type values to objects of types that can only be produced in a given script runtime. Those object can be used as arguments to scripts&#x27; parameters.    This operation retrieves the available argument script templates.
     * @param callback The callback to be executed when the API call finishes
     * @return The request call
     * @throws ApiException If fail to process the API call, e.g. serializing the request body object
     */
    public com.squareup.okhttp.Call listArgumentScriptsTemplatesAsync(final ApiCallback<List<ArgumentScriptTemplate>> callback) throws ApiException {

        ProgressResponseBody.ProgressListener progressListener = null;
        ProgressRequestBody.ProgressRequestListener progressRequestListener = null;

        if (callback != null) {
            progressListener = new ProgressResponseBody.ProgressListener() {
                @Override
                public void update(long bytesRead, long contentLength, boolean done) {
                    callback.onDownloadProgress(bytesRead, contentLength, done);
                }
            };

            progressRequestListener = new ProgressRequestBody.ProgressRequestListener() {
                @Override
                public void onRequestProgress(long bytesWritten, long contentLength, boolean done) {
                    callback.onUploadProgress(bytesWritten, contentLength, done);
                }
            };
        }

        com.squareup.okhttp.Call call = listArgumentScriptsTemplatesValidateBeforeCall(progressListener, progressRequestListener);
        Type localVarReturnType = new TypeToken<List<ArgumentScriptTemplate>>(){}.getType();
        apiClient.executeAsync(call, localVarReturnType, callback);
        return call;
    }
}
