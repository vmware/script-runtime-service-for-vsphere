#!/bin/bash

. /root/config.sh

encodedauthinfo=$(echo -n $username:$password | base64)

#login to SRS
srsapikey=$(curl -X POST "https://${ses_address}/api/auth/login" -H "accept: \*/\*" \
			-H "Authorization: Basic ${encodedauthinfo}" -H "content-length: 0" -i -k \
			| grep X-SRS-API-KEY: | awk {'print $2'})
			 
#create runspace
runspaceInfo=$(curl -X POST "https://${ses_address}/api/runspaces" --http1.1 \
		     -H "accept: application/json" -H "X-SRS-API-KEY:${srsapikey}" \
			 -H "Content-Type: application/json" \
			 -d "{\"name\":\"MyRunspace\",\"run_vc_connection_script\":\"true\"}" -k)

#extract the runspace ID				
runspaceId=$(echo $runspaceInfo | pcregrep -o '"id":.*?[^\\]"' |awk -F':' '{print $2}' | tr --delete \")

#wait for the runspace to become ready
state=""
while [ "$state" != "ready" ]
do
   sleep 3
   response=$(curl -X GET "https://${ses_address}/api/runspaces/${runspaceId}" \
            -H "accept: application/json" -H "X-SRS-API-KEY:${srsapikey}" -k)
   state=$(echo $response | pcregrep -o '"state":.*?[^\\]"' |awk -F':' '{print $2}' | tr --delete \")   
done

#read the PowerCLI script
scriptText=$(</root/$1)

#Initiate script execution
curl -X POST "https://${ses_address}/api/script-executions" --http1.1 \
     -H "accept: application/json" -H "X-SRS-API-KEY:${srsapikey}" -H "Content-Type: application/json" \
	 -d "{\"runspace_id\":\"${runspaceId}\",\"name\":\"My script exectution\",\"script\":\"${scriptText}\"}" -k