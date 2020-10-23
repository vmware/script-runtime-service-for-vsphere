// **************************************************************************
//  Copyright 2020 VMware, Inc.
//  SPDX-License-Identifier: Apache-2.0
// **************************************************************************
package com.vmware.srs.client;

import java.util.List;

import io.swagger.client.ApiClient;
import io.swagger.client.ApiResponse;
import io.swagger.client.api.AuthenticationApi;
import io.swagger.client.api.RunspacesApi;
import io.swagger.client.api.ScriptexecutionsApi;
import io.swagger.client.model.Runspace;
import io.swagger.client.model.RunspaceState;
import io.swagger.client.model.ScriptExecution;
import io.swagger.client.model.ScriptExecutionState;
import io.swagger.client.model.StreamRecord;
import io.swagger.client.model.StreamType;

public class App 
{
	private static String _usage = "Usage: java -jar client-1.0.jar <ses address> <username> <password> <PowerCLI script>\n"
			+ "Example: java -jar client-1.0.jar https://10.23.82.159 administrator@vsphere.local Admin!23 Get-Folder";
    public static void main( String[] args )
    {
    	if (args == null || args.length != 4) {
    		System.out.println( _usage );
    		System.exit(2);
    	}
    	
    	String srsAddress = args[0];
    	String username = args[1];
    	String password = args[2];
    	String scriptText = args[3];

        if ( srsAddress == "" ||
             username == "" ||
             password == "" ||
             scriptText == "" ) {
        	System.out.println( _usage );
    		System.exit(2);
        }
        
        try {
        	
        	// Login with username and password
            ApiClient apiClient = new ApiClient();
            apiClient.setBasePath(srsAddress);
            apiClient.setUsername(username);
            apiClient.setPassword(password);
            apiClient.setVerifyingSsl(false);
            
            AuthenticationApi authApi = new AuthenticationApi(apiClient);
            ApiResponse<Void> loginResponse = authApi.loginWithHttpInfo();
            String sesApiKey = loginResponse.getHeaders().get("X-SRS-API-KEY").get(0);

            apiClient = new ApiClient();
            apiClient.setBasePath(srsAddress);
            apiClient.setApiKey(sesApiKey);
            apiClient.setVerifyingSsl(false);
            
            // Create Runspace
            RunspacesApi runspaceApi = new RunspacesApi(apiClient);
            Runspace runspaceRequest = new Runspace();
            runspaceRequest.setName("MyPSRunspace");
            runspaceRequest.setRunVcConnectionScript(true);
            Runspace runspace = runspaceApi.createRunspace(runspaceRequest);

            while (runspace.getState() == RunspaceState.CREATING) {
               Thread.sleep(500);
               runspace = runspaceApi.getRunspace(runspace.getId());
            }

            if (runspace.getState() == RunspaceState.ERROR) {
            	System.out.println(String.format("Error on runspace creation: %s", runspace.getErrorDetails().getDetails()));
            	System.exit(3);
            }
            
            // Run Script
            ScriptexecutionsApi scriptExecutionsApi = new ScriptexecutionsApi(apiClient);
            
            ScriptExecution scriptExecutionRequest = new ScriptExecution();
            scriptExecutionRequest.setRunspaceId(runspace.getId());
            scriptExecutionRequest.setName("MyScript");
            scriptExecutionRequest.setScript(scriptText);
            
            ScriptExecution scriptExecution = scriptExecutionsApi.createScriptExecution(
            		scriptExecutionRequest);
               

            while (scriptExecution.getState() == ScriptExecutionState.RUNNING) {
               Thread.sleep(500);
               scriptExecution = scriptExecutionsApi.getScriptExecution(scriptExecution.getId());
            }

            if (scriptExecution.getState() == ScriptExecutionState.ERROR) {
            	System.out.println(String.format("Error on script execution: %s", scriptExecution.getReason()));               
            	System.exit(4);
            }

            // Read Script Output
            List<String> scriptOutput = scriptExecutionsApi.getScriptExecutionOutput(scriptExecution.getId());
            if (scriptOutput != null && !scriptOutput.isEmpty()) {
            	System.out.println("Script Output:");
               for (String output : scriptOutput) {
            	   System.out.println(output);
               }
            }

            // Read Script Errors   
            List<StreamRecord> scriptErrorRecords = scriptExecutionsApi.getScriptExecutionStream(scriptExecution.getId(), StreamType.ERROR);
            if (scriptErrorRecords != null && !scriptErrorRecords.isEmpty()) {
            	System.out.println("Script Error:");
               for (StreamRecord errorRecord : scriptErrorRecords) {
            	   System.out.println(errorRecord.getMessage());
               }
            }
            
            // Delete Runspace
            runspaceApi.deleteRunspace(runspace.getId());
        
        } catch (Exception exc) {        
        	System.out.println(String.format("Error: %s", exc));
            System.exit(100);
        }
    }
}
