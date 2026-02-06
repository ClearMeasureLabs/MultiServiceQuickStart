@echo off
REM Simple build script for ClearHostedService solution
REM For more options, use build.ps1

setlocal enabledelayedexpansion

set CONFIGURATION=Release
set SKIP_TESTS=0
set PACK=0
set PACKAGE_OUTPUT=artifacts\packages

REM Parse arguments
:parse_args
if "%~1"=="" goto end_parse
if /i "%~1"=="Debug" set CONFIGURATION=Debug
if /i "%~1"=="Release" set CONFIGURATION=Release
if /i "%~1"=="--skip-tests" set SKIP_TESTS=1
if /i "%~1"=="-s" set SKIP_TESTS=1
if /i "%~1"=="--pack" set PACK=1
if /i "%~1"=="-p" set PACK=1
shift
goto parse_args
:end_parse

echo.
echo ========================================
echo   ClearHostedService Build Script
echo ========================================
echo.
echo Configuration: %CONFIGURATION%
echo.

REM Check for .NET SDK
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: .NET SDK not found. Please install .NET 10 SDK or later.
    exit /b 1
)

echo [1/3] Restoring packages...
dotnet restore ClearHostedService\ClearMeasure.HostedService.csproj
if %ERRORLEVEL% neq 0 goto error
dotnet restore ClearHostedEndpoint\ClearMeasure.HostedEndpoint.csproj
if %ERRORLEVEL% neq 0 goto error
dotnet restore ClearHostedEndpoint.SqlServerTransport\ClearMeasure.HostedEndpoint.SqlServerTransport.csproj
if %ERRORLEVEL% neq 0 goto error
dotnet restore ClearHostedService.Tests\ClearHostedService.Tests.csproj
if %ERRORLEVEL% neq 0 goto error
dotnet restore ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj
if %ERRORLEVEL% neq 0 goto error

echo.
echo [2/3] Building projects (%CONFIGURATION%)...
dotnet build ClearHostedService\ClearMeasure.HostedService.csproj --configuration %CONFIGURATION% --no-restore
if %ERRORLEVEL% neq 0 goto error
dotnet build ClearHostedEndpoint\ClearMeasure.HostedEndpoint.csproj --configuration %CONFIGURATION% --no-restore
if %ERRORLEVEL% neq 0 goto error
dotnet build ClearHostedEndpoint.SqlServerTransport\ClearMeasure.HostedEndpoint.SqlServerTransport.csproj --configuration %CONFIGURATION% --no-restore
if %ERRORLEVEL% neq 0 goto error
dotnet build ClearHostedService.Tests\ClearHostedService.Tests.csproj --configuration %CONFIGURATION% --no-restore
if %ERRORLEVEL% neq 0 goto error
dotnet build ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj --configuration %CONFIGURATION% --no-restore
if %ERRORLEVEL% neq 0 goto error

if %SKIP_TESTS%==1 (
    echo.
    echo [3/3] Tests skipped
    goto success
)


echo.
echo [3/3] Running tests...
dotnet test ClearHostedService.Tests\ClearHostedService.Tests.csproj --configuration %CONFIGURATION% --no-build --no-restore --logger "console;verbosity=normal"
if %ERRORLEVEL% neq 0 goto error
dotnet test ClearHostedEndpoint.Tests\ClearHostedEndpoint.Tests.csproj --configuration %CONFIGURATION% --no-build --no-restore --logger "console;verbosity=normal"
if %ERRORLEVEL% neq 0 goto error

:pack_check
if %PACK%==0 goto success

echo.
echo [4/4] Creating NuGet packages...
if not exist %PACKAGE_OUTPUT% mkdir %PACKAGE_OUTPUT%

echo Packing ClearMeasure.HostedService...
dotnet pack ClearHostedService\ClearMeasure.HostedService.csproj --configuration %CONFIGURATION% --no-build --no-restore --output %PACKAGE_OUTPUT%
if %ERRORLEVEL% neq 0 goto error

echo Packing ClearMeasure.HostedEndpoint...
dotnet pack ClearHostedEndpoint\ClearMeasure.HostedEndpoint.csproj --configuration %CONFIGURATION% --no-build --no-restore --output %PACKAGE_OUTPUT%
if %ERRORLEVEL% neq 0 goto error

echo Packing ClearMeasure.HostedEndpoint.SqlServerTransport...
dotnet pack ClearHostedEndpoint.SqlServerTransport\ClearMeasure.HostedEndpoint.SqlServerTransport.csproj --configuration %CONFIGURATION% --no-build --no-restore --output %PACKAGE_OUTPUT%
if %ERRORLEVEL% neq 0 goto error

echo Packages created in: %PACKAGE_OUTPUT%

:success
echo.
echo ========================================
echo   BUILD SUCCESSFUL
echo ========================================
echo.
exit /b 0

:error
echo.
echo ========================================
echo   BUILD FAILED
echo ========================================
echo.
exit /b 1
