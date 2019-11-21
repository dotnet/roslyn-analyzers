[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $xlfFileName
)

$f = Get-ChildItem $xlfFileName;
[xml] $x = Get-Content $xlfFileName;
$body = $x.xliff.file.body;
$sorted = $body.ChildNodes | Sort-Object -Property id | ForEach-Object { $_.CloneNode($true) };
$body.RemoveAll();
$sorted | ForEach-Object { $body.AppendChild($_) | Out-Null };

$utf8 = [System.Text.Encoding]::UTF8;
$w = New-Object -TypeName System.Xml.XmlTextWriter -ArgumentList $f.FullName,$utf8;
$w.Formatting = [System.Xml.Formatting]::Indented;
$w.Indentation = 2;
$x.WriteContentTo($w);
$w.Close();
