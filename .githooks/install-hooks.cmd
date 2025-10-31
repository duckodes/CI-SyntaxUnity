@echo off
echo Installing Git pre-push hook...
SET SCRIPT_DIR=%~dp0
copy "%SCRIPT_DIR%pre-push" "%SCRIPT_DIR%..\.git\hooks\pre-push" >nul
echo Hook installed.