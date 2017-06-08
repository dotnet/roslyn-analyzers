string nuspecFile = Args[0];
string assetsDir = Args[1];
var metadataList = Args[2].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var fileList = Args[3].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var dependencyList = Args[4].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

var result = new StringBuilder();

result.AppendLine(@"<?xml version=""1.0""?>");
result.AppendLine(@"<package xmlns=""http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd"">");
result.AppendLine(@"  <metadata>");

string version = null;

foreach (string entry in metadataList)
{
    int equals = entry.IndexOf('=');
    string name = entry.Substring(0, equals);
    string value = entry.Substring(equals + 1);
    if (value != "")
    {
        result.AppendLine($"    <{name}>{value}</{name}>");
    }

    if (name == "version")
    {
        version = value;
    }
}

if (dependencyList.Length > 0)
{
    result.AppendLine(@"    <dependencies>");

    foreach (var dependency in dependencyList)
    {
        result.AppendLine($@"      <dependency id=""{dependency}"" version=""{version}"" />");
    }

    result.AppendLine(@"    </dependencies>");
}

result.AppendLine(@"  </metadata>");

result.AppendLine(@"  <files>");

string FileElement(string file, string target) => $@"    <file src=""{file}"" target=""{target}""/>";

if (fileList.Length > 0)
{
    const string csName = "CSharp";
    const string vbName = "VisualBasic";
    const string csTarget = @"analyzers\dotnet\cs";
    const string vbTarget = @"analyzers\dotnet\vb";
    const string agnosticTarget = @"analyzers\dotnet";


    var allTargets = new List<string>();
    if (fileList.Any(file => file.Contains(csName)))
    {
        allTargets.Add(csTarget);
    }

    if (fileList.Any(file => file.Contains(vbName)))
    {
        allTargets.Add(vbTarget);
    }

    if (allTargets.Count == 0)
    {
        allTargets.Add(agnosticTarget);
    }

    foreach (string file in fileList)
    {
        IEnumerable<string> targets;

        if (Path.GetExtension(file) == ".props")
        {
            targets = new[] { "build" };
        }
        else if (file.Contains(csName))
        {
            targets = new[] { csTarget };
        }
        else if (file.Contains(vbName))
        {
            targets = new[] { vbTarget };
        }
        else
        {
            targets = allTargets;
        }

        foreach (string target in targets)
        {
            result.AppendLine(FileElement(file, target));
        }
    }

    result.AppendLine(FileElement(Path.Combine(assetsDir, "Install.ps1"), "tools"));
    result.AppendLine(FileElement(Path.Combine(assetsDir, "Uninstall.ps1"), "tools"));
}

result.AppendLine(FileElement(Path.Combine(assetsDir, "ThirdPartyNotices.rtf"), ""));
result.AppendLine(@"  </files>");

result.AppendLine(@"</package>");

File.WriteAllText(nuspecFile, result.ToString());