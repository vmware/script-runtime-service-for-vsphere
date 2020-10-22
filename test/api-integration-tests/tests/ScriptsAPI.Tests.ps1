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

Describe "Scripts API Tests" {
   Context "Scripts API Workflow" {
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

      It 'Starts named script' {

         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "Write-Output 'Test Text'"
         $expectedScriptName = "TestNamedScript"
         $expectedOutputObjectsFormat = "text"

         # Act
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "name" = $expectedScriptName
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         # Assert
         $scriptTask | Should -Not -Be $null
         $scriptTask.Id | Should -Not -Be $null
         $scriptTask.State | Should -Be 'running'
         $scriptTask.Name | Should -Be $expectedScriptName
         $scriptTask.Reason | Should -BeNullOrEmpty
         $scriptTask.output_objects_format | Should -Be $expectedOutputObjectsFormat

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         ## Assert ScriptTask State
         $scriptTask.State | Should -Be 'success'
         $scriptTask.Name | Should -Be $expectedScriptName
         $scriptTask.Reason | Should -BeNullOrEmpty
      }

      It 'Runs "Write-Output "Test Text"" script' {

         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $expectedString = 'Test Text'
         $testScript = "Write-Output '$expectedString'"

         # Act
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "output_objects_format" = "text"
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         # Assert
         $scriptTask | Should -Not -Be $null
         $scriptTask.Id | Should -Not -Be $null
         $scriptTask.State | Should -Be 'running'
         $scriptTask.Name | Should -BeNullOrEmpty
         $scriptTask.Reason | Should -BeNullOrEmpty

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         ## Assert ScriptTask State
         $scriptTask.State | Should -Be 'success'
         $scriptTask.Reason | Should -BeNullOrEmpty

         ## Get Script Text Result
         $actualString = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $actualString.Trim() | Should -Be $expectedString
      }

      It 'Get script by Id' {

         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "Write-Output 'Test Text'"
         $expectedScriptName = "GetScriptByIdTest"

         ## Start Script
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "name" = $expectedScriptName
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         $expectedScriptId = $scriptTask.Id

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         # Act
         $actual = Invoke-RestMethod `
                     -Method GET `
                     -Uri "$($connection.APIBasePath)/script-executions/$($expectedScriptId)" `
                     -Headers $connection.Headers `
                     -SkipCertificateCheck

         # Assert
         $actual.Id | Should -Be $expectedScriptId
         $actual.State | Should -Be 'success'
         $actual.Name | Should -Be $expectedScriptName
         $actual.Reason | Should -BeNullOrEmpty

      }

      It 'List scripts' {

         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id

         # Act
         ## It lists all scripts from previous tests
         $scripts = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $scripts = $scripts | Sort-Object -Property end_time -Descending

         # Assert
         $scripts | Should -Not -Be $null
         $scripts.Count | Should -BeGreaterThan 1
         $scripts[0].State | Should -Be 'success'
         $scripts[0].Id | Should -Not -BeNullOrEmpty

      }

      It 'Gets Script Streams' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $expectedDebugMessage = 'Debug Message'
         $expectedVerboseMessage = 'Verbose Message'
         $expectedWarningMessage = 'Warning Message'
         $expectedInformationMessage = 'Information Message'
         $expectedErrorMessage = 'Error Message'

         $testScript = "
            `$DebugPreference = 'Continue'
            `$VerbosePreference = 'Continue'
            `$WarningPreference = 'Continue'
            `$InformationPreference = 'Continue'
            `$ErrorPreference = 'Continue'

            Write-Debug '$expectedDebugMessage'
            Write-Verbose '$expectedVerboseMessage'
            Write-Warning '$expectedWarningMessage'
            Write-Information '$expectedInformationMessage'
            Write-Error '$expectedErrorMessage'
         "

         ## Start Script Execution
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         # Act
         $debugActual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/streams/debug" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $warningActual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/streams/warning" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $verboseActual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/streams/verbose" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $informationActual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/streams/information" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $errorActual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/streams/error" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $dtNow = Get-Date
         $debugActual[0].Message | Should -Be $expectedDebugMessage
         $debugActual[0].Time.Hour | Should -Be $dtNow.Hour
         $debugActual[0].Time.Day | Should -Be $dtNow.Day
         $debugActual[0].Time.Year | Should -Be $dtNow.Year

         $verboseActual[0].Message | Should -Be $expectedVerboseMessage
         $warningActual[0].Message | Should -Be $expectedWarningMessage
         $informationActual[0].Message | Should -Be $expectedInformationMessage
         $errorActual[0].Message | Should -Be $expectedErrorMessage

      }

      It 'Gets Script Objects' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "
            Write-Output (Get-Date -Year 2000 -Month 1 -Day 1)
            Write-Output (Get-Date -Year 2000 -Month 1 -Day 2)
         "

         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "output_objects_format" = "json"
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask


         # Act
         ## Get Script Objects Result
         $actual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $scriptTask.output_objects_format | Should -Be 'json'
         $actual.Count | Should -Be 2
         $date1Object = ConvertFrom-Json $actual[0]
         $date1Object.TypeName | Should -Be 'System.DateTime'
         $date1Object.Value | Should -Not -Be ( (Get-Date -Year 2000 -Month 1 -Day 1).ToUniversalTime() | Get-Date -Format 'o')
         $date2Object = ConvertFrom-Json $actual[1]
         $date2Object.TypeName | Should -Be 'System.DateTime'
         $date2Object.Value | Should -Not -Be ( (Get-Date -Year 2000 -Month 1 -Day 2).ToUniversalTime() | Get-Date -Format 'o')
      }

      It 'Gets Script Text' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $expectedString = 'Test Text'
         $testScript = "Write-Output '$expectedString'"

         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "output_objects_format" = "text"
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask


         # Act
         ## Get Script Text Result
         $actualString = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $actualString.Trim() | Should -Be $expectedString
      }

      It 'Cancels Long Running Script' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "
            1..30 | % {
               Start-Sleep -Second 1
            }
         "

         ## Start Script Execution
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         Start-Sleep -Seconds 3

         # Act
         ## Cancel Script
         Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/cancel" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         Wait-ScriptTask $connection $scriptTask

         # Assert
         ## Get Script
         $actual = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $actual.State | Should -Be 'canceled'
      }

      It 'Tests Script Failure' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "Write-Output 'Test Message'; throw 'Test Error'"

         # Act
         ## Start Script Execution
         $actual = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "output_objects_format" = "text"
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $actual = Wait-ScriptTask $connection $actual

         # Assert
         $actual.State | Should -Be 'error'
         $actual.Reason.Contains('Test Error') | Should -Be $true

         ## Get Script Text Result
         $actualString = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($actual.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         ## Assert Script Output is empty
         $actualString[0].Trim() | Should -BeNullOrEmpty
      }
   }
   Context "Negative Scenarios for Scripts API" {
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

      It 'Requests a script execution to unexistent runspaces' {
         # Covers bug #2439886

         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = 'invalid'
         $testScript = "Write-Output 'Negative Test Case'"

         # Act
         $httpException = $null
         $errorDetails = $null
         try {
            Invoke-RestMethod `
                  -Method POST `
                  -Uri "$($connection.APIBasePath)/script-executions" `
                  -Headers $connection.Headers `
                  -Body (
                     @{
                        "runspace_id" = $runspaceId
                        "script" = $testScript
                        "output_objects_format" = "text"
                     } `
                     | ConvertTo-Json
                  ) `
                  -SkipCertificateCheck
         } catch {
            $httpException = $_.Exception
            $errorDetails = $_.ErrorDetails | ConvertFrom-Json
         }

         # Assert
         $httpException | Should -Not -Be $null
         $httpException.Response.StatusCode.value__ | Should -Be 404 # Not Found
         $errorDetails.code | Should -Be 2030
         $errorDetails.error_message | Should -Match "Runspace with id '.*' not found"
      }

      It 'Request script execution before previous script in the runspace has finished' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $testScript = "Start-Sleep -Seconds 5; Write-Output 'Test Text'"
         $expectedScriptName = "TestNamedScript"
         $expectedOutputObjectsFormat = "text"

         $script1Task = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "name" = $expectedScriptName
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         $httpException = $null
         $errorDetails = $null

         # Act
         try {
            $testScript = "Write-Output 'Test Text'"
            Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $testScript
                     "name" = $expectedScriptName
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck
         } catch {
            $httpException = $_.Exception
            $errorDetails = $_.ErrorDetails | ConvertFrom-Json
         }

         # Assert
         $httpException | Should -Not -Be $null
         $httpException.Response.StatusCode.value__ | Should -Be 500 # InternalServerError
         $errorDetails.code | Should -Be 1310
         $errorDetails.error_message | Should -Be "Another script with id '$($script1Task.Id)' is running"

         # Wait script to finish
         Wait-ScriptTask $connection $script1Task
      }

      It 'Gets script that doesn`t exist' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $unexistentId = [guid]::NewGuid().ToString()
         $httpException = $null
         $errorDetails = $null

         # Act
         try {
            Invoke-RestMethod `
                  -Method GET `
                  -Uri "$($connection.APIBasePath)/script-executions/$unexistentId" `
                  -Headers $connection.Headers `
                  -SkipCertificateCheck
         } catch {
            $httpException = $_.Exception
            $errorDetails = $_.ErrorDetails | ConvertFrom-Json
         }

         # Assert
         $httpException | Should -Not -Be $null
         $httpException.Response.StatusCode.value__ | Should -Be 404 # Not Found
         $errorDetails.code | Should -Be 1040
         $errorDetails.error_message | Should -Be "Script with id '$unexistentId' not found"
      }
   }
}