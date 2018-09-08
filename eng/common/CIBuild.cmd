@echo off
powershell -ExecutionPolicy ByPass -command "& """%~dp0Build.ps1""" -restore -build -test -sign -pack -publish -ci %*"
exit /b %ErrorLevel%
