param(
   [Parameter(Mandatory=$true)]
   [string]
   $DeployServer,

   [Parameter(Mandatory=$true)]
   [string]
   $DeployServerUser,

   [Parameter(Mandatory=$true)]
   [string]
   $DeployServerPassword,

   [Parameter(Mandatory=$true)]
   [string]
   $DeployTargetVMHost,

   [Parameter(Mandatory=$true)]
   [string]
   $DeployTargetDatastore,

   [Parameter(Mandatory=$true)]
   [string]
   $DeployTargetLocation,

   [Parameter(Mandatory=$true)]
   [ValidateScript({Test-Path $_})]
   [string]
   $SrsaOvf,

   [Parameter(Mandatory=$true)]
   [ValidateNotNullOrEmpty()]
   [string]
   $VMName,

   [Parameter(Mandatory=$true)]
   [string]
   $VMNetworkName,

   [Parameter(Mandatory=$true)]
   [string]
   $VMHostname,

   [Parameter(Mandatory=$true)]
   [string]
   $VMRootPassword,

   [Parameter(Mandatory=$true)]
   [string]
   $SrsVcAddress,

   [Parameter(Mandatory=$true)]
   [string]
   $SrsVcUser,

   [Parameter(Mandatory=$true)]
   [string]
   $SrsVcPassword,

   [Parameter(Mandatory=$true)]
   [string]
   $SrsVcThumbprint
)

$vcConnection = Connect-VIServer $DeployServer  -User $DeployServerUser -Password $DeployServerPassword

$ovfConfig = Get-OvfConfiguration -Ovf $SrsaOvf -Server $vcConnection

$ovfConfig.NetworkMapping.VM_Network.Value = $VMNetworkName
$ovfConfig.Common.guestinfo.hostname.Value = $VMHostname
$ovfConfig.Common.guestinfo.root_password.Value = $VMRootPassword

$ovfConfig.Common.srs.vcaddress.Value = $SrsVcAddress
$ovfConfig.Common.srs.vcuser.Value = $SrsVcUser
$ovfConfig.Common.srs.vcpassword.Value = $SrsVcPassword
$ovfConfig.Common.srs.vcthumbprint.Value = $SrsVcThumbprint

$vmhost = Get-VMHost -Server $vcConnection -Name $DeployTargetVMHost
$datastore = $vmhost | Get-Datastore -Name $DeployTargetDatastore

Import-VApp -Server $vcConnection -OvfConfiguration $ovfConfig -Source $SrsaOvf -Name $VMName -VMHost $vmhost -Datastore $datastore -Location $DeployTargetLocation | Start-VM

Disconnect-VIServer $vcConnection -Confirm:$false
