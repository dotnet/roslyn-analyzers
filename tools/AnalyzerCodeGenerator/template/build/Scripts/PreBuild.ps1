<#
.SYNOPSIS
Performs any pre-MSBuild cleanup needed on the build systems.

.EXAMPLE
C:\PS> .\PreBuild.ps1
#>

#MAIN BODY

& "$PSScriptRoot\..\..\src\.nuget\NuGetRestore.ps1"