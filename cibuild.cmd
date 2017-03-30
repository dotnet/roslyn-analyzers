@echo off
@setlocal enabledelayedexpansion

REM Parse Arguments.

set AnalyzersRoot=%~dp0
set BuildConfiguration=Debug
:ParseArguments
if "%1" == "" goto :DoneParsing
if /I "%1" == "/?" call :Usage && exit /b 1
if /I "%1" == "/debug" set BuildConfiguration=Debug&&shift&& goto :ParseArguments
if /I "%1" == "/release" set BuildConfiguration=Release&&shift&& goto :ParseArguments
call :Usage && exit /b 1
:DoneParsing

REM Prefer building with Dev15 and try the simple route first (we may be running from a DevCmdPrompt already)
set CommonToolsDir=%VS150COMNTOOLS%
if not exist "%CommonToolsDir%" set CommonToolsDir=%VS140COMNTOOLS%
call "%CommonToolsDir%VsDevCmd.bat"

msbuild /v:m /m %AnalyzersRoot%\BuildAndTest.proj /p:CIBuild=true /p:Configuration=%BuildConfiguration%

if ERRORLEVEL 1 (
    taskkill /F /IM vbcscompiler.exe
    echo Build failed
    exit /b 1
)

REM Kill any instances of VBCSCompiler.exe to release locked files;
REM otherwise future CI runs may fail while trying to delete those files.
taskkill /F /IM vbcscompiler.exe

REM It is okay and expected for taskkill to fail (it's a cleanup routine).  Ensure
REM caller sees successful exit.
exit /b 0

:Usage
@echo Usage: cibuild.cmd [/debug^|/release]
@echo   /debug 	Perform debug build.  This is the default.
@echo   /release Perform release build
@goto :eof