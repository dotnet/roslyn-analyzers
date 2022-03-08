<#
.SYNOPSIS
Downloads 32-bit and 64-bit procdump executables and returns the path to where they were installed.
eng\scripts\Get-ProcDump.ps1
#>
. $PSScriptRoot/../common/tools.ps1
$procDumpDir = Join-Path $ToolsDir "procdump"
$procDumpToolPath = Join-Path $procDumpDir "procdump.exe"
if (-not (Test-Path $procDumpToolPath)) {
    Invoke-WebRequest -Uri https://download.sysinternals.com/files/Procdump.zip -UseBasicParsing -OutFile $TempDir\Procdump.zip | Out-Null
    Expand-Archive -Path $TempDir\Procdump.zip $procDumpDir
}
(Resolve-Path $procDumpDir).Path
