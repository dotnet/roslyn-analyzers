@echo off
powershell -ExecutionPolicy ByPass %~dp0build\Build.ps1 -restore %*
exit /b %ErrorLevel%
