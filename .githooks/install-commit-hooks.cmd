@echo off
echo Installing Git pre-commit hook...
SET SCRIPT_DIR=%~dp0
copy "%SCRIPT_DIR%pre-commit" "%SCRIPT_DIR%..\.git\hooks\pre-commit" >nul
echo Hook installed.