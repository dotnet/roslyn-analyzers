$ErrorActionPreference = 'Stop'

$Projects = @( "$PSScriptRoot\src", "$PSScriptRoot\tools" ) |
Get-ChildItem -Include *.??proj -File -Recurse | 
ForEach-Object {

    $ProjectDir = [System.IO.Path]::GetDirectoryName($_.FullName)
    $ResourceName = [System.IO.Path]::GetFileNameWithoutExtension($_.FullName).Replace('.', '') + "Resources"

    switch -Exact -CaseSensitive ($ResourceName) {
        'MicrosoftCodeAnalysisPerformanceSensitiveAnalyzersResources' { $ResourceName = 'PerformanceSensitiveAnalyzersResources' } 
        'MicrosoftCodeAnalysisCSharpPerformanceSensitiveAnalyzersResources' { $ResourceName = 'AnalyzersResources' } 
        'MetaCompilationAnalyzersResources' { $ResourceName = 'MetaCompilationResources' } 
        'MicrosoftCodeAnalysisDiagnosticsResources' { $ResourceName = 'CodeAnalysisDiagnosticsResources' } 
        'MicrosoftBannedApiAnalyzerResources' { $ResourceName = 'BannedApiAnalyzerResources' } 
        'FileIssuesResources' { $ResourceName = 'Resources' } 
        'MicrosoftCodeAnalysisAnalyzersResources' { $ResourceName = 'CodeAnalysisDiagnosticsResources' } 
        'MicrosoftCodeAnalysisBannedApiAnalyzersResources' { $ResourceName = 'BannedApiAnalyzerResources' } 
        'MicrosoftCodeAnalysisPublicApiAnalyzersResources' { $ResourceName = 'PublicApiAnalyzerResources' } 
    }

    $ResxPath = [System.IO.Path]::Combine($ProjectDir, $ResourceName + '.resx')

    [PSCustomObject]@{
        ProjectPath  = $_.FullName
        ProjectDir   = $ProjectDir
        ResourceName = $ResourceName
        ResxPath     = $ResxPath
        Resources    = Get-ChildItem -Path $ProjectDir -Filter *.resx -Exclude $ResxPath -File -Recurse |
        Where-Object FullName -NE $ResxPath |
        ForEach-Object { 

            [PSCustomObject]@{
                ResxPath          = $_.FullName
                ResourceName      = [System.IO.Path]::GetFileNameWithoutExtension($_.FullName)
                ResourceNamespace = [System.IO.Path]::GetFileName([System.IO.Path]::GetDirectoryName($_.FullName))
            } 
        }
    }
} |
Where-Object { $_.Resources.Count -gt 0 }

$Languages = [string[]]@(
    'cs'
    'de'
    'es'
    'fr'
    'it'
    'ja'
    'ko'
    'pl'
    'pt-BR'
    'ru'
    'tr'
    'zh-Hans'
    'zh-Hant'
)

$EmptyResx = @'
<?xml version="1.0" encoding="utf-8"?>
<root>
  <!-- 
    Microsoft ResX Schema 
    
    Version 2.0
    
    The primary goals of this format is to allow a simple XML format 
    that is mostly human readable. The generation and parsing of the 
    various data types are done through the TypeConverter classes 
    associated with the data types.
    
    Example:
    
    ... ado.net/XML headers & schema ...
    <resheader name="resmimetype">text/microsoft-resx</resheader>
    <resheader name="version">2.0</resheader>
    <resheader name="reader">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
    <resheader name="writer">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
    <data name="Name1"><value>this is my long string</value><comment>this is a comment</comment></data>
    <data name="Color1" type="System.Drawing.Color, System.Drawing">Blue</data>
    <data name="Bitmap1" mimetype="application/x-microsoft.net.object.binary.base64">
        <value>[base64 mime encoded serialized .NET Framework object]</value>
    </data>
    <data name="Icon1" type="System.Drawing.Icon, System.Drawing" mimetype="application/x-microsoft.net.object.bytearray.base64">
        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
        <comment>This is a comment</comment>
    </data>
                
    There are any number of "resheader" rows that contain simple 
    name/value pairs.
    
    Each data row contains a name, and value. The row also contains a 
    type or mimetype. Type corresponds to a .NET class that support 
    text/value conversion through the TypeConverter architecture. 
    Classes that don't support this are serialized and stored with the 
    mimetype set.
    
    The mimetype is used for serialized objects, and tells the 
    ResXResourceReader how to depersist the object. This is currently not 
    extensible. For a given mimetype the value must be set accordingly:
    
    Note - application/x-microsoft.net.object.binary.base64 is the format 
    that the ResXResourceWriter will generate, however the reader can 
    read any of the formats listed below.
    
    mimetype: application/x-microsoft.net.object.binary.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            : and then encoded with base64 encoding.
    
    mimetype: application/x-microsoft.net.object.soap.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter
            : and then encoded with base64 encoding.

    mimetype: application/x-microsoft.net.object.bytearray.base64
    value   : The object must be serialized into a byte array 
            : using a System.ComponentModel.TypeConverter
            : and then encoded with base64 encoding.
    -->
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
</root>
'@

