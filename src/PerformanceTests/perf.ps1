$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$output = Join-Path $RepoRoot "artifacts\performance\perfResults"
Start-Process -Wait -FilePath "dotnet" -Verb RunAs -ArgumentList "run -c Release --project CSharp\CSharpPerformanceTests.csproj -- --memory --exporters JSON --profiler ETW --artifacts $output --filter *"
Start-Process -Wait -FilePath "dotnet" -Verb RunAs -ArgumentList "run -c Release --project VisualBasic\VisualBasicPerformanceTests.csproj -- --memory --exporters JSON --profiler ETW --artifacts $output --filter *"