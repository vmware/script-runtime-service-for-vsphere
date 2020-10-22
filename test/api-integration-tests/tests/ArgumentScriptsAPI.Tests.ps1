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

Describe "Arguments Scripts API Tests" {
   Context "Retrieval and Generation API Workflow" {
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

      It 'Lists argument script templates' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $expectedItem1 = "vm-by-id-server-name"
         $expectedItem2 = "vm-by-id-server-instance-uuid"


         # Act
         $scriptsList = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/argument-scripts/templates" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $scriptsList | Should -Not -Be $null
         $scriptsList.id | Should -Contain $expectedItem1
         $scriptsList.id | Should -Contain $expectedItem2
      }

      It 'Gets argument script template' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $scriptName = "vm-by-id-server-name"
         $expectedContent = "Get-VM -Id <id> -Server <server>"


         # Act
         $actualTemplate = Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/argument-scripts/templates/$($scriptName)" `
               -Headers $connection.Headers `
               -SkipCertificateCheck

         # Assert
         $actualTemplate | Should -Not -Be $null
         $actualTemplate.script_template | Should -Be $expectedContent
         $actualTemplate.placeholders | Should -Contain 'id'
         $actualTemplate.placeholders | Should -Contain 'server'
      }

      It 'Generates argument script by one id and one server' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $vmId = "vm-1"
         $serverName = "10.23.45.67"
         $scriptName = "vm-by-id-server-name"
         $expectedContent = "Get-VM -Id '$vmId' -Server '$serverName'"


         # Act
         $actualContent = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/argument-scripts/script" `
               -Headers $connection.Headers `
               -Body (
               @{
                  "template_id" = $scriptName
                  "template_placeholder_value_list" = @(
                     @{
                        "values" = @(
                           @{
                              "placeholder_name" = "id"
                              "value" = @($vmId)
                           },
                           @{
                              "placeholder_name" = "server"
                              "value" = @($serverName)
                           }
                        )
                     }
                  )
               } `
               | ConvertTo-Json -Depth 10) `
               -SkipCertificateCheck

         # Assert
         $actualContent | Should -Not -Be $null
         $actualContent.script | Should -Be $expectedContent
      }

      It 'Generates argument script by two ids and one server' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $vm1Id = "vm-1"
         $vm2Id = "vm-2"
         $serverName = "10.23.45.67"
         $scriptName = "vm-by-id-server-name"
         $expectedContent = "Get-VM -Id '$vm1Id','$vm2Id' -Server '$serverName'"


         # Act
         $actualContent = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/argument-scripts/script" `
               -Headers $connection.Headers `
               -Body (
               @{
                  "template_id" = $scriptName
                  "template_placeholder_value_list" = @(
                     @{
                        "values" = @(
                           @{
                              "placeholder_name" = "id"
                              "value" = @($vm1Id, $vm2Id)
                           },
                           @{
                              "placeholder_name" = "server"
                              "value" = @($serverName)
                           }
                        )
                     }
                  )
               } `
               | ConvertTo-Json -Depth 10) `
               -SkipCertificateCheck

         # Assert
         $actualContent | Should -Not -Be $null
         $actualContent.script | Should -Be $expectedContent
      }

      It 'Generates argument script by one id and two servers' {
         # Arrange
         $connection = $script:sesUser1Connection
         $runspaceId = $script:scriptTestsRunspace.Id
         $vmId = "vm-1"
         $server1Name = "10.23.45.67"
         $server2Name = "67.23.45.10"
         $scriptName = "vm-by-id-server-name"
         $expectedContent = @("Get-VM -Id '$vmId' -Server '$server1Name'","Get-VM -Id '$vmId' -Server '$server2Name'")


         # Act
         $actualContent = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/argument-scripts/script" `
               -Headers $connection.Headers `
               -Body (
               @{
                  "template_id" = $scriptName
                  "template_placeholder_value_list" = @(
                     @{
                        "values" = @(
                           @{
                              "placeholder_name" = "id"
                              "value" = @($vmId)
                           },
                           @{
                              "placeholder_name" = "server"
                              "value" = @($server1Name)
                           }
                        )
                     },
                     @{
                        "values" = @(
                           @{
                              "placeholder_name" = "id"
                              "value" = @($vmId)
                           },
                           @{
                              "placeholder_name" = "server"
                              "value" = @($server2Name)
                           }
                        )
                     }
                  )
               } `
               | ConvertTo-Json -Depth 10) `
               -SkipCertificateCheck

         # Assert
         $actualContent | Should -Not -Be $null
         $actualScriptContent = $actualContent.script
         ## The below complex assert is to verify multiple script that is generated
         ## To handle different OS new lines charaters, check content of the actual for containing
         ## Expected lines and new line in the middle.
         ## This assert handles all combination of OSs server and client side
         $actualScriptContent.StartsWith($expectedContent[0]) | Should -Be $true
         $actualScriptContent.Contains("`n") | Should -Be $true
         $actualScriptContent.EndsWith($expectedContent[1]) | Should -Be $true
         ($actualScriptContent.Length - 2) -le ($expectedContent[0].Length + $expectedContent[1].Length)| Should -Be $true
      }

      It 'Requests generation of argument script that doesn`t exist' {

         # Arrange
         $connection = $script:sesUser1Connection
         $scriptName = "Unexistent-Script"
         $httpException = $null
         $errorDetails = $null

         # Act
         try {
            $actualContent = Invoke-RestMethod `
               -Method POST `
               -Uri "$($connection.APIBasePath)/argument-scripts/script" `
               -Headers $connection.Headers `
               -Body (
               @{
                  "template_id" = $scriptName
                  "template_placeholder_value_list" = @(
                     @{
                        "values" = @(
                           @{
                              "placeholder_name" = "id"
                              "value" = @('1234')
                           }
                        )
                     }
                  )
               } `
               | ConvertTo-Json -Depth 10) `
               -SkipCertificateCheck
         } catch {
            $httpException = $_.Exception
            $errorDetails = $_.ErrorDetails | ConvertFrom-Json
         }

         # Assert
         $httpException | Should -Not -Be $null
         $httpException.Response.StatusCode.value__ | Should -Be 404 # Not Found
         $errorDetails.code | Should -Be 4010
         $errorDetails.error_message | Should -Be "Argument transformation script not found for '$scriptName'"
      }
   }
}