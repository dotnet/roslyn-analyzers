[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $SourceXlf,
    [Parameter(Mandatory=$true)]
    [string]
    $DestinationXlf
)

$destinationFile = Get-ChildItem $DestinationXlf;
[xml] $source = Get-Content $SourceXlf;
[xml] $destination = Get-Content $DestinationXlf;
$destination.xliff.file.body.ChildNodes | ForEach-Object {
    $destinationTransUnit = $_;
    $sourceTransUnit = $source.xliff.file.body.'trans-unit' | Where-Object { $_.id -eq $destinationTransUnit.Id };
    if ($sourceTransUnit -ne $null)
    {
        $destinationTransUnit.source = $sourceTransUnit.source;
        $destinationTransUnit.target.state = $sourceTransUnit.target.state;
        $destinationTransUnit.target.InnerText = $sourceTransUnit.target.InnerText;
    }
};

$s = New-Object -TypeName System.Xml.XmlWriterSettings;
$s.Indent = $true;
$w = [System.Xml.XmlWriter]::Create($destinationFile.FullName, $s);
$destination.WriteContentTo($w);
$w.Close();
