[CmdletBinding(PositionalBinding=$false)]
Param(
    [String] $baseline, # folder that contains the baseline results
    [String] $results,   # folder that contains the performance results
    [switch] $ci        # the scripts is running on a CI server
  )

$currentLocation = Get-Location
try {

    #Get result files
    $baselineFolder = Join-Path $baseline "results"
    
    $resultsFolder = Join-Path $results "results"

    Write-Host "Comparing performance results baseline: '$baselineFolder' "
    Write-Host " - baseline: '$baselineFolder' "
    Write-Host " - results: '$resultsFolder' "
    
    $RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
    $perfDiff = Join-Path $RepoRoot "src\Tools\PerfDiff\PerfDiff.csproj"
    $global:LASTEXITCODE = 0
    Invoke-Expression "dotnet run -c Release --project $perfDiff -- --baseline $baselineFolder --results $resultsFolder --failOnRegression --verbosity diag"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Performance run failed"
        exit $LASTEXITCODE
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
    Set-Location $currentLocation
}