$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$output = Join-Path $RepoRoot "artifacts\performance\perfResults"
Start-Process -Wait -FilePath "dotnet" -Verb RunAs -ArgumentList "run -c Release --project Tests\PerformanceTests.csproj -- --memory --exporters JSON --profiler ETW --artifacts $output --filter *"