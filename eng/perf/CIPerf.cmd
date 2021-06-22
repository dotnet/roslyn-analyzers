@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0PerfCore.ps1""" -v diag -etl -diff %*"
exit /b %ErrorLevel%