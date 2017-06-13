@echo off
powershell -ExecutionPolicy ByPass %~dp0build\Build.ps1 -restore -build %*
exit /b %ErrorLevel%
