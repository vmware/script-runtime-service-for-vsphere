## VCVA Client That Initiates PowerShell Script Execution

The client contains two files:  
`config.sh` – This is the file where you define the SRS endpoint address, the vCenter username and password  
`ExecutePowerCLIScript.sh` – this is the BASH script that makes the actual REST API calls to the SRS to initiate the script execution. 

To use the client to run a PowerShell action on vSphere alarm follow these steps:
1. Copy the two files together with PowerShell scripts that you want to execute in the `/root` folder ot the VCVA
2. Set the script to an executable by running the following command on the VCVA:
	`chmod +x ExecutePowerCLIScript.sh`
3. Create an alarm in the vCenter and as an action of the alarm specify that it should run a script. Under "Run this" specify: `/root/ExecutePowerCLIScript.sh <name of the script>`, for example `/root/ExecutePowerCLIScript.sh TurnOffNonEssentialVMs.ps1`