$EmptyXlf = @'
<?xml version="1.0" encoding="utf-8"?>
<xliff xmlns="urn:oasis:names:tc:xliff:document:1.2" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" version="1.2" xsi:schemaLocation="urn:oasis:names:tc:xliff:document:1.2 xliff-core-1.2-transitional.xsd">
  <file datatype="xml" source-language="en" target-language="_" original="../_.resx">
    <body/>
  </file>
</xliff>
'@

$Projects |
ForEach-Object {
    $Project = $_

    $Resxs = $Project.Resources.ResxPath |
    ForEach-Object { [xml](Get-Content -LiteralPath $_ -Raw) }

    $Resources = $Resxs.root.data

    if ($Resources) {

        $Resources |
        Group-Object -Property name |
        Where-Object Count -gt 1 |
        ForEach-Object {
            Write-Error -Message "Repeted resource '$_.name' in project '$($Project.ProjectPath)'" -Category InvalidOperation -ErrorAction Stop
        }

        if (Test-Path -LiteralPath $Project.ResxPath) {
            $Resx = [xml]::new()
            $Resx.Load($Project.ResxPath)
        }
        else {
            $Resx = [xml]$EmptyResx
        }

        $Resources |
        ForEach-Object {
            $Resx.root.AppendChild($Resx.ImportNode($_, $true)) | Out-Null
        }

        $Resx.root.data |
        Group-Object -Property name |
        Where-Object Count -gt 1 |
        ForEach-Object {
            Write-Error -Message "Repeted resource name '$($_.Name)' in resource '$($Project.ResxPath)'" -Category InvalidOperation -ErrorAction Stop
        }

        $Resx.Save($Project.ResxPath)

        $Project.Resources.ResxPath | Remove-Item
    }
    
    $DestinationXlfDir = "$($Project.ProjectDir)\xlf"
    $DestinationXlfBasePath = "$($DestinationXlfDir)\$($Project.ResourceName)"

    New-Item -Path ($DestinationXlfDir) -ItemType Directory -Force:$false -ErrorAction SilentlyContinue | Out-Null

    $Languages |
    ForEach-Object {
        $Language = $_

        $DestinationXlfPath = "$($DestinationXlfBasePath).$($Language).xlf"

        $XlfsPaths = ($Project.Resources |
        ForEach-Object {
            Get-ChildItem -Path "$([System.IO.Path]::GetDirectoryName($_.ResxPath))\xlf" -Filter "$([System.IO.Path]::GetFileNameWithoutExtension($_.ResxPath)).$($Language).xlf" -File -Recurse |
            Where-Object FullName -NE $DestinationXlfPath
        }).FullName

        if ($XlfsPaths) {

            $Xlfs = $XlfsPaths |
            ForEach-Object { 
                [xml](Get-Content -LiteralPath $_ -Raw) 
            }

            if (Test-Path -LiteralPath $DestinationXlfPath) {
                $Xlf = [xml]::new()
                $Xlf.Load($DestinationXlfPath)
            }
            else {
                $Xlf = [xml]$EmptyXlf
                $Xlf.xliff.file.'target-language' = $Language
                $Xlf.xliff.file.original = "../$($Project.ResourceName).resx"
            }

            $TransUnits = $Xlfs.xliff.file.body.'trans-unit'
        
            if ($TransUnits) {
                $body = $Xlf.SelectSingleNode('//*[local-name()="body"]')
                $TransUnits |
                ForEach-Object {
                    $body.AppendChild($Xlf.ImportNode($_, $true)) | Out-Null
                }

                $Xlf.xliff.file.body.'trans-unit' |
                Group-Object -Property id |
                Where-Object Count -gt 1 |
                ForEach-Object {
                    Write-Error -Message "Repeted resource id '$($_.name)' in resource '$($DestinationXlfPath)'" -Category InvalidOperation -ErrorAction Stop
                }
            }

            $Xlf.Save($DestinationXlfPath)

            $XlfsPaths | Remove-Item
        }
    }
}

Get-ChildItem -Path $PSScriptRoot -Filter '*.??' -Include '*.cs', '*.vb' -File -Recurse |
Where-Object { $_.FullName -NotLike '*\obj\*' -and $_.FullName -NotLike '*\bin\*' } |
ForEach-Object {
    $FilePath = $_.FullName

    $source = (Get-Content -LiteralPath $FilePath -Raw)
    $NewSource = $source

    $Projects | 
    ForEach-Object {
        $Project = $_

        $Project.Resources |
        ForEach-Object {
            $Resource = $_

            $NewSource = $NewSource -creplace "(?<![\w]|System\.)$($Resource.ResourceNamespace)\.$($Resource.ResourceName)(?![\w])", $Project.ResourceName
            $NewSource = $NewSource -creplace "(?<![\w]|System\.)$($Resource.ResourceName)(?![\w])", $Project.ResourceName

            if ($Resource.ResourceNamespace -eq 'Resources') {
                $NewSource = $NewSource.Replace("System.$($Project.ResourceName)", 'System.Resources')
            }
        }
    }

    if ($NewSource -ne $source) {
        Set-Content -LiteralPath $FilePath -Value $NewSource -NoNewline
    }
}
