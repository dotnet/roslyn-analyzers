@echo off
powershell.exe -ExecutionPolicy ByPass -NoProfile -NoLogo -File "perf.ps1"
exit /b %ErrorLevel%