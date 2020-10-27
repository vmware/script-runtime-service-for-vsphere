# **************************************************************************
#  Copyright 2020 VMware, Inc.
#  SPDX-License-Identifier: Apache-2.0
# **************************************************************************

#
# This is helper script to cleanup SRS service and solution user registration from VCSA
# It builds the needed .NET libraries from source code, then
# searches for SRS registration on specified VC server.
# If valid registration is found deletes the entries from VCSA.
#

param(
   [Parameter(Mandatory=$true)]
   [string]
   $VcAddress,

   [Parameter(Mandatory=$true)]
   [string]
   $VcUser,

   [Parameter(Mandatory=$true)]
   [string]
   $VcPassword
)

function Test-BuildToolsAreAvailable {
   $dotnetSdk = Get-Command 'dotnet'
   if (-not $dotnetSdk) {
     throw "'dotnet' sdk is not available"
   }
}

function Build-vSphereIntegrationLibrary {
param(
   [string]
   $LibraryName
)
   $librariesSourcePath = (Join-Path (Join-Path ($PsScriptRoot | Split-Path) "src") "vSphereIntegrationLibraries")
   $libraryProjectFile = [IO.Path]::Combine($librariesSourcePath, $LibraryName, "$LibraryName.csproj")

   if (-not (Test-Path $libraryProjectFile)) {
      throw "Cannot build library '$LibraryName'. Project file '$libraryProjectFile' not found"
   }

   Write-Host "Build '$libraryProjectFile'"
   dotnet build $libraryProjectFile | Out-Null

   # Return Path to build output
   [IO.Path]::Combine(
      $librariesSourcePath,
      $LibraryName,
      "bin",
      "Debug",
      "netcoreapp3.0",
      "$LibraryName.dll")
}

# Build .NET libraries needed for integration tests from source
Test-BuildToolsAreAvailable

$STSLibraryPath = Build-vSphereIntegrationLibrary "VMware.ScriptRuntimeService.Sts"
Write-Host "STS Library: '$STSLibraryPath'"
$LSLibraryPath = Build-vSphereIntegrationLibrary "VMware.ScriptRuntimeService.Ls"
Write-Host "LS Library: '$LSLibraryPath'"
$SsoAdminLibraryPath = Build-vSphereIntegrationLibrary "VMware.ScriptRuntimeService.SsoAdmin"
Write-Host "Sso Admin Library: '$SsoAdminLibraryPath'"

Import-Module $STSLibraryPath
Import-Module $LSLibraryPath
Import-Module $SsoAdminLibraryPath

$certificateValidatorHelper = [IO.Path]::Combine(
   ($PsScriptRoot | Split-Path),
   "test",
   "api-integration-tests",
   "tests",
   "helpers",
   "CertificateValidators",
   "bin",
   "CertificateValidators.dll"
)

Import-Module $certificateValidatorHelper

$lsClient = New-Object `
   'VMware.ScriptRuntimeService.Ls.LookupServiceClient' `
   -ArgumentList $VcAddress, (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

# Get SRS Solution
$services = $lsClient.ListRegisteredServices()
$srsService = $services | Where-Object {$_.serviceDescriptionResourceKey -eq 'srs.ServiceDescritpion'}

if ($srsService -ne $null) {
   Write-Host "Remove SRS Service registration from VC"
   $lsClient.DeleteService(
      $VcUser,
      (ConvertTo-SecureString -String $VcPassword -Force -AsPlainText),
      $srsService.serviceId)

   $ssoAdminClient = New-Object `
      'VMware.ScriptRuntimeService.SsoAdmin.SsoAdminClient' `
      -ArgumentList $lsClient.GetSsoAdminEndpointUri(),  $lsClient.GetStsEndpointUri(), (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

   $ssoAdminClient.
         DeleteLocalPrincipal(
            $VcUser,
            (ConvertTo-SecureString -String $VcPassword -Force -AsPlainText),
            $srsService.ownerId );
}