[CmdletBinding(PositionalBinding=$false)]
Param(
    [String] $baselineSHA, # git SHA to use as the baseline for performance
    [String] $output,      # common folder to write the benchmark results to
    [string] $projects,    # semicolon separated list of relative paths to benchmark projects to run
    [string] $filter,      # filter for tests to run (supports wildcards)
    [bool] $etl = $false,  # capture etl traces for performance tests
    [bool] $ci = $false    # run in ci mode (fail fast an keep all partial artifacts)
  )

function EnsureFolder {
    param (
        [String] $path # path to create if it does not exist
    )
    If(!(test-path $path))
    {
        New-Item -ItemType Directory -Force -Path $path
    }
}
 
# Setup paths
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$RunPerfTests = Join-Path $PSScriptRoot "RunPerfTests.ps1"
$ComparePerfResults = Join-Path $PSScriptRoot "ComparePerfResults.ps1"
$Temp = Join-Path $RepoRoot "temp"

try {  
    # Get baselline results
    Write-Host "Running Baseline Tests"
    
    # Ensure output directory has been created
    EnsureFolder Join-Path $output "baseline"
    $resultsOutput = Join-Path $output "baseline"
    
    # Checkout SHA
    $baselineFolder = Join-Path $Temp "perfBaseline"
    Invoke-Expression "git worktree add $baselineFolder $baselineSHA"
    
    $baselineCommandArgs = @{
        perftestRootFolder = $baselineFolder
        projects = $projects
        output = $resultsOutput
        filter = $filter
    }
    if ($etl) { $baselineCommandArgs.etl = $True }
    if ($ci) { $baselineCommandArgs.ci =  $True}
    
    & $RunPerfTests @baselineCommandArgs

    Write-Host "Done with baseline run"    
    
    # Ensure output directory has been created
    EnsureFolder Join-Path $output "perfTest"
    $testOutput = Join-Path $output "perfTest"
    
    $commandArgs = @{
        perftestRootFolder = $RepoRoot
        projects = $projects
        output = $testOutput
        filter = $filter
    }
    if ($etl) { $commandArgs.etl = $True }
    if ($ci) { $commandArgs.ci =  $True}
    
    # Get perf results
    Write-Host "Running performance tests"
    & $RunPerfTests @commandArgs
    Write-Host "Done with performance run"    
    
    # Diff perf results
    if ($ci) {
        & $ComparePerfResults -baseline $resultsOutput -results $testOutput -ci
    }
    else {
        & $ComparePerfResults -baseline $resultsOutput -results $testOutput
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
}