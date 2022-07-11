@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\perf\PerfCore.ps1""" %*"
exit /b %ErrorLevel%