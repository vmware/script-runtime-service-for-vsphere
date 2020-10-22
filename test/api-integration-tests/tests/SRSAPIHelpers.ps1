# **************************************************************************
#  Copyright 2020 VMware, Inc.
#  SPDX-License-Identifier: Apache-2.0
# **************************************************************************

param(
   [Parameter(Mandatory = $true)]
   [string]
   $stsLibraryPath,

   [Parameter()]
   [string]
   $lsLibraryPath,

   [Parameter()]
   [string]
   $ssoAdminLibraryPath,

   [Parameter()]
   [string]
   $ssoAuthenticationLibraryPath)

Import-Module $stsLibraryPath
Import-Module $lsLibraryPath
Import-Module $ssoAdminLibraryPath
Import-Module $ssoAuthenticationLibraryPath
Import-Module (Join-Path $PSScriptRoot (Join-Path 'helpers' (Join-Path 'CertificateValidators' (Join-Path 'bin' 'CertificateValidators.dll'))))

function Get-SRSPort {
   443
}

function Get-SRSProtocol {
   "https"
}

function Connect-SrsWithSignAuthorization($SRSIPAddress, $vcServer, $username, $password) {
   $stsClient = New-Object `
      'VMware.ScriptRuntimeService.Sts.STSClient' `
      -ArgumentList (Get-StsServiceFromLS $vcServer), (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

   $sesSolution = Get-SrsServiceFromLS -vcServer $vcServer

   $clientSigningCertificate = Get-ClientSignCertificate

   # Issue HoK token by user and pass with client certificate
   $clientHoK = $stsClient.IssueHoKTokenByUserCredential(
         $username,
         (ConvertTo-SecureString -String $password -Force -AsPlainText),
         $clientSigningCertificate);

   # Issue Delegate to SES Solution HoK token
   $delegateToSESToken = $stsClient.IssueDelegateToHoKTokenBySolutionHoK(
      $clientHoK,
      $clientSigningCertificate,
      $sesSolution.ServiceId,
      $sesSolution.OwnerId);

   $body = @{"saml_token" = $delegateToSESToken.OuterXml } | `
      ConvertTo-Json

   $loginPath = "/api/auth/login"

   $signAuthReq = [VMware.Http.Sso.Authentication.RequestFactory]::Create("POST", $loginPath, $SRSIPAddress, $(Get-SRSPort), $body)

   $authzHeaderValues = [VMware.Http.Sso.Authentication.AuthCalculatorFactory]::Create().ComputeToken(
      $signAuthReq,
      $clientSigningCertificate,
      [VMware.Http.Sso.Authentication.SigningAlgorithm]::RSA_SHA256,
      $clientHoK.OuterXml)

   $Headers = @{
      "Accept" = "application/json"
      "Content-Type" = "application/json"
      "Authorization" = [string]::Join(",", $authzHeaderValues)
   }

   $connection = "" | select APIBasePath, Headers, SamlToken
   $connection.APIBasePath = "$(Get-SRSProtocol)://$($SRSIPAddress):$(Get-SRSPort)/api"
   $connection.Headers =  @{
      "Accept" = "application/json"
      "Content-Type" = "application/json"
   }

   $req = Invoke-WebRequest -Uri "$($connection.APIBasePath)/auth/login" `
                            -Body $body  `
                            -Headers $Headers `
                            -Method POST `
                            -SkipCertificateCheck

   $connection.Headers['X-SRS-API-KEY'] = $req.Headers['X-SRS-API-KEY'] | Select-Object -First 1

   $connection
}

function Wait-ScriptTask($connection, $scriptTask) {
   while ($scriptTask.state -eq 'running') {
      Start-Sleep -Seconds 1
      $scriptTask = Invoke-RestMethod -Method GET `
                     -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.id)" `
                     -Headers $connection.Headers `
                     -SkipCertificateCheck
   }
   $scriptTask
}

function Wait-Runspace($connection, $runspace) {
   while ($runspace.state -ne 'ready' -and $runspace.state -ne 'error') {
      Start-Sleep -Seconds 1
      $runspace = Invoke-RestMethod -Method GET `
                     -Uri "$($connection.APIBasePath)/runspaces/$($runspace.id)" `
                     -Headers $connection.Headers `
                     -SkipCertificateCheck
   }

   if ($runspace.state -eq 'error') {
      throw "Runspace preparation failed with error: $($runspace.error_details)"
   }

   $runspace
}

