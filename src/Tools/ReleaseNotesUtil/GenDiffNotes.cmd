@echo off
REM GenDiffNotes.cmd scratchDir nugetSource fxCopAnalyzersOldVersion fxCopAnalyzersNewVersion
REM Ex: GenDiffNotes.cmd C:\scratch nuget.org 2.6.3 2.9.0

if [%1]==[] goto usage
if [%2]==[] goto usage
if [%3]==[] goto usage
if [%4]==[] goto usage

set SCRATCHDIR=%1
set NUGETSOURCE=%2
set OLDVERSION=%3
set NEWVERSION=%4

set DIFFNOTES=%SCRATCHDIR%\%OLDVERSION%_%NEWVERSION%-notes.md

@echo on
mkdir %SCRATCHDIR%
pushd %SCRATCHDIR%
nuget.exe install -source %NUGETSOURCE% Microsoft.CodeAnalysis.FxCopAnalyzers -version %OLDVERSION%
nuget.exe install -source %NUGETSOURCE% Microsoft.CodeAnalysis.FxCopAnalyzers -version %NEWVERSION%
popd
dotnet.exe ReleaseNotesUtil.dll getrulesjson %SCRATCHDIR% %OLDVERSION% %SCRATCHDIR%\%OLDVERSION%-rules.json
dotnet.exe ReleaseNotesUtil.dll getrulesjson %SCRATCHDIR% %NEWVERSION% %SCRATCHDIR%\%NEWVERSION%-rules.json
dotnet.exe ReleaseNotesUtil.dll diffrules %SCRATCHDIR%\%OLDVERSION%-rules.json %SCRATCHDIR%\%NEWVERSION%-rules.json %DIFFNOTES%
@echo off

echo.
echo.

if exist "%DIFFNOTES%" (
    echo Added/Removed/Changed notes in %DIFFNOTES%
) else (
    echo Guess something went wrong
)

goto :eof

:usage
echo Usage: %0 scratchDir nugetSource fxCopAnalyzersOldVersion fxCopAnalyzersNewVersion
echo Example: %0 C:\scratch nuget.org 2.6.3 2.9.0