string nuspecFile = Args[0];
string assetsDir = Args[1];
string projectDir = Args[2];
string tfm = Args[3];
var metadataList = Args[4].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var fileList = Args[5].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var assemblyList = Args[6].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var dependencyList = Args[7].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var rulesetsDir = Args[8];
var legacyRulesets = Args[9].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

var result = new StringBuilder();

result.AppendLine(@"<?xml version=""1.0""?>");
result.AppendLine(@"<package xmlns=""http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd"">");
result.AppendLine(@"  <metadata>");

string version = null;
string repositoryType = null;
string repositoryUrl = null;
string repositoryCommit = null;

foreach (string entry in metadataList)
{
    int equals = entry.IndexOf('=');
    string name = entry.Substring(0, equals);
    string value = entry.Substring(equals + 1);
    switch (name)
    {
        case "repositoryType": repositoryType = value; continue;
        case "repositoryUrl": repositoryUrl = value; continue;
        case "repositoryCommit": repositoryCommit = value; continue;
    }
    
    if (value != "")
    {
        result.AppendLine($"    <{name}>{value}</{name}>");
    }

    if (name == "version")
    {
        version = value;
    }
}

if (!string.IsNullOrEmpty(repositoryType))
{
    result.AppendLine($@"    <repository type=""{repositoryType}"" url=""{repositoryUrl}"" commit=""{repositoryCommit}""/>");
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

if (fileList.Length > 0 || assemblyList.Length > 0)
{
    const string csName = "CSharp";
    const string vbName = "VisualBasic";
    const string csTarget = @"analyzers\dotnet\cs";
    const string vbTarget = @"analyzers\dotnet\vb";
    const string agnosticTarget = @"analyzers\dotnet";

    var allTargets = new List<string>();
    if (assemblyList.Any(assembly => assembly.Contains(csName)))
    {
        allTargets.Add(csTarget);
    }

    if (assemblyList.Any(assembly => assembly.Contains(vbName)))
    {
        allTargets.Add(vbTarget);
    }

    if (allTargets.Count == 0)
    {
        allTargets.Add(agnosticTarget);
    }

    foreach (string assembly in assemblyList) 
    {
        IEnumerable<string> targets;

        if (assembly.Contains(csName))
        {
            targets = new[] { csTarget };
        }
        else if (assembly.Contains(vbName))
        {
            targets = new[] { vbTarget };
        }
        else
        {
            targets = allTargets;
        }

        string path = Path.Combine(Path.GetFileNameWithoutExtension(assembly), tfm, assembly);

        foreach (string target in targets)
        {
            result.AppendLine(FileElement(path, target));
        }
    }

    foreach (string file in fileList)
    {
        result.AppendLine(FileElement(Path.Combine(projectDir, file), "build"));
    }

    result.AppendLine(FileElement(Path.Combine(assetsDir, "Install.ps1"), "tools"));
    result.AppendLine(FileElement(Path.Combine(assetsDir, "Uninstall.ps1"), "tools"));
}

if (rulesetsDir.Length > 0 && Directory.Exists(rulesetsDir))
{
    foreach (string ruleset in Directory.EnumerateFiles(rulesetsDir))
    {
        if (Path.GetExtension(ruleset) == ".ruleset")
        {
            result.AppendLine(FileElement(Path.Combine(rulesetsDir, ruleset), "rulesets"));
        }
    }
}

if (legacyRulesets.Length > 0)
{
    foreach (string legacyRuleset in legacyRulesets)
    {
        if (Path.GetExtension(legacyRuleset) == ".ruleset")
        {
            result.AppendLine(FileElement(Path.Combine(projectDir, legacyRuleset), @"rulesets\legacy"));
        }
    }
}

result.AppendLine(FileElement(Path.Combine(assetsDir, "ThirdPartyNotices.rtf"), ""));
result.AppendLine(@"  </files>");

result.AppendLine(@"</package>");

File.WriteAllText(nuspecFile, result.ToString());