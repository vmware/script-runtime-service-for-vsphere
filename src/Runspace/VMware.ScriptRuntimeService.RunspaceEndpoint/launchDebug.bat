SET mypath=%~dp0
echo %mypath:~0,-1%
cd %mypath%
dotnet run "server.urls=http://127.0.0.1:5550/