param([string]$server, 
	  [string] $samlToken, 
	  [bool] $allLinked)

Import-Module VMware.VimAutomation.Security
Import-Module VMware.VimAutomation.Cis.Core

$samlTokenSecurityContext = New-Object `
	'VMware.VimAutomation.Common.Util10.Authentication.RawStringSamlSecurityContext' `
	-ArgumentList $samlToken

Connect-VIServer -Server $server -SamlSecurityContext $samlTokenSecurityContext -AllLinked:$allLinked