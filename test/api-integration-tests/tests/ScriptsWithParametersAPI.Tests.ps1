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
    $STSLibraryPath,

    [Parameter(Mandatory = $true)]
    [string]
    $LSLibraryPath,

    [Parameter(Mandatory = $true)]
    [string]
    $SsoAuthenticationLibraryPath,

    [Parameter(Mandatory = $true)]
    [string]
    $SsoAdminLibraryPath,

    [Parameter(Mandatory = $true)]
    [string]
    $VCIPAddress,

    [Parameter(Mandatory = $true)]
    [string]
    $VCUser1,

    [Parameter(Mandatory = $true)]
    [string]
    $VCUser1Password
)

# Import SES API Helper Functions
$apiHelpersPath = Join-Path $PSScriptRoot SRSAPIHelpers.ps1
. $apiHelpersPath `
   -stsLibraryPath $STSLibraryPath `
   -lsLibraryPath $LSLibraryPath `
   -ssoAdminLibraryPath $SsoAdminLibraryPath `
   -ssoAuthenticationLibraryPath $SsoAuthenticationLibraryPath

# Script Variables to store state for all tests
$script:sesUser1Connection
$script:scriptTestsRunspace

Describe "Scripts with Parameters API Tests" {
   Context "Scripts with Parameters API Workflow" {
      BeforeAll {

            ## Connect SES API
            $script:sesUser1Connection = Connect-SrsWithSignAuthorization `
               -SRSIPAddress $SRSIPAddress `
               -vcServer $VCIPAddress `
               -username $VCUser1 `
               -password $VCUser1Password

            $connection = $script:sesUser1Connection

            ## Create Runspace
            $script:scriptTestsRunspace = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/runspaces" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "run_vc_connection_script" = $false
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

            Wait-Runspace $connection $script:scriptTestsRunspace
      }

      AfterAll {
         $connection = $script:sesUser1Connection

         ## Get All Runspaces For Connection
         $runspaces = Invoke-RestMethod `
            -Method GET `
            -Uri "$($connection.APIBasePath)/runspaces" `
            -Headers $connection.Headers `
            -SkipCertificateCheck

         ## Clean up runspaces
         foreach ($runspace in $runspaces) {
            Invoke-RestMethod `
               -Method DELETE `
               -Uri "$($connection.APIBasePath)/runspaces/$($runspace.Id)" `
               -Headers $connection.Headers `
               -SkipCertificateCheck
         }

         $script:scriptTestsRunspace = $null
      }

      It 'Executes script with one parameter passed by value' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "param([Parameter()][int]`$a) `$result = `$a + 5; `$result"
         $parameter = @{
            "name" = "a"
            "value" = 4
         }

         # Act
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "script_parameters" = @(,$parameter)
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         # Assert
         ## Assert ScriptTask State
         $scriptTask.State | Should -Be 'success'

         ## Get Script Text Result
         $actualString = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         ## Assert Script Output is empty
         $actualString[0].Trim() | Should -Be "9"
      }

      It 'Executes script with one parameter passed by value' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "param([Parameter()][string]`$a, [int]`$b) `$result = `$a + `$b; `$result"
         $parameterA = @{
            "name" = "a"
            "value" = "You"
         }
         $parameterB = @{
            "name" = "b"
            "value" = 2
         }

         # Act
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "script_parameters" = @($parameterA, $parameterB)
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         # Assert
         ## Assert ScriptTask State
         $scriptTask.State | Should -Be 'success'

         ## Get Script Text Result
         $actualString = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         ## Assert Script Output is empty
         $actualString[0].Trim() | Should -Be "You2"
      }

      It 'Executes script with one parameter passed by scrtip' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "param([Parameter()][int]`$a) `$result = `$a + 5; `$result"
         $parameter = @{
            "name" = "a"
            "script" = "1 + 3"
         }

         # Act
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "script_parameters" = @(,$parameter)
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         # Assert
         ## Assert ScriptTask State
         $scriptTask.State | Should -Be 'success'

         ## Get Script Text Result
         $actualString = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         ## Assert Script Output is empty
         $actualString[0].Trim() | Should -Be "9"
      }
   }
}