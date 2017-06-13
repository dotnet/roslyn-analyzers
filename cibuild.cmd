@echo off
powershell -ExecutionPolicy ByPass %~dp0build\Build.ps1 -restore -build -test -sign -pack -ci %*
exit /b %ErrorLevel%
