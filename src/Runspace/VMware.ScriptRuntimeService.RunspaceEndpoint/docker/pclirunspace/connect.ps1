Import-Module VMware.VimAutomation.Security
Import-Module VMware.VimAutomation.Cis.Core

$rawSamlToken = [System.Text.Encoding]::Unicode.GetString([System.Convert]::FromBase64String($env:token))
$samlTokenSecurityContext = New-Object `
	'VMware.VimAutomation.Common.Util10.Authentication.RawStringSamlSecurityContext' `
	-ArgumentList $rawSamlToken

Connect-VIServer -Server $env:vc -SamlSecurityContext $samlTokenSecurityContext -AllLinked:$([bool]::Parse($env:allLinked)) -ErrorAction SilentlyContinue
