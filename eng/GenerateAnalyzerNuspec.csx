string nuspecFile = Args[0];
string assetsDir = Args[1];
string projectDir = Args[2];
string configuration = Args[3];
string tfm = Args[4];
var metadataList = Args[5].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var fileList = Args[6].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var folderList = Args[7].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var assemblyList = Args[8].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var dependencyList = Args[9].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var libraryList = Args[10].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var rulesetsDir = Args[11];
var legacyRulesets = Args[12].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
var artifactsBinDir = Args[13];
var analyzerDocumentationFileDir = Args[14];
var analyzerDocumentationFileName = Args[15];
var analyzerSarifFileDir = Args[16];
var analyzerSarifFileName = Args[17];
var analyzerConfigurationFileDir = Args[18];
var analyzerConfigurationFileName = Args[19];

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
		case "license": result.AppendLine($"    <license type=\"expression\">{value}</license>"); continue;
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

if (fileList.Length > 0 || assemblyList.Length > 0 || libraryList.Length > 0)
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

        string assemblyNameWithoutExtension = Path.GetFileNameWithoutExtension(assembly);
        string assemblyFolder = Path.Combine(artifactsBinDir, assemblyNameWithoutExtension, configuration, tfm);
        string assemblyPathForNuspec = Path.Combine(assemblyNameWithoutExtension, configuration, tfm, assembly);

        foreach (string target in targets)
        {
            result.AppendLine(FileElement(assemblyPathForNuspec, target));

            if (Directory.Exists(assemblyFolder))
            {
                string resourceAssemblyName = assemblyNameWithoutExtension + ".resources.dll";
                foreach (var directory in Directory.EnumerateDirectories(assemblyFolder))
                {
                    var resourceAssemblyFullPath = Path.Combine(directory, resourceAssemblyName);
                    if (File.Exists(resourceAssemblyFullPath))
                    {
                        var directoryName = Path.GetFileName(directory);
                        string resourceAssemblyPathForNuspec = Path.Combine(assemblyNameWithoutExtension, configuration, tfm, directoryName, resourceAssemblyName);
                        string targetForNuspec = Path.Combine(target, directoryName);
                        result.AppendLine(FileElement(resourceAssemblyPathForNuspec, targetForNuspec));
                    }
                }
            }
        }
    }

    foreach (string file in fileList)
    {
        var fileWithPath = Path.IsPathRooted(file) ? file : Path.Combine(projectDir, file);
        result.AppendLine(FileElement(fileWithPath, "build"));
    }

    foreach (string file in libraryList)
    {
        var fileWithPath = Path.Combine(artifactsBinDir, Path.GetFileNameWithoutExtension(file), configuration, tfm, file);
        result.AppendLine(FileElement(fileWithPath, Path.Combine("lib", tfm)));
    }

    foreach (string folder in folderList)
    {
        string folderPath = Path.Combine(artifactsBinDir, folder, configuration, tfm);
        foreach (var file in Directory.EnumerateFiles(folderPath))
        {
            var fileExtension = Path.GetExtension(file);
            if (fileExtension == ".exe" ||
                fileExtension == ".dll" ||
                fileExtension == ".config")
            {
                var fileWithPath = Path.Combine(folderPath, file);
                result.AppendLine(FileElement(fileWithPath, folder));
            }
        }
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

if (analyzerDocumentationFileDir.Length > 0 && Directory.Exists(analyzerDocumentationFileDir) && analyzerDocumentationFileName.Length > 0)
{
    var fileWithPath = Path.Combine(analyzerDocumentationFileDir, analyzerDocumentationFileName);
    if (File.Exists(fileWithPath))
    {
        result.AppendLine(FileElement(fileWithPath, "documentation"));
    }
}

if (analyzerSarifFileDir.Length > 0 && Directory.Exists(analyzerSarifFileDir) && analyzerSarifFileName.Length > 0)
{
    var fileWithPath = Path.Combine(analyzerSarifFileDir, analyzerSarifFileName);
    if (File.Exists(fileWithPath))
    {
        result.AppendLine(FileElement(fileWithPath, "documentation"));
    }
}

if (analyzerConfigurationFileDir.Length > 0 && Directory.Exists(analyzerConfigurationFileDir) && analyzerConfigurationFileName.Length > 0)
{
    var fileWithPath = Path.Combine(analyzerConfigurationFileDir, analyzerConfigurationFileName);
    if (File.Exists(fileWithPath))
    {
        result.AppendLine(FileElement(fileWithPath, "documentation"));
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