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

Describe "PowerCLI Scripts Tests" {
   Context "PowerCLI Scripts API Workflow" {
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
                     "run_vc_connection_script" = $true
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

      It 'Connects PowerCLI to VC and lists vi accounts with Get-VIAccount cmdlet' {

         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id

         # Act
         ## Test PowerCLI Get-VIAccount cmdlet returns result with the
         ## established PowerCLI connection

         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = 'Get-VIAccount'
                     "name" = "Get VIAccount"
                     "output_objects_format" = "json"
                  } `
                  | ConvertTo-Json) `
               -SkipCertificateCheck

         $scriptTask = Wait-ScriptTask $connection $scriptTask

         $scriptTask.State | Should -Be 'success'

         ## Get Script Objects Result
         $actualviAccountObjects = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         $actualviAccountObjects.Count | Should -BeGreaterOrEqual 2

         ## Convert object results to objects
         $viAccountObjects = $actualviAccountObjects | ConvertFrom-Json

         ## Check VC folders present in the result objects
         $viAccountNames = $viAccountObjects | Foreach-Object { $_.Value.Name }
         $viAccountNames | Should -Contain 'root'

         ## Check Object's Interfaces property contains VMware.VimAutomation.ViCore.Types.V1.PermissionManagement.VIAccount
         $viAccountObjectInterfaces = $viAccountObjects | Select-Object -First 1 | Foreach-Object { $_.Interfaces }
         $viAccountObjectInterfaces | Should -Contain 'VMware.VimAutomation.ViCore.Types.V1.PermissionManagement.VIAccount'
      }

      It 'Tests Multiple PowerCLI scripts in same runspace produce text output' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id

         # Act
         $powerCLIScript1 = 'Get-VIAccount'
         $powercliScript2 = 'New-Datacenter -Name SESTest -Location datacenters | Out-Null; Get-Datacenter -Name SESTest; Get-Datacenter -Name SESTest | Remove-Datacenter -Confirm:$false'

         ## Run Script 1
         $script1Task = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $powerCLIScript1
                     "name" = "pcliscript1"
                     "output_objects_format" = "text"
                  } `
                  | ConvertTo-Json) `
               -SkipCertificateCheck

         $script1Task = Wait-ScriptTask $connection $script1Task

         ## Get Text Result of Script 1

         $script1TextResult = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($script1Task.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck


         ## Run Script 2
         $script2Task = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $powercliScript2
                     "name" = "pcliscript2"
                     "output_objects_format" = "text"
                  } `
                  | ConvertTo-Json) `
               -SkipCertificateCheck

         $script2Task = Wait-ScriptTask $connection $script2Task

         ## Get Text Result of Script 2

         $script2TextResult = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($script2Task.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $script1TextResult | Should -Not -BeNullOrEmpty
         $script2TextResult | Should -Not -BeNullOrEmpty

         $script1TextResult.Count | Should -Be 1
         $script2TextResult.Count | Should -Be 1

         $script1TextResult[0].Contains('root') | Should -Be $true
         $script2TextResult[0].Contains('SESTest') | Should -Be $true
      }

      It 'Renames Datacenter using script parameters retrieved with argument transformation script' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $powercliScriptFile = Join-Path (Join-Path $PSScriptRoot "powercliscripts") "RenameDatacenter.ps1"
         $powercliScriptContent = [IO.File]::ReadAllText($powercliScriptFile)

         ## Create Datacenter Script
         $script1Task = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = "New-Datacenter -Name 'DC1' -Location 'datacenters' | Out-Null; Get-Datacenter -Name DC1"
                     "name" = "NewDataCenter"
                     "output_objects_format" = "json"
                  } `
                  | ConvertTo-Json) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $script1Task = Wait-ScriptTask $connection $script1Task

         ## Get Script Text Result
         $newDatacenterObjectsResult = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($script1Task.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         ## Get Datacenter Id from result
         $dcObject = $newDatacenterObjectsResult | Select-Object -First 1 | ConvertFrom-Json
         $dcID = $dcObject.Value.Id

         # Act
         ## Generate Script Parameter From Datacenter Id and Server Name
         $dcScriptParameter = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/argument-scripts/script" `
               -Headers $connection.Headers `
               -Body (
               @{
                  "template_id" = "datacenter-by-id-server-name"
                  "template_placeholder_value_list" = @(
                     @{
                        "values" = @(
                           @{
                              "placeholder_name" = "id"
                              "value" = @($dcID)
                           },
                           @{
                              "placeholder_name" = "server"
                              "value" = @("*")
                           }
                        )
                     }
                  )
               } `
               | ConvertTo-Json -Depth 10) `
               -SkipCertificateCheck

         ## Call Script With Parameters
         $newName = "MyRenamedDC"

         $parameterDatacenter = @{
            "name" = "datacenter"
            "script" = $dcScriptParameter.script
         }

         $parameterName  = @{
            "name" = "name"
            "value" = $newName
         }

         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $powercliScriptContent
                     "script_parameters" = @($parameterDatacenter, $parameterName)
                     "output_objects_format" = "json"
                     "name" = "RenameDCWithParams"
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask

         # Assert
         $scriptTask.State | Should -Be 'success'

         $renameDatacenterObjectsResult = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($scriptTask.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         ## Get Datacenter Id from result
         $dcObject = $renameDatacenterObjectsResult | Select-Object -First 1 | ConvertFrom-Json
         $actual = $dcObject.Value.Name

         $actual | Should -Be $newName

         # Clean Up
         $scriptTask = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = "param(`$datacenter) Remove-Datacenter -Datacenter `$datacenter -Confirm:`$false"
                     "script_parameters" = @(,$parameterDatacenter)
                     "output_objects_format" = "json"
                     "name" = "RemoveDCWithParams"
                  } `
                  | ConvertTo-Json
               ) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $scriptTask = Wait-ScriptTask $connection $scriptTask
      }

      It 'Runs CRUD10Datacenters PowerCLI Script' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $powercliScriptFile = Join-Path (Join-Path $PSScriptRoot "powercliscripts") "CRUD10Datacenters.ps1"
         $powercliScriptContent = [IO.File]::ReadAllText($powercliScriptFile)

         # Act
         ## Run Script
         $script1Task = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -Body (
                  @{
                     "runspace_id" = $runspaceId
                     "script" = $powercliScriptContent
                     "name" = "CRUD10Datacenters"
                     "output_objects_format" = "json"
                  } `
                  | ConvertTo-Json) `
               -SkipCertificateCheck

         ## Wait Script To Complete
         $script1Task = Wait-ScriptTask $connection $script1Task

         ## Get Script Text Result
         $script1ObjectsResult = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions/$($script1Task.Id)/output" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $script1Task.State | Should -Be 'success'
         $script1ObjectsResult.Count | Should -Be 20
         ($script1ObjectsResult | Foreach-Object { ($_ | ConvertFrom-Json).Value.Name }) -contains 'SES-Test-Datacenter-2' | Should -Be $true
         ($script1ObjectsResult | Foreach-Object { ($_ | ConvertFrom-Json).Value.Name }) -contains 'SES-Test-Datacenter-2-updated' | Should -Be $true
      }
   }
}