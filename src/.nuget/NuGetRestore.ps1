$NuGetExe = "$PSScriptRoot\NuGet.exe"
$NuGetConfig = "$PSScriptRoot\NuGet.config"

& $NuGetExe restore "$PSScriptRoot\..\Analyzers.sln" -configfile "$NuGetConfig"

