[CmdletBinding(PositionalBinding=$false)]
Param(
    [String] $baseline, # folder that contains the baseline results
    [String] $results   # folder that contains the performance results
  )
  
function EnsureFolder {
param (
    [String] $path
)
    If(!(test-path $path))
    {
        New-Item -ItemType Directory -Force -Path $path
    }
}

$currentLocation = Get-Location
try {
    
    #Get result files
    $baselineFolder = Join-Path $baseline "results"
    
    $resultsFolder = Join-Path $results "results"

    $RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
    $perfDiff = Join-Path $RepoRoot "src\Tools\PerfDiff\PerfDiff.csproj"
    Invoke-Expression "dotnet run -c Release --project $perfDiff -- --baseline $baselineFolder --results $resultsFolder --failOnRegression --verbosity diag"
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    $host.SetShouldExit(1)
    exit
}
finally {
    Set-Location $currentLocation
}