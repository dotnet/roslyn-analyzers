[CmdletBinding(PositionalBinding=$false)]
Param(
    [string] $projects, # semicolon separated list of relative paths to benchmark projects to run
    [string] $filter,   # filter for tests to run (supports wildcards)
    [String] $root,     # root folder all of the  benchmark projects share
    [String] $output,   # folder to write the benchmark results to
    [switch] $etl,      # capture etl traces for performance tests
    [switch] $ci        # run in ci mode (fail fast an keep all partial artifacts)
  )

try {
    $projectsList = $projects -split ";"
    foreach ($project in $projectsList){
        $projectFullPath = Join-Path $root $project
        $comandArguments = "run -c Release --project $projectFullPath -- --memory --exporters JSON --artifacts $output"
        if ($ci) {
            $comandArguments = "$comandArguments --stopOnFirstError --keepFiles"
        }
        if ($etl) {
            Start-Process -Wait -FilePath "dotnet" -Verb RunAs -ArgumentList "$comandArguments --profiler ETW --filter $filter"
        }
        else {
            Invoke-Expression "dotnet $comandArguments --filter $filter"
        }
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    $host.SetShouldExit(1)
    exit
}
finally {
}