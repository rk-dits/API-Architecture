@echo off
echo Running Identity Service Test Suite...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0run-tests.ps1" -TestType All -Coverage -Verbose

echo.
echo Press any key to exit...
pause > nul