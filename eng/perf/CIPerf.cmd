@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0PerfCore.ps1""" -v diag -diff -ci %*"
exit /b %ErrorLevel%