$NuGetExe = "$PSScriptRoot\NuGet.exe"

& $NuGetExe install xunit.runner.console -version 2.0.0 -OutputDirectory "$PSScriptRoot\..\..\packages"
& $NuGetExe restore "$PSScriptRoot\packages.config" -PackagesDirectory "$PSScriptRoot\..\..\packages"
& $NuGetExe restore "$PSScriptRoot\..\AllAnalyzers.sln" -PackagesDirectory "$PSScriptRoot\..\..\packages"

