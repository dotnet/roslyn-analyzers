<#
.SYNOPSIS
Performs any post-MSBuild cleanup needed on the build systems.

.EXAMPLE
C:\PS> .\PostBuild.ps1
#>

#MAIN BODY

$killVBCSCompilerError = $false
try
{
    Stop-Process -Name "vbcscompiler" -Force -ErrorAction SilentlyContinue
}
catch [exception] 
{
    Write-Error -Exception $_.Exception
    $killVBCSCompilerError = $true
}

if ($killVBCSCompilerError)
{
    exit 1
}