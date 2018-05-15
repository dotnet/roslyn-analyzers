@echo off
powershell -ExecutionPolicy ByPass -NoProfile %~dp0build\Build.ps1 -restore %*
exit /b %ErrorLevel%
