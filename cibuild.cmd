@echo off
@setlocal enabledelayedexpansion

REM Parse Arguments.

set AnalyzersRoot=%~dp0
set BuildConfiguration=Debug
set OfficialBuild=false
:ParseArguments
if "%1" == "" goto :DoneParsing
if /I "%1" == "/?" call :Usage && exit /b 1
if /I "%1" == "/debug" set BuildConfiguration=Debug&&shift&& goto :ParseArguments
if /I "%1" == "/release" set BuildConfiguration=Release&&shift&& goto :ParseArguments
if /I "%1" == "/officialbuild" set OfficialBuild=true&&shift&& goto :ParseArguments
call :Usage && exit /b 1
:DoneParsing

if exist "%VS150COMNTOOLS%VsDevCmd.bat" (
    call "%VS150COMNTOOLS%VsDevCmd.bat"
)

REM load Visual Studio 2017 developer command prompt if VS150COMNTOOLS is not set
if "%VS150COMNTOOLS%" EQU "" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat" (
    call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat"
)
if "%VS150COMNTOOLS%" EQU "" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat" (
    call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat"
)
if "%VS150COMNTOOLS%" EQU "" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat" (
    call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
)
if "%VS150COMNTOOLS%" EQU "" if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\Common7\Tools\VsDevCmd.bat" (
    call "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\Common7\Tools\VsDevCmd.bat"
)

msbuild /v:m /m %AnalyzersRoot%\BuildAndTest.proj /p:CIBuild=true /p:Configuration=%BuildConfiguration% /p:OfficialBuild=%OfficialBuild%

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