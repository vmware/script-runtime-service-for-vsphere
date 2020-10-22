#$script:apigatewayEndpoint = "http://10.23.82.131:5050"
#$script:apigatewayEndpoint = "http://localhost:5000"
param(
   $apigatewayEndpoint = "http://10.23.80.86:5050",
   $VC_SERVER = '10.23.81.114')

$Headers = @{
   "Accept" = "application/json"
   "Content-Type" = "application/json"
}


$env:PSModulePath += ";C:\temp\PowerCLIModules"
Import-Module VMware.VimAutomation.Core

function Issue-BearerSamlToken($vcServer, $username, $password) {
   $stsUri = "https://$vcServer/sts/STSService/vsphere.local";
   $sts = new-object 'VMware.Binding.Sts.VmwareSecruityTokenService' -ArgumentList $stsUri, $true, $null
   $sts.IssueBearerTokenByUserCredential($username, (ConvertTo-SecureString -String $password -AsPlainText -Force), $null, $null);
}

function Connect-Srs($samlToken) {
   $body = @{
      "saml_token" = $samlToken.RawToken.OuterXml
   }

   $connection = "" | select Headers, SamlToken
   $connection.Headers = $Headers.Clone()
   $connection.SamlToken = $samlToken.RawToken.OuterXml

   $req = Invoke-WebRequest -Uri "$($script:apigatewayEndpoint)/api/sessions" `
                            -Body ($body | ConvertTo-Json) `
                            -Headers $Headers `
                            -Method Post
   $connection.Headers['X-SRS-Authorization'] = $req.Headers['X-SRS-Authorization']

   $connection
}

function New-Runspace($connection) {
   $runspaceResponse = Invoke-RestMethod -Method POST `
                     -Uri "$($script:apigatewayEndpoint)/api/runspaces" `
                     -Headers $connection.Headers
   $runspaceResponse.Id
}

function Remove-Runspace($runspaceId, $connection) {
   Invoke-RestMethod -Method DELETE `
                     -Uri "$($script:apigatewayEndpoint)/api/runspaces/$($runspaceId)" `
                     -Headers $connection.Headers
}

function Invoke-ListRunspaces($connection) {
   $runspaces = Invoke-RestMethod -Method GET `
                     -Uri "$($script:apigatewayEndpoint)/api/runspaces" `
                     -Headers $connection.Headers
   foreach ($r in $runspaces) {
      $r.Id
   }
}

function Invoke-ListScripts($connection) {
   $scripts = Invoke-RestMethod -Method GET `
                     -Uri "$($script:apigatewayEndpoint)/api/scripts" `
                     -Headers $connection.Headers
   foreach ($script in $scripts) {
      $script | Format-Table -AutoSize
   }
}

function Invoke-RunspaceScript($runspaceId, $scriptText, $connection) {
   # Run $scriptText script
   $runScriptRequest = @{
      "runspace_id" = $runspaceId
      "script" = $scriptText
   }
   $scriptTask = Invoke-RestMethod -Method POST `
                     -Uri "$($script:apigatewayEndpoint)/api/scripts" `
                     -Body (ConvertTo-Json $runScriptRequest) `
                     -Headers $connection.Headers

   # Refresh ScriptTask
   while ($scriptTask.status -eq 'Running') {
      Start-Sleep -Seconds 1
      $scriptTask = Invoke-RestMethod -Method GET `
                     -Uri "$($script:apigatewayEndpoint)/api/scripts/$($scriptTask.id)" `
                     -Headers $connection.Headers
   }

   $scriptTask.formatted_text_result
   if ($scriptTask.data_streams.error) {
      Write-Error $scriptTask.data_streams.error
   }
}

function Invoke-VCLogin($runspaceId, $serverIp, $token, $connection) {
   # Set Ignore Certificate check in PowerCLI
   Invoke-RunspaceScript `
      $runspaceId `
      "Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:`$false" `
      $connection

   $vcLoginRequest = @{
      "runspace_id" = $runspaceId
      "saml_token" = $token
      "server" = $serverIp
      "all_linked" = $false
   }
   $scriptTask = Invoke-RestMethod -Method POST `
                     -Uri "$($script:apigatewayEndpoint)/api/scripts/powercli/vclogin" `
                     -Body (ConvertTo-Json $vcLoginRequest) `
                     -Headers $connection.Headers

   # Refresh ScriptTask
   while ($scriptTask.status -eq 'Running') {
      Start-Sleep -Seconds 1
      $scriptTask = Invoke-RestMethod -Method GET `
                     -Uri "$($script:apigatewayEndpoint)/api/scripts/$($scriptTask.id)" `
                     -Headers $connection.Headers
   }

   $scriptTask.formatted_text_result
   if ($scriptTask.data_streams.error) {
      Write-Error $scriptTask.data_streams.Error
   }
}

function Start-InteractiveSession($connection) {
   $runspaceId = New-Runspace $connection


   while ($true) {
      $script = Read-Host "pclirunspace>"
      if ($script -eq 'stop') {
         break
      }
      if ($script -eq 'vclogin') {
         Invoke-VCLogin $runspaceId $VC_SERVER $connection.SamlToken $connection
      } else {
         Invoke-RunspaceScript $runspaceId $script $connection
      }
   }
}

# ==== Example 1 ====
# - Invoke Simple PowerShell Script In Clean Runspace -

