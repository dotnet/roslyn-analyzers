@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0Build.ps1""" -restore -rebuild -test -sign -pack -publish -ci %*"
exit /b %ErrorLevel%
