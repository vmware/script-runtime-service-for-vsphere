$currentPath = $PSScriptRoot

$product = 'IO.Swagger'
$outputDirectory = (Join-Path $currentPath "build")
$osType = Get-ComputerInfo | Select-Object -ExpandProperty OsType
$dotnetInstallScript = 'dotnet-install.ps1'
$dotnetInstallScriptRemoteLocation = 'https://dot.net/v1/dotnet-install.ps1'
$sln = (Join-Path $currentPath "$product.sln")

Write-Host "******************************************************" -ForegroundColor Green
Write-Host "[INFO] Building $product" -ForegroundColor Green
Write-Host "******************************************************" -ForegroundColor Green
Write-Host ''

$dotnetCmd = Get-Command 'dotnet' -ErrorAction SilentlyContinue
$continueBuilding = $false

if (!$dotnetCmd) {
   Write-Host "[WARN] dotnet CLI is missing." -ForegroundColor Yellow
   $response = Read-Host -Prompt "[CONF] Do you want to install dotnet CLI (Yes/Y/No/N): "
   if ($response.ToLower() -eq 'n') {
      Write-Host "[ERRO] dotnet CLI is missing and installation is denied. Unable to proceed." -ForegroundColor Red
   } else {
      Write-Host "[INFO]    Installing dotnet CLI" -ForegroundColor Green
      if ($osType -ne 'WINNT') {
         $dotnetInstallScript = 'dotnet-install.sh'
         $dotnetInstallScriptRemoteLocation = 'https://dot.net/v1/dotnet-install.sh'
      }
      $dotnetInstallScriptLocalLocation = (Join-Path $outputDirectory $dotnetInstallScript)

      Write-Host "[INFO]    Downloading dotnet CLI install script from '$dotnetInstallScriptRemoteLocation' to '$dotnetInstallScriptLocalLocation'" -ForegroundColor Green
      if (!(Test-Path $dotnetInstallScriptLocalLocation)) {
         Invoke-WebRequest $dotnetInstallScriptRemoteLocation -OutFile $dotnetInstallScriptLocalLocation
      }

      Write-Host "[INFO]    Running dotnet CLI install script" -ForegroundColor Green
      if ($osType -ne 'WINNT') {
         . $dotnetInstallScriptLocalLocation --channel LTS --quality GA --runtime dotnet
      } else {
         . $dotnetInstallScriptLocalLocation -Channel LTS -Quality GA -Runtime dotnet
      }

      $continueBuilding = $true
   }
} else {
   $continueBuilding = $true
}

if ($continueBuilding) {
   Write-Host "[INFO] Running $product tests" -ForegroundColor Green

   & dotnet test $sln

   Write-Host "[INFO] Building $product SDK" -ForegroundColor Green

   & dotnet build -c Release $sln -o (Join-Path $outputDirectory 'bin')

   Write-Host "[INFO] Packaging $product SDK" -ForegroundColor Green

   & dotnet pack -c Release $sln -o (Join-Path $outputDirectory 'packages')
}
