# Script Runtime Service for vSphere API Integration Tests

## Overview
API Integration tests cover SRS API interacting with a SRS deployment.<br/>
The goal of the theses tests is to call API surface without having dependencies to client-side sdk. That's why they performe raw REST invoke over HTTP. <br/>
Tests are implemented as PowerShell Pester test calling PowerShell `Invoke-RestMethod` cmdlet. Payloads are raw json content produced by PowerShell `ConvertTo-Json` cmdlet.<br/>

## Prerequisite
To run integration test you need the following tools installed on the machine tests will run<br/>

* PowerShell 7.0
* Pester 4.8.1
* dotnet sdk 3.1

In addition you need Script Runtime Service deployed with VCenter server<br/>

## Run the tests
`IntegrationTestsLauncher.ps1` script is the entry point to run the tests. It should be called in PowerShell 7.0.<br/>
The first thing the tests launcher scripts does is to build vSphereIntegration .NET libraries from source. It uses dotnet sdk to do this.<br/>
Having the vSphereIntegration .NET libraries built the tests launcher scripts calls `Invoke-Pester` cmdet on the `test` folder where tests scripts are defined.<br/>

### Parameters
* `SRSIPAddress` the address of the Script Runtime Service deployment
* `VCIPAddress` the VC address which Script Runtime Service is registered to
* `VCUser1` the VC user which will be used to run the tests
* `VCUser1Password` the password of VCUser1

### Example
`pwsh -Command "IntegrationTestsLauncher.ps1 -SRSIPAddress '10.160.207.178' -VCIPAddress '10.23.80.205' -VCUser1 'administrator@vsphere.local' -VCUser1Password 'Admin!23'"`