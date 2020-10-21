@echo off
:: Path to swagger-codeget-cli e.g. "c:\git-repos\swagger-codegen\modules\swagger-codegen-cli\target\swagger-codegen-cli.jar"
SET SWAGGER_CODEGEN_CLI=%1
SET INPUT_SWAGGER_PATH=%2
SET OUTPUT_PATH=%3

:: End command should look like
:: java -jar c:\git-repos\swagger-codegen\modules\swagger-codegen-cli\target\swagger-codegen-cli.jar generate -i .\swagger.json -l csharp -o .\csharp

java -jar %SWAGGER_CODEGEN_CLI% generate -i %INPUT_SWAGGER_PATH% -l csharp -o %OUTPUT_PATH%

