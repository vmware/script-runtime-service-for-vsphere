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

# Import SRS API Helper Functions
$apiHelpersPath = Join-Path $PSScriptRoot SRSAPIHelpers.ps1
. $apiHelpersPath `
   -stsLibraryPath $STSLibraryPath `
   -lsLibraryPath $LSLibraryPath `
   -ssoAdminLibraryPath $SsoAdminLibraryPath `
   -ssoAuthenticationLibraryPath $SsoAuthenticationLibraryPath

$srsServicePort = Get-SRSPort

$script:srsUser1Connection

$script:TestSolutionUser = "srs-tests-solution-user"
$script:TestSolutionId = "2b26df3f-bacf-4301-b153-1788424f3b24"
$script:TestSolutionServiceName = "srs-tests-solution"

Describe "SIGNAuthenticationAPITests" {
   Context "Login" {
      It "Service To Service Login with SIGN Authnetication and delegated token" {
         # Arrange
         $stsClient = New-Object `
            'VMware.ScriptRuntimeService.Sts.STSClient' `
            -ArgumentList (Get-StsServiceFromLS $VCIPAddress), (New-Object 'CertificateValidators.AcceptAllX509CertificateValidator')

         $srsSolution = Get-SrsServiceFromLS -vcServer $VCIPAddress

         $clientSigningCertificate = Get-ClientSignCertificate

         # Issue HoK token by user and pass with client certificate
         $clientHoK = $stsClient.IssueHoKTokenByUserCredential(
               $VCUser1,
               (ConvertTo-SecureString -String $VCUser1Password -Force -AsPlainText),
               $clientSigningCertificate);

         Import-Module $SsoAuthenticationLibraryPath

         $loginPath = "/api/auth/login"

         $signAuthReq = [VMware.Http.Sso.Authentication.RequestFactory]::Create("POST", $loginPath, $SRSIPAddress, (Get-SRSPort), $null)

         $authzHeaderValues = [VMware.Http.Sso.Authentication.AuthCalculatorFactory]::Create().ComputeToken(
            $signAuthReq,
            $clientSigningCertificate,
            [VMware.Http.Sso.Authentication.SigningAlgorithm]::RSA_SHA256,
            $clientHoK.OuterXml)

         $requestHeaders = @{
            "accept" = "application/json"
            "content-type" = "application/json"
            "authorization" = [string]::Join(",", $authzHeaderValues)
         }

         # Act
         $actual = Invoke-WebRequest `
            -Method POST `
            -Uri "$(Get-SRSProtocol)://$($SRSIPAddress):$(Get-SRSPort)$($loginPath)" `
            -Headers $requestHeaders `
            -SkipCertificateCheck

         # Assert
         $actual.Headers['X-SRS-API-KEY'] | Should -Not -Be $null
      }
   }
}

Describe "BASICAuthenticationAPITests" {
   Context "Login" {
      It "User creates SRS session with username and password" {
         # Arrange
         $password = ConvertTo-SecureString –String  $VCUser1Password –AsPlainText -Force
         $credential = New-Object –TypeName "System.Management.Automation.PSCredential" –ArgumentList $VCUser1, $password
         $requestHeaders = @{
            "accept" = "application/json"
            "content-type" = "application/json"
         }

         # Act
         $actual = Invoke-WebRequest `
            -Method POST `
            -Uri "$(Get-SRSProtocol)://$($SRSIPAddress)/api/auth/login" `
            -Credential $credential `
            -Headers $requestHeaders `
            -SkipCertificateCheck

         # Assert
         $actual.Headers['X-SRS-API-KEY'] | Should -Not -Be $null
      }

      It "User tries to create SRS session with invalid username and password" {
         # Arrange
         $password = ConvertTo-SecureString –String "invalid" –AsPlainText -Force
         $credential = New-Object –TypeName "System.Management.Automation.PSCredential" –ArgumentList $VCUser1, $password
         $acualError = $null
         $requestHeaders = @{
            "accept" = "application/json"
            "content-type" = "application/json"
         }

         # Act
         try {
            Invoke-RestMethod `
               -Method POST `
               -Uri "$(Get-SRSProtocol)://$($SRSIPAddress)/api/auth/login" `
               -Credential $credential `
               -Headers $requestHeaders `
               -SkipCertificateCheck
         } catch {
            $acualError = $_
         }

         $acualError | Should -Not -Be $null
         $acualError.Exception.Response.StatusCode | Should -Be 401 # Unauthorized

         # Assert
         $acualError.Exception.Response.Headers['X-SRS-API-KEY'] | Should -Be $null
         $wwwAuthenticate = $acualError.Exception.Response.Headers | ? {$_.Key -eq 'WWW-Authenticate' }
         $wwwAuthenticate | Should -Not -Be $null
         $wwwAuthenticate.Value[0] | Should -Be 'Basic realm="SRS endpoint"'
      }
   }
}

