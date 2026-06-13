@echo off

echo Running EF migrations...
powershell -ExecutionPolicy Bypass -File "%~dp0migrate.ps1"
if errorlevel 1 (
    echo Migration failed. Aborting startup.
    exit /b 1
)

start "TableNow - BackEnd" dotnet run --project server\src\Api\CM.TableNow.Api.csproj

pushd client
ng serve
popd
