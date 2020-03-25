# This works in PowerShell Core.

# To copy localization from the master branch to the 2.9.x branch, run this script from a 2.9.x-derived branch, providing a 
# $SourceRepo path to another clone of the repository checked out to master.

[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $SourceRepo
)

# Assuming this script lives in src\Tools\Scripts
$destinationRepo = [System.IO.Path]::Combine($PSScriptRoot, "..\..\..");

$xlfDirectories = Get-ChildItem -Filter xlf -Directory -Recurse $destinationRepo `
    | ForEach-Object { [System.IO.Path]::Combine($_.Parent, "xlf") }
    | ForEach-Object { [System.IO.Path]::GetRelativePath($destinationRepo, $_) };

$xlfDirectories | ForEach-Object {
    $xlfDirectory = $_;
    $destinationDirectory = [System.IO.Path]::Combine($destinationRepo, $_);
    $sourceDirectory = [System.IO.Path]::Combine($SourceRepo, $_);
    $xlfFiles = Get-ChildItem -Filter *.xlf $destinationDirectory;
    $xlfFiles | ForEach-Object {
        $sourceFile = [System.IO.Path]::Combine($sourceDirectory, $_.Name)
        if (Test-Path $sourceFile)
        {
            & "$PSScriptRoot\CopyXlfTransUnits.ps1" $sourceFile $_.FullName;
        }
        else
        {
            Write-Error -Message "Can't find corresponding $xlfDirectory\$_ under $SourceRoot";
        }
    };
};