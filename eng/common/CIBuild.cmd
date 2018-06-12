@echo off
powershell -ExecutionPolicy ByPass -command "& """%~dp0Build.ps1""" -restore -build -projects """%~dp0..\GenerateAnalyzerRulesets\GenerateAnalyzerRulesets.csproj""" %*"
powershell -ExecutionPolicy ByPass -command "& """%~dp0Build.ps1""" -restore -build -test -sign -pack -ci %*"
exit /b %ErrorLevel%
