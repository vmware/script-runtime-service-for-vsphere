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

Describe "Runspaces API Tests" {
   Context "Runspaces API Workflow" {
      BeforeAll {
            ## Connect SES API
            $script:sesUser1Connection = Connect-SrsWithSignAuthorization `
               -SRSIPAddress $SRSIPAddress `
               -vcServer $VCIPAddress `
               -username $VCUser1 `
               -password $VCUser1Password
      }

      AfterEach {
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
      }

      It 'Create Runspace should return object with Id' {

         # Arrange
         $connection = $script:sesUser1Connection

         # Act
         $runspace = Invoke-RestMethod `
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

         # Assert
         $runspace | Should -Not -Be $null
         $runspace.Id | Should -Not -Be $null

      }

      It 'Get Runspace by Id should return object with desired' {

         # Arrange
         $connection = $script:sesUser1Connection

         $expected = Invoke-RestMethod `
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

         # Act
         $actual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/runspaces/$($expected.Id)" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $actual.Id | Should -Be $expected.Id

      }

      It 'List Runspace should return all runspaces' {

         # Arrange
         $connection = $script:sesUser1Connection

         $runspace1 = Invoke-RestMethod `
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

         $runspace2 = Invoke-RestMethod `
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

         $expected = @($runspace1.Id, $runspace2.Id)

         # Act
         $runspaces = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/runspaces" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $actual = $runspaces | Foreach-Object { $_.Id }


         # Assert
         $actual | Should -HaveCount $expected.Count
         $actual | Should -Contain $expected[0]
         $actual | Should -Contain $expected[1]

      }

      It 'Delete Runspace should remove runspace' {

         # Arrange
         $connection = $script:sesUser1Connection

         $runspace = Invoke-RestMethod `
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

         # Act
         $actual = Invoke-RestMethod `
               -Method DELETE `
               -Uri "$($connection.APIBasePath)/runspaces/$($runspace.Id)" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $actual | Should -BeNullOrEmpty
         $actual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/runspaces" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $actual | Should -BeNullOrEmpty
      }

      It 'Gets Runspace taht doesn`t exist' {

         # Arrange
         $connection = $script:sesUser1Connection
         $unexistentId = [guid]::NewGuid().ToString()
         $httpException = $null
         $errorDetails = $null

         # Act
         try {
            Invoke-RestMethod `
                  -Method GET `
                  -Uri "$($connection.APIBasePath)/runspaces/$unexistentId" `
                  -Headers $connection.Headers `
                  -SkipCertificateCheck

         } catch {
            $httpException = $_.Exception
            $errorDetails = $_.ErrorDetails | ConvertFrom-Json
         }

         # Assert
         $httpException | Should -Not -Be $null
         $httpException.Response.StatusCode.value__ | Should -Be 404 # Not Found
         $errorDetails.code | Should -Be 2030
         $errorDetails.error_message | Should -Be "Runspace with id '$unexistentId' not found"
      }
   }

   Context "Max Number Of Runspaces" {
      AfterEach {
         $connection = $script:sesUser1Connection

         ## Get All Runspaces For Connection
         $runspaces = Invoke-RestMethod `
            -Method GET `
            -Uri "$($connection.APIBasePath)/runspaces" `
            -Headers $connection.Headers `
            -Body (
                  @{
                     "run_vc_connection_script" = $false
                  } `
                  | ConvertTo-Json
               ) `
            -SkipCertificateCheck

         ## Clean up runspaces
         foreach ($runspace in $runspaces) {
            Invoke-RestMethod `
               -Method DELETE `
               -Uri "$($connection.APIBasePath)/runspaces/$($runspace.Id)" `
               -Headers $connection.Headers `
               -SkipCertificateCheck
         }
      }
      It 'Requests create runspaces until maximum allowed runspaces are reached' {
         # Arrange
         $connection = $script:sesUser1Connection
         $actualError = $null
         $numberOfCreatedRunspaces = 0
         $runspaceResponse = $null
         $errorDetails = $null

         # Act
         while ($true) {
            try {
               $runspaceResponse = Invoke-RestMethod `
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

               $numberOfCreatedRunspaces++
               Start-Sleep -Seconds 1
            } catch {
               $actualError = $_
               $errorDetails = $_.ErrorDetails | ConvertFrom-Json
               break
            }
         }

         # Assert
         $numberOfCreatedRunspaces | Should -BeGreaterThan 0
         $actualError | Should -Not -Be $null
         $actualError.Exception.Response.StatusCode | Should -Be 500 # Forbidden
         $errorDetails.code | Should -Be 2020
         $errorDetails.error_message | Should -Match "Maximum number of running runspaces is reached"
      }
   }
}