function Wait-RunspaceScriptTask($connection, $runspaceId, $scriptTask) {
   while ($scriptTask.state -eq 'running') {
      Start-Sleep -Seconds 1
      $scriptTask = Invoke-RestMethod -Method GET `
                     -Uri "$($connection.APIBasePath)/runspaces/$($runspaceId)/script/$($scriptTask.id)" `
                     -Headers $connection.Headers `
                     -SkipCertificateCheck
   }
   $scriptTask
}

function Get-Certificate {
param($pfxName)
   $pfx = Join-Path (Join-Path $PSScriptRoot "certs") $pfxName
   $pwd = "test_cert"

   New-Object "System.Security.Cryptography.X509Certificates.X509Certificate2" -ArgumentList $pfx, $pwd
}

function Get-ClientSignCertificate {
   Get-Certificate "client_sign.pfx"
}

function Get-TestSolutionSignCertificate {
   Get-Certificate "test_solution_sign.pfx"
}

function Get-TestSolutionTlsCertificate {
   Get-Certificate "test_solution_tls.pfx"
}


function Get-SrsServiceFromLS {
param($vcServer)
   $lsClient = New-Object `
      'VMware.ScriptRuntimeService.Ls.LookupServiceClient' `
      -ArgumentList $vcServer, (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

   $services = $lsClient.ListRegisteredServices()
   $services | Where-Object {$_.serviceDescriptionResourceKey -eq 'srs.ServiceDescritpion'}
}

function Get-StsServiceFromLS {
param($vcServer)
   $lsClient = New-Object `
      'VMware.ScriptRuntimeService.Ls.LookupServiceClient' `
      -ArgumentList $vcServer, (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

   $lsClient.GetStsEndpointUri()
}

function Register-VCSASolution {
param(
   [Parameter(Mandatory = $true)]
   [string]
   $VCIPAddress,

   [Parameter(Mandatory = $true)]
   [string]
   $VCAdminUser,

   [Parameter(Mandatory = $true)]
   [string]
   $VCAdminUserPassword,

   [Parameter(Mandatory = $true)]
   [string]
   $SolutionUser,

   [Parameter(Mandatory = $true)]
   $SolutionSigningCertificate,

   [Parameter(Mandatory = $true)]
   $SolutionServiceTlsCertificate,

   [Parameter(Mandatory = $true)]
   [string]
   $SolutionServiceId,

   [Parameter(Mandatory = $true)]
   [string]
   $SolutionServiceName)


   $ssoAdminClient = New-Object `
      'VMware.ScriptRuntimeService.SsoAdmin.SsoAdminClient' `
      -ArgumentList $VCIPAddress, (New-Object 'Ses.Tests.AcceptAllX509CertificateValidator')

   $ssoAdminClient.
         CreateLocalSolutionUser(
            $VCAdminUser,
            (ConvertTo-SecureString -String $VCAdminUserPassword -Force -AsPlainText),
            $SolutionUser,
            $SolutionSigningCertificate,
            "Solution User Description");

   $lsClient = New-Object `
      'VMware.ScriptRuntimeService.Ls.LookupServiceClient' `
      -ArgumentList $VCIPAddress, (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

   $lsClient.RegisterService(
            $VCAdminUser,
            (ConvertTo-SecureString -String $VCAdminUserPassword -Force -AsPlainText),
            [guid]::NewGuid().Guid,
            $SolutionUser,
            $SolutionServiceName,
            $SolutionServiceId,
            "$($SolutionServiceName).ServiceName",
            "1.0",
            "com.vmware.$($SolutionServiceName)",
            "service_type",
            "http://localhost:1234/",
            "https",
            "com.vmware.$($SolutionServiceName)",
            $SolutionServiceTlsCertificate);
}

function Delete-VCSASolution {
param(
   [Parameter(Mandatory = $true)]
   [string]
   $VCIPAddress,

   [Parameter(Mandatory = $true)]
   [string]
   $VCAdminUser,

   [Parameter(Mandatory = $true)]
   [string]
   $VCAdminUserPassword,

   [Parameter(Mandatory = $true)]
   [string]
   $SolutionUser,

   [Parameter(Mandatory = $true)]
   [string]
   $SolutionServiceId)

   $lsClient = New-Object `
      'VMware.ScriptRuntimeService.Ls.LookupServiceClient' `
      -ArgumentList $VCIPAddress, (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

   $lsClient.DeleteService(
      $VCAdminUser,
      (ConvertTo-SecureString -String $VCAdminUserPassword -Force -AsPlainText),
      $SolutionServiceId)

   $ssoAdminClient = New-Object `
      'VMware.ScriptRuntimeService.SsoAdmin.SsoAdminClient' `
      -ArgumentList $VCIPAddress, (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

   $ssoAdminClient.
         DeleteLocalPrincipal(
            $VCAdminUser,
            (ConvertTo-SecureString -String $VCAdminUserPassword -Force -AsPlainText),
            $SolutionUser);
}