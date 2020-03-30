[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $SourceResx,
    [Parameter(Mandatory=$true)]
    [string]
    $DestinationResx
)

$destinationFile = Get-ChildItem $DestinationResx;
[xml] $source = Get-Content $SourceResx;
[xml] $destination = Get-Content $DestinationResx;
$destination.root.data | ForEach-Object {
    $destinationData = $_;
    $sourceData = $source.root.data | Where-Object { $_.name -eq $destinationData.Name };
    if ($sourceData -ne $null)
    {
        $destText = $destinationData.value;
        $srcText = $sourceData.value;
        if ($destText -ne $srcText `
                -and (($destText.Contains('{') -and $destText.Contains('}') `
                      -or ($srcText.Contains('{') -and $srcText.Contains('}')))))
        {
            Write-Host $destinationData.name;
            Write-Host "  Old: $destText";
            Write-Host "  New: $srcText";
        }

        $destinationData.value = $srcText;
    }
};

$s = New-Object -TypeName System.Xml.XmlWriterSettings;
$s.Indent = $true;
$w = [System.Xml.XmlWriter]::Create($destinationFile.FullName, $s);
$destination.WriteContentTo($w);
$w.Close();
