SET mypath=%~dp0
echo %mypath:~0,-1%
cd %mypath%
dotnet run