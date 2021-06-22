[CmdletBinding(PositionalBinding=$false)]
Param(
    [String] $baselineSHA, # git SHA to use as the baseline for performance
    [String] $testSHA,     # git SHA to use as the test for performance
    [String] $output,      # common folder to write the benchmark results to
    [string] $projects,    # semicolon separated list of relative paths to benchmark projects to run
    [string] $filter,      # filter for tests to run (supports wildcards)
    [switch] $etl,         # capture etl traces for performance tests
    [switch] $ci           # run in ci mode (fail fast an keep all partial artifacts)
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

function RunTest {
    param (
        [String] $RunPerfTests,   # path to the RunPerfTests.ps1 script
        [String] $output,         # common folder to write the benchmark results to
        [String] $resultsFolder,  # folder name to write the benchmark results to
        [String] $temp,           # root folder to check repos into
        [String] $repoName,       # folder name to place checked-out repo in
        [String] $repoSHA,        # git SHA run performance tests against
        [String] $projects,       # semicolon separated list of relative paths to benchmark projects to run
        [String] $filter,         # filter for tests to run (supports wildcards)
        [bool] $etl,              # capture etl traces for performance tests
        [bool] $ci                # run in ci mode (fail fast an keep all partial artifacts)
    )
    
    # Ensure output directory has been created
    $resultsOutput = Join-Path $output $resultsFolder
    EnsureFolder $resultsOutput
    
    # Checkout SHA
    $repoFolder = Join-Path $Temp $repoName
    Invoke-Expression "git worktree add $repoFolder $repoSHA"
    
    $commandArgs = "-root $repoFolder -output $resultsOutput -projects $projects -filter $filter"
    if ($etl) {
        $commandArgs = "$commandArgs -etl"
    }
    
    if ($ci) {
        $commandArgs = "$commandArgs -ci"
    }
    
    Invoke-Expression "$RunPerfTests $commandArgs"
}
  
$currentLocation = Get-Location

# Setup paths
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$RunPerfTests = Join-Path $PSScriptRoot "RunPerfTests.ps1"
$ComparePerfResults = Join-Path $PSScriptRoot "ComparePerfResults.ps1"
$Temp = Join-Path $RepoRoot "temp"

try {  
    # Get baseline results
    RunTest -RunPerfTests $RunPerfTests -output $output -resultsFolder "baseline" -temp $Temp -repoName "perfBaseline" -repoSHA $baselineSHA -projects $projects -filter $filter -etl $etl.IsPresent -ci $ci.IsPresent
    
    # Get perf results
    RunTest -RunPerfTests $RunPerfTests -output $output -resultsFolder "perfTest" -temp $Temp -repoName "perfTest" -repoSHA $testSHA -projects $projects -filter $filter -etl $etl.IsPresent -ci $ci.IsPresent
    
    # Diff perf results
    Invoke-Expression "$ComparePerfResults -baseline $baselineOutput -results $testOutput"
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    $host.SetShouldExit(1)
    exit
}
finally {
    Invoke-Expression 'git worktree remove perfBaseline'
    Invoke-Expression 'git worktree remove perfTest'
    Invoke-Expression 'git worktree prune'
    Set-Location $currentLocation
}