Describe "Authentication API Tests" {

   Context "Logout API" {
      BeforeEach {
         ## Connect SRS API
         $script:srsUser1Connection = Connect-SrsWithSignAuthorization `
               -SRSIPAddress $SRSIPAddress `
               -vcServer $VCIPAddress `
               -username $VCUser1 `
               -password $VCUser1Password
      }

      It 'Logout closes the session' {

         # Arrange
         $connection = $script:srsUser1Connection

         # Act
         $logoutResult = Invoke-RestMethod `
            -Method POST `
            -Uri "$($connection.APIBasePath)/auth/logout" `
            -Headers $connection.Headers `
            -SkipCertificateCheck

         # Assert
         $logoutResult | Should -BeNullOrEmpty

         ## Check session is not active calling GET /script-executions
         $acualError = $null
         try {
            Invoke-RestMethod `
               -Method GET `
               -Uri "$($connection.APIBasePath)/script-executions" `
               -Headers $connection.Headers `
               -SkipCertificateCheck
         } catch {
            $acualError = $_
         }

         $acualError | Should -Not -Be $null
         $acualError.Exception.Response.StatusCode | Should -Be 401 # Unauthorized
      }

      It 'Logout shuts down all runspaces in maximum 2 minutes' {

         # Arrange
         $connection = $script:srsUser1Connection

         ## Create Runspace 1
         $runspace1 = Invoke-RestMethod `
            -Method POST `
            -Uri "$($connection.APIBasePath)/runspaces" `
            -Headers $connection.Headers `
            -Body (
              @{
                  "name" = "test"
                  "run_vc_connection_script" = $false
               } `
               | ConvertTo-Json
            ) `
            -SkipCertificateCheck

         $runspace1 | Should -Not -Be $null

         ## Create Runspace 1
         $runspace2 = Invoke-RestMethod `
            -Method POST `
            -Uri "$($connection.APIBasePath)/runspaces" `
            -Headers $connection.Headers `
            -Body (
              @{
                  "name" = "test"
                  "run_vc_connection_script" = $false
               } `
               | ConvertTo-Json
            ) `
            -SkipCertificateCheck

         $runspace2 | Should -Not -Be $null

         $openedRunspaces = Invoke-RestMethod `
            -Method GET `
            -Uri "$($connection.APIBasePath)/runspaces" `
            -Headers $connection.Headers `
            -SkipCertificateCheck

         $openedRunspaces.Count | Should -Be 2

         # Act
         $logoutResult = Invoke-RestMethod `
            -Method POST `
            -Uri "$($connection.APIBasePath)/auth/logout" `
            -Headers $connection.Headers `
            -SkipCertificateCheck

         # Assert

         ## Login again, same user different session

         ## Connect SRS API
         $connection = Connect-SrsWithSignAuthorization `
               -SRSIPAddress $SRSIPAddress `
               -vcServer $VCIPAddress `
               -username $VCUser1 `
               -password $VCUser1Password

         ## Wait 2 minutes and check runspaces are shut down
         Start-Sleep -Seconds 120

         ## Retrieve runspaces
         $getRunspaceResult = Invoke-RestMethod `
            -Method GET `
            -Uri "$($connection.APIBasePath)/runspaces" `
            -Headers $connection.Headers `
            -SkipCertificateCheck

         $getRunspaceResult | Should -BeNullOrEmpty
      }
   }
}
