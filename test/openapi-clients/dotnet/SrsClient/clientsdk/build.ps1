$currentPath = $PSScriptRoot

$osType = Get-ComputerInfo | Select-Object -ExpandProperty OsType
$dotnetInstallScript = 'dotnet-install.ps1'
$dotnetInstallScriptRemoteLocation = 'https://dot.net/v1/dotnet-install.ps1'
$sln = (Join-Path $currentPath "IO.Swagger.sln")

Write-Host ''
Write-Host "******************************************************" -ForegroundColor Green
Write-Host "[INFO] Install dotnet CLI" -ForegroundColor Green
Write-Host "******************************************************" -ForegroundColor Green
Write-Host ''

if ($osType -ne 'WINNT') {
   $dotnetInstallScript = 'dotnet-install.sh'
   $dotnetInstallScriptRemoteLocation = 'https://dot.net/v1/dotnet-install.sh'
}
$dotnetInstallScriptLocalLocation = (Join-Path $currentPath "bin\$dotnetInstallScript")

if (!(Test-Path $dotnetInstallScriptLocalLocation)) {
   Invoke-WebRequest $dotnetInstallScriptRemoteLocation -outfile $dotnetInstallScriptLocalLocation
}

if ($osType -ne 'WINNT') {
   . $dotnetInstallScriptLocalLocation --channel LTS --quality GA --runtime dotnet
} else {
   . $dotnetInstallScriptLocalLocation -Channel LTS -Quality GA -Runtime dotnet
}

Write-Host ''
Write-Host "******************************************************" -ForegroundColor Green
Write-Host "[INFO] Running tests" -ForegroundColor Green
Write-Host "******************************************************" -ForegroundColor Green
Write-Host ''

dotnet test $sln

Write-Host ''
Write-Host "******************************************************" -ForegroundColor Green
Write-Host "[INFO] Packaging client SDK" -ForegroundColor Green
Write-Host "******************************************************" -ForegroundColor Green
Write-Host ''

dotnet pack -c Release $sln