@echo off
powershell -ExecutionPolicy ByPass -command "& """%~dp0eng\common\Build.ps1""" -restore %*"
exit /b %ErrorLevel%
