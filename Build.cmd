@echo off
powershell -ExecutionPolicy ByPass -command "& """%~dp0eng\common\Build.ps1""" -restore -build %*"
exit /b %ErrorLevel%