function Example1() {
   Write-Host "Example1"

   Write-Host "   Issue SamlToken from STS"
   $adminSamlToken = Issue-BearerSamlToken `
                     -vcServer $VC_SERVER `
                     -username 'administrator@vsphere.local' `
                     -password Admin!23

   Write-Host "   Login SRS"
   # Login Admin in SRS
   $adminConnection = Connect-Srs -samlToken $adminSamlToken

   Write-Host "   Create Runspace"
   # Create Runspace
   $runspaceId = New-Runspace $adminConnection

   Write-Warning "   SRS Bug workaround"
   # the below sleep is workaround of a bug in SRS
   Start-Sleep 1

   Write-Host "   Invoke Write-Output 'Dimitar Milov'"
   # Run PowerShell Script in Runspace
   Invoke-RunspaceScript `
         -runspaceId $runspaceId `
         -scriptText "Write-Output 'Dimitar Milov'" `
         -connection $adminConnection

   Write-Host "   Remove Runspace"
   # Remove Runspace
   Remove-Runspace -runspaceId $runspaceId -connection $adminConnection
}

# ==== Example 2 ====
# - Invoke Simple PowerCLI Script In Clean Runspace -

function Example2() {
   Write-Host "Example2"

   Write-Host "   Issue SamlToken from STS"
   $adminSamlToken = Issue-BearerSamlToken `
                     -vcServer $VC_SERVER `
                     -username 'administrator@vsphere.local' `
                     -password 'Admin!23'

   Write-Host "   Login SRS"
   # Login Admin in Srs
   $adminConnection = Connect-Srs -samlToken $adminSamlToken

   Write-Host "   Create Runspace"
   # Create Runspace
   $runspaceId = New-Runspace $adminConnection

   Write-Warning "   SRS Bug workaround"
   # the below sleep is workaround of a bug in SRS
   Start-Sleep 1

   Write-Host "   Invoke PowerCLI VC login"
   # Conned PowerCLI To VC
   Invoke-VCLogin `
         -runspaceId $runspaceId `
         -serverIp $VC_SERVER `
         -token $adminConnection.SamlToken `
         -connection $adminConnection

   Write-Host "   Invoke Get-Datacenter"
   # Run PowerCLI Script in Runspace
   Invoke-RunspaceScript `
         -runspaceId $runspaceId `
         -scriptText "Get-Datacenter" `
         -connection $adminConnection

   Write-Host "   Remove Runspace"
   # Remove Runspace
   Remove-Runspace -runspaceId $runspaceId -connection $adminConnection
}

# ==== Example 3 ====
# - List Runspaces and Scripts for different users -

function Example3() {
   Write-Host "Example3"

   Write-Host "   Issue Admin UserSamlToken from STS"
   $adminSamlToken = Issue-BearerSamlToken `
                     -vcServer $VC_SERVER `
                     -username 'administrator@vsphere.local' `
                     -password 'Admin!23'

   Write-Host "   Issue Readuser UserSamlToken from STS"
   $readSamlToken = Issue-BearerSamlToken `
                     -vcServer $VC_SERVER `
                     -username 'readuser@vsphere.local' `
                     -password 'appsfvt1!'

   Write-Host "   Login Admin in SRS"
   # Login Admin in Srs
   $adminConnection = Connect-Srs -samlToken $adminSamlToken

    Write-Host "   Login Read in SRS"
   # Login Read in Srs
   $readConnection = Connect-Srs -samlToken $readSamlToken

   Write-Host "   Create Runspace 1 for Admin"
   # Create Runspace
   $adminRunspace1Id = New-Runspace $adminConnection

    Write-Host "   Create Runspace 2 for Admin"
   # Create Runspace
   $adminRunspace2Id = New-Runspace $adminConnection

   Write-Host "   Create Runspace 1 for Read"
   # Create Runspace
   $readRunspace1Id = New-Runspace $readConnection

   Write-Warning "   SRS Bug workaround"
   # the below sleep is workaround of a bug in SRS
   Start-Sleep 1

   Write-Host "   Invoke Script in Admin's Runspace 1"
   # Run PowerShell Script in Runspace
   Invoke-RunspaceScript `
         -runspaceId $adminRunspace1Id `
         -scriptText "Write-Output 'Hi, this is Admin Runspace 1'" `
         -connection $adminConnection

   Write-Host "   Invoke Script in Admin's Runspace 2"
   # Run PowerShell Script in Runspace
   Invoke-RunspaceScript `
         -runspaceId $adminRunspace2Id `
         -scriptText "Write-Output 'Hi, this is Admin Runspace 2'" `
         -connection $adminConnection

   Write-Host "   Invoke Script in Read's Runspace 1"
   # Run PowerShell Script in Runspace
   Invoke-RunspaceScript `
         -runspaceId $readRunspace1Id `
         -scriptText "Write-Output 'Hi, this is Read`s Runspace'" `
         -connection $readConnection

   Write-Host "   List Admin's Runspaces"
   # Run PowerShell Script in Runspace
   Invoke-ListRunspaces `
         -connection $adminConnection

   Write-Host "   --- List Admin's Scripts ---`n"
   # Run PowerShell Script in Runspace
   Invoke-ListScripts `
         -connection $adminConnection

   Write-Host "   --- List Read's Runspaces ---`n"
   # Run PowerShell Script in Runspace
   Invoke-ListRunspaces `
         -connection $readConnection

   Write-Host "   --- List Reads's Scripts ---`n"
   # Run PowerShell Script in Runspace
   Invoke-ListScripts `
         -connection $readConnection

   Write-Host "   Remove Admin's Runspaces"
   # Remove Runspaces
   Remove-Runspace -runspaceId $adminRunspace1Id -connection $adminConnection
   Remove-Runspace -runspaceId $adminRunspace2Id -connection $adminConnection

   Write-Host "   Remove Reads's Runspace"
   # Remove Runspaces
   Remove-Runspace -runspaceId $readRunspace1Id -connection $readConnection
}