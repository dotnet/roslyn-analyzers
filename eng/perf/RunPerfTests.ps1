[CmdletBinding(PositionalBinding=$false)]
Param(
    [string] $projects,           # semicolon separated list of relative paths to benchmark projects to run
    [string] $filter,             # filter for tests to run (supports wildcards)
    [String] $perftestRootFolder, # root folder all of the  benchmark projects share
    [String] $output,             # folder to write the benchmark results to
    [bool]   $etl=$false,         # capture etl traces for performance tests
    [bool]   $ci=$false           # run in ci mode (fail fast an keep all partial artifacts)
  )

Push-Location $perftestRootFolder
try {
    $Build = Join-Path $perftestRootFolder "eng\common\build.ps1"
    Write-Host "Restoring repo at '$perftestRootFolder'"
    & $Build -restore -configuration release -prepareMachine /p:Coverage=false
    Write-Host "Restore complete"
    $projectsList = $projects -split ";"
    foreach ($project in $projectsList){
        $projectFullPath = Join-Path $perftestRootFolder $project
        & dotnet build -c Release --no-incremental $projectFullPath
        $comandArguments = "run -c Release --no-build --project $projectFullPath -- --warmupCount  2 --invocationCount 1 --runOncePerIteration --memory --exporters JSON --artifacts $output"
        if ($ci) {
            $comandArguments = "$comandArguments --stopOnFirstError --keepFiles"
        }
        if ($etl) {
            Write-Host "Running tests in project '$projectFullPath'"
            Start-Process -Wait -FilePath "dotnet" -Verb RunAs -ArgumentList "$comandArguments --profiler ETW --filter $filter"
        }
        else {
            Write-Host "Running tests in project '$projectFullPath'"
            Invoke-Expression "dotnet $comandArguments --filter $filter"
        }
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    $host.SetShouldExit(1)
    exit 1
}
finally {
    Pop-Location
}