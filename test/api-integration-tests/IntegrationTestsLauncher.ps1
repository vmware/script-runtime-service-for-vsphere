# **************************************************************************
#  Copyright 2020 VMware, Inc.
#  SPDX-License-Identifier: Apache-2.0
# **************************************************************************

param(
    [Parameter(Mandatory = $true)]
    [string]
    $SRSIPAddress,

    [Parameter(Mandatory = $true)]
    [string]
    $VCIPAddress,

    [Parameter(Mandatory = $true)]
    [string]
    $VCUser1,

    [Parameter(Mandatory = $true)]
    [string]
    $VCUser1Password,

    [string]
    $OutputFile,

    [Switch]
    $EnableExit,

    [string]
    $TestFileFilter,

    [string]
    $TestName
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
   $librariesSourcePath = (Join-Path (Join-Path ($PsScriptRoot | Split-Path | Split-Path) "src") "vSphereIntegrationLibraries")
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
$SsoAuthenticationLibraryPath = Build-vSphereIntegrationLibrary "VMware.Http.Sso.Authentication"
Write-Host "Sso Authentication Library: '$SsoAuthenticationLibraryPath'"
$SsoAdminLibraryPath = Build-vSphereIntegrationLibrary "VMware.ScriptRuntimeService.SsoAdmin"
Write-Host "Sso Admin Library: '$SsoAdminLibraryPath'"

if ($TestFileFilter) {
   $TestFileFilter = "*$TestFileFilter*"
} else {
   $TestFileFilter = "*"
}

$testResults = Invoke-Pester `
   -Script @{
       Path = Join-Path (Join-Path $PSScriptRoot tests) $TestFileFilter
       Parameters = @{
         SRSIPAddress = $SRSIPAddress
         STSLibraryPath = $STSLibraryPath
         LSLibraryPath = $LSLibraryPath
         SsoAuthenticationLibraryPath = $SsoAuthenticationLibraryPath
         SsoAdminLibraryPath = $SsoAdminLibraryPath
         VCIPAddress = $VCIPAddress
         VCUser1 = $VCUser1
         VCUser1Password = $VCUser1Password
      }
   } `
   -TestName $TestName `
   -PassThru

if ($OutputFile) {
   $testResults | `
   Foreach-Object { $_.TestResult } | `
   Select-Object Describe, Context, Name, Result, FailureMessage | `
   Export-Csv $OutputFile
}

if  ($EnableExit) {
   exit $testResults.FailedCount
}