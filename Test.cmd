@echo off
powershell -ExecutionPolicy ByPass %~dp0build\Build.ps1 -test %*
exit /b %ErrorLevel%