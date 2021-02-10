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

$script:vSphereIntegrationLibsOutput = (Join-Path $PsScriptRoot "vsphere-libs-output")

function Test-BuildToolsAreAvailable {
   $dotnetSdk = Get-Command 'dotnet'
   if (-not $dotnetSdk) {
     throw "'dotnet' sdk is not available"
   }
}

function Build-vSphereIntegrationLibrary {
param(
   [string]
   $LibraryName,

   [string]
   $OutputPath
)
   $librariesSourcePath = (Join-Path (Join-Path ($PsScriptRoot | Split-Path) "src") "vSphereIntegrationLibraries")
   $libraryProjectFile = [IO.Path]::Combine($librariesSourcePath, $LibraryName, "$LibraryName.csproj")

   if (-not (Test-Path $libraryProjectFile)) {
      throw "Cannot build library '$LibraryName'. Project file '$libraryProjectFile' not found"
   }

   Write-Host "Build '$libraryProjectFile'"
   dotnet build $libraryProjectFile -o $OutputPath | Out-Null

   # Return Path to build output
   [IO.Path]::Combine(
      $OutputPath,
      "$LibraryName.dll")
}

# Build .NET libraries needed for integration tests from source
Test-BuildToolsAreAvailable

# Create .NET libraries output dir
if (-not (Test-Path $script:vSphereIntegrationLibsOutput)) {
   New-Item -ItemType Directory -Path $script:vSphereIntegrationLibsOutput | Out-Null
}

$STSLibraryPath = Build-vSphereIntegrationLibrary "VMware.ScriptRuntimeService.Sts" $script:vSphereIntegrationLibsOutput
Write-Host "STS Library: '$STSLibraryPath'"
$LSLibraryPath = Build-vSphereIntegrationLibrary "VMware.ScriptRuntimeService.Ls" $script:vSphereIntegrationLibsOutput
Write-Host "LS Library: '$LSLibraryPath'"
$SsoAdminLibraryPath = Build-vSphereIntegrationLibrary "VMware.ScriptRuntimeService.SsoAdmin" $script:vSphereIntegrationLibsOutput
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

# Get Registered SRS Solutions
$services = $lsClient.ListRegisteredServices()
$srsServices = $services | Where-Object {$_.serviceDescriptionResourceKey -eq 'srs.ServiceDescritpion'}

$srsServices | Foreach-Object {
   $srsServiceRegistration = $_
   Write-Host "Remove SRS Service registration '$($srsServiceRegistration.serviceId)' from VC"

   $lsClient.DeleteService(
      $VcUser,
      (ConvertTo-SecureString -String $VcPassword -Force -AsPlainText),
      $srsServiceRegistration.serviceId)
}

$ssoAdminClient = New-Object `
      'VMware.ScriptRuntimeService.SsoAdmin.SsoAdminClient' `
      -ArgumentList $lsClient.GetSsoAdminEndpointUri(),  $lsClient.GetStsEndpointUri(), (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

$srsSolutionOwners = $ssoAdminClient.
          FindSolutionUser(
             $VcUser,
             (ConvertTo-SecureString -String $VcPassword -Force -AsPlainText),
             'srs-SolutionOwner',
             10);

$srsSolutionOwners | Foreach-Object {
   $srsSolutionOwner = $_
   Write-Host "Remove SRS Solution User '$srsSolutionOwner' from VC"

   $ssoAdminClient.
         DeleteLocalPrincipal(
            $VcUser,
            (ConvertTo-SecureString -String $VcPassword -Force -AsPlainText),
            $srsSolutionOwner);
}
