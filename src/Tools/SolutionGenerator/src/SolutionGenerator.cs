using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;

namespace Roslyn.Analyzers.SolutionGenerator
{
    public static class SolutionGenerator
    {
        private static string _masterCsvFile;
        private static string _messageCsvFile;
        private static string _sourceDirectory;
        private static string _outputDirectory;
        private static Dictionary<string, Dictionary<string, string>> _projectGuids;

        private static Dictionary<string, IEnumerable<string>> _categories;

        static void Main(string[] args)
        {                        
            _masterCsvFile = args[0];
            _messageCsvFile = args[1];
            _sourceDirectory = args[2];
            _outputDirectory = args[3];

            _projectGuids = new Dictionary<string, Dictionary<string, string>>();

            bool force = false;

            if (args.Length == 5 && args[4].Equals("/force")) { force = true; }

            if (Directory.Exists(_outputDirectory))
            {
                if (!force)
                {
                    Console.WriteLine("ERROR: can't overwrite existing directory: " + _outputDirectory);
                    Console.WriteLine("       pass /force on the command-line to delete and regenerate this content.");
                    return;
                }
                Directory.Delete(_outputDirectory, true);
            }

            CopyStarterFilesToOutputLocation(_sourceDirectory, _outputDirectory);

            // TODOS: add priorization data

            // Build a global list of checks. This operation ensures
            // no check id is reused between any entries
            Dictionary<string, CheckData> checks = BuildIdToChecksMap(_masterCsvFile);

            // Add FxCop resolutions as messages for checks ported from FxCop
            CsvOperations.ParseCheckMessages(_messageCsvFile, checks);

            // Now we will rebuild the data, organizing it by project
            Dictionary<string, List<CheckData>> analyzers = BuildAnalyzerToChecksMap(checks.Values);

            _categories = new Dictionary<string, IEnumerable<string>>();
            foreach (var pair in analyzers)
            {
                _categories[pair.Key] = GetCategories(pair.Value);
            }

            // Emit projects associated with each analyzer. These include a core project, a C#
            // project (for C#-specific AST analyzers), a VB project (ditto) and test project
            EmitAnalyzerProjects(analyzers);

            // TODO emit table that can be published as wiki page to track porting progress
            // EmitWikiTable(analyzers);

            EmitAnalyzerVersionsTargetsFile(analyzers);

            EmitNuGetPackagingProjFile(analyzers);

            EmitBuildAndTestProjFile(analyzers);

            EmitSolution(analyzers);

            Console.WriteLine("Done");
        }

        private static void EmitSolution(Dictionary<string, List<CheckData>> analyzers)
        {
            string original = Path.Combine(_sourceDirectory, "src", "Analyzers.sln");
            string generated = Path.Combine(_outputDirectory, "src", "Analyzers.sln");

            string fileContents = File.ReadAllText(original);

            var sb = new StringBuilder();

            Dictionary<string, string> analyzerToFolderMap = new Dictionary<string, string>();

            string folderProjectGuid = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";

            foreach (string analyzer in analyzers.Keys)
            {
                string folderGuid = Utilities.GetGuid();

                sb.AppendLine(@"Project(""" + folderProjectGuid + @""") = """ + analyzer + @".Analyzers"", """ + analyzer + @".Analyzers"", """ + folderGuid + @"""");
                sb.AppendLine("EndProject");
                analyzerToFolderMap[analyzer] = folderGuid;
            }

            fileContents = fileContents.Replace("INSERTFOLDERS", sb.ToString());
            sb.Length = 0;

            // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Desktop.Analyzers", "Desktop\Core\Desktop.Analyzers.csproj", "{BAA0FEE4-93C8-46F0-BB36-53A6053776C8}"
            // EndProject

            string csharpProjectGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
            string vbProjectGuid = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";

            foreach (string analyzer in analyzers.Keys)
            {
                foreach (ProjectKind kind in GetProjectKinds())
                {
                    // skip NuGet project, which is build by BuildAndTest.proj
                    if (kind == ProjectKind.NuGet)
                    {
                        continue;
                    }
                    string projectName = GetProjectName(analyzer, kind);
                    string projectKindGuid = kind == ProjectKind.VisualBasic ? vbProjectGuid : csharpProjectGuid;
                    sb.AppendLine(@"Project(""" + projectKindGuid + @""") = """ + Path.GetFileNameWithoutExtension(projectName) + @""", """ + analyzer + @".Analyzers\" + kind + @"\" + projectName + @""", """ + _projectGuids[kind.ToString()][analyzer] + @"""");
                    sb.AppendLine("EndProject");
                }
            }

            fileContents = fileContents.Replace("INSERTPROJECTS", sb.ToString());
            sb.Length = 0;

            //{ BAA0FEE4 - 93C8 - 46F0 - BB36 - 53A6053776C8}.Debug|AnyCPU.ActiveCfg = Debug|AnyCPU
            //{ BAA0FEE4 - 93C8 - 46F0 - BB36 - 53A6053776C8}.Debug|AnyCPU.Build.0 = Debug|AnyCPU
            //{ BAA0FEE4 - 93C8 - 46F0 - BB36 - 53A6053776C8}.Release|AnyCPU.ActiveCfg = Release|AnyCPU
            //{ BAA0FEE4 - 93C8 - 46F0 - BB36 - 53A6053776C8}.Release|AnyCPU.Build.0 = Release|AnyCPU


            foreach (string kind in _projectGuids.Keys)
            {
                foreach (string guid in _projectGuids[kind].Values)
                {
                    EmitConfiguration(guid, sb);
                }
            }
            fileContents = fileContents.Replace("INSERTCONFIGURATIONS", sb.ToString());
            sb.Length = 0;

            foreach (string analyzer in analyzers.Keys)
            {
                foreach (string kind in _projectGuids.Keys)
                {
                    string folderGuid = analyzerToFolderMap[analyzer];
                    string coreGuid = _projectGuids[kind][analyzer];
                    sb.AppendLine(coreGuid + " = " + folderGuid);
                }
            }

            fileContents = fileContents.Replace("INSERTNESTEDPROJECTS", sb.ToString());
            sb.Length = 0;

            File.WriteAllText(generated, fileContents);
        }

        private static string GetProjectName(string analyzer, ProjectKind kind)
        {
            switch (kind)
            {
                case ProjectKind.Core: return analyzer + ".Analyzers.csproj";
                case ProjectKind.CSharp: return analyzer + ".CSharp.Analyzers.csproj";
                case ProjectKind.Setup: return analyzer + ".Analyzers.Setup.csproj";
                case ProjectKind.UnitTests: return analyzer + ".Analyzers.UnitTests.csproj";
                case ProjectKind.VisualBasic: return analyzer + ".VisualBasic.Analyzers.vbproj";
                case ProjectKind.NuGet: return analyzer + ".Analyzers.NuGet.proj";
            }
            throw new InvalidOperationException();
        }

        private static IEnumerable<ProjectKind> GetProjectKinds()
        {
            foreach (var member in Enum.GetValues(typeof(ProjectKind)))
            {
                yield return (ProjectKind)member;
            }
        }

        private static void EmitConfiguration(string guid, StringBuilder sb)
        {
            sb.AppendLine(guid + ".Debug|AnyCPU.ActiveCfg = Debug|AnyCPU");
            sb.AppendLine(guid + ".Debug|AnyCPU.Build.0 = Debug|AnyCPU");
            sb.AppendLine(guid + ".Release|AnyCPU.ActiveCfg = Release|AnyCPU");
            sb.AppendLine(guid + ".Release|AnyCPU.Build.0 = Release|AnyCPU");
        }

        private static void EmitNuGetPackagingProjFile(Dictionary<string, List<CheckData>> analyzers)
        {
            string original = Path.Combine(_sourceDirectory, "src", "Packaging", "Packaging.proj");
            string generated = Path.Combine(_outputDirectory, "src", "Packaging", "Packaging.proj"); 
            string marker = "INSERTNUGETPROJECTS";

            var sb = new StringBuilder();

            foreach (string analyzer in analyzers.Keys)
            {
                sb.AppendLine(CodeTemplates.GenerateNuGetProjectItem(analyzer));
            }

            EmitFile(original, generated, marker, sb.ToString());
        }

        private static void EmitBuildAndTestProjFile(Dictionary<string, List<CheckData>> analyzers)
        {
            string original = Path.Combine(_sourceDirectory, "BuildAndTest.proj");
            string generated = Path.Combine(_outputDirectory, "BuildAndTest.proj");
            string marker = "INSERTTESTASSEMBLIES";

            var sb = new StringBuilder();

            foreach (string analyzer in analyzers.Keys)
            {
                sb.AppendLine(CodeTemplates.GenerateUnitTestAssemblyItem(analyzer));
            }

            EmitFile(original, generated, marker, sb.ToString());
        }

        private static void EmitAnalyzerVersionsTargetsFile(Dictionary<string, List<CheckData>> analyzers)
        {
            string original = Path.Combine(_sourceDirectory, "Build", "Targets", "Analyzers.Versions.targets");
            string generated = Path.Combine(_outputDirectory, "Build", "Targets", "Analyzers.Versions.Targets");
            string marker = "INSERTVERSIONS";

            var sb = new StringBuilder();

            foreach (string analyzer in analyzers.Keys)
            {
                string entry = "      <REPLACEMEAnalyzersSemanticVersion>1.0.0</REPLACEMEAnalyzersSemanticVersion>";
                entry = Replace(entry, analyzer);
                sb.AppendLine(entry);

                entry = "      <REPLACEMEAnalyzersPreReleaseVersion>beta1</REPLACEMEAnalyzersPreReleaseVersion>";
                entry = Replace(entry, analyzer);
                sb.AppendLine(entry);

                sb.AppendLine();
            }

            EmitFile(original, generated, marker, sb.ToString());
        }

        private static void EmitFile(string original, string generated, string marker, string replacementContent)
        { 
            string fileContents = File.ReadAllText(original);
            fileContents = fileContents.Replace(marker, replacementContent);
            File.WriteAllText(generated, fileContents);
        }

        private static string Replace(string input, string analyzer)
        {
            input = input.Replace("REPLACE.ME", analyzer);
            input = input.Replace("REPLACEME", analyzer.Replace(".", String.Empty));
            return input;
        }

        private static void CopyStarterFilesToOutputLocation(string sourceDirectory, string outputDirectory)
        {
            Utilities.Copy(sourceDirectory, outputDirectory);
            Directory.Delete(Path.Combine(outputDirectory, "src", "REPLACE.ME"), true);
        }

        private static void EmitAnalyzerProjects(Dictionary<string, List<CheckData>> analyzers)
        {
            foreach(string analyzer in analyzers.Keys)
            {
                BuildCoreAnalyzerProject(analyzer, analyzers[analyzer]);
                BuildCSharpAnalyzerProject(analyzer, analyzers[analyzer]);
                BuildVisualBaseAnalyzerProject(analyzer, analyzers[analyzer]);
                BuildUnitTestsProject(analyzer, analyzers[analyzer]);
                BuildSetupProject(analyzer, analyzers[analyzer]);
                BuildNuGetProject(analyzer);
            }
        }

        private static void BuildNuGetProject(string analyzer)
        {
            string[] copyFiles = new string[] 
            {
            };

            string[] renamedFiles = new string[]
            {
            };

            BuildProject(analyzer, ProjectKind.NuGet, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.NuGet.proj");
        }

        private static void BuildSetupProject(string analyzer, IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "source.extension.vsixmanifest"
            };

            string[] renamedFiles = new string[]
            {
                "source.extension.vsixmanifest"
            };

            BuildProject(analyzer, ProjectKind.Setup, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.Setup.csproj");
        }

        private static void BuildUnitTestsProject(string analyzer, IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "packages.config"
            };

            string[] renamedFiles = new string[]
            {
            };

            BuildProject(analyzer, ProjectKind.UnitTests, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.UnitTests.csproj", checks);
        }

        private static void BuildVisualBaseAnalyzerProject(string analyzer, IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "packages.config"
            };

            string[] renamedFiles = new string[]
            {
                "REPLACE.ME.VisualBasic.Analyzers.props"
            };

            BuildProject(analyzer, ProjectKind.VisualBasic, copyFiles, renamedFiles, "REPLACE.ME.VisualBasic.Analyzers.vbproj", checks);
        }

        private static void BuildCSharpAnalyzerProject(string analyzer, IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "packages.config"
            };

            string[] renamedFiles = new string[]
            {
                "REPLACE.ME.CSharp.Analyzers.props"
            };

            BuildProject(analyzer, ProjectKind.CSharp, copyFiles, renamedFiles, "REPLACE.ME.CSharp.Analyzers.csproj", checks);
        }

        private static void BuildCoreAnalyzerProject(string analyzer, IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "install.ps1",
                "uninstall.ps1",
                "packages.config",
                "ThirdPartyNotices.rtf"
            };

            string[] renamedFiles = new string[]
            {
                "REPLACEMEAnalyzersResources.resx", 
                "REPLACE.ME.Analyzers.nuspec",
                "REPLACE.ME.Analyzers.props"
            };

            BuildProject(analyzer, ProjectKind.Core, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.csproj", checks);
        }

        enum ProjectKind {
            Core,
            CSharp,
            VisualBasic,
            UnitTests,
            Setup,
            NuGet
        };

        private static void BuildProject(string analyzer, ProjectKind kind, IEnumerable<string> copyFiles, IEnumerable<string> renamedFiles, string projectName, IList<CheckData> checks = null)
        {
            string target = Path.Combine(_outputDirectory, "src", analyzer + ".Analyzers", kind.ToString());
            Directory.CreateDirectory(target);

            // Identically named supporting Files
            string source = Path.Combine(_sourceDirectory, "src", "REPLACE.ME", kind.ToString());

            foreach(string copiedFile in copyFiles)
            {
                Utilities.CopyFile(source, target, copiedFile);
            }

            // Everybody gets an assemblyInfo, except vb and nuget
            if (kind != ProjectKind.VisualBasic && kind != ProjectKind.NuGet)
            {
                Directory.CreateDirectory(Path.Combine(target, "Properties"));
                Utilities.CopyFile(source, target, @"Properties\AssemblyInfo.cs");
            }

            foreach (string renamedFile in renamedFiles)
            {
                if (renamedFile == "REPLACEMEAnalyzersResources.resx")
                {
                    Debug.Assert(kind == ProjectKind.Core);
                    Debug.Assert(checks != null);
                }
                CopyRenamedFile(source, target, renamedFile, analyzer, checks);
            }

            Dictionary<string, string> analyzerGuids;

            if (!_projectGuids.TryGetValue(kind.ToString(), out analyzerGuids))
            {
                _projectGuids[kind.ToString()] = analyzerGuids = new Dictionary<string, string>();
            }

            analyzerGuids[analyzer] = CopyRenamedFile(source, target, projectName, analyzer, checks);
            CreateStubFiles(kind, target, analyzer, checks);
        }

        private static void CreateStubFiles(ProjectKind kind, string target, string analyzer, IList<CheckData> checks, bool multipleDirs = false)
        {
            if (multipleDirs)
            {
                // create sub-directories for each category
                if (kind == ProjectKind.Core ||
                    kind == ProjectKind.CSharp ||
                    kind == ProjectKind.VisualBasic ||
                    kind == ProjectKind.UnitTests)
                {
                    Debug.Assert(_categories != null && _categories.ContainsKey(analyzer));
                    foreach (var category in _categories[analyzer])
                    {
                        string newDir = Path.Combine(target, category);
                        Directory.CreateDirectory(newDir);
                    }
                }
            }
                                                                    
            if (kind == ProjectKind.Setup || kind == ProjectKind.NuGet)
            {
                return;
            }

            foreach (var check in checks)
            {
                if (kind == ProjectKind.Core)
                {
                    Utilities.CreateFile(CodeTemplates.GenerateAnalyzer(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateAnalyzerFileName(check, multipleDirs));
                    Utilities.CreateFile(CodeTemplates.GenerateCodeFix(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateCodeFixFileName(check, multipleDirs));
                    Utilities.CreateFile(CodeTemplates.GenerateCategory(analyzer, _categories[analyzer]),
                                         target,
                                         CodeTemplates.CategoryFileName);
                }
                else if (kind == ProjectKind.CSharp)
                {
                    Utilities.CreateFile(CodeTemplates.GenerateCSharpAnalyzer(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateCSharpAnalyzerFileName(check, multipleDirs));
                    Utilities.CreateFile(CodeTemplates.GenerateCSharpCodeFix(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateCSharpCodeFixFileName(check, multipleDirs));
                }
                else if (kind == ProjectKind.VisualBasic)
                {
                    Utilities.CreateFile(CodeTemplates.GenerateBasicAnalyzer(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateBasicAnalyzerFileName(check, multipleDirs));
                    Utilities.CreateFile(CodeTemplates.GenerateBasicCodeFix(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateBasicCodeFixFileName(check, multipleDirs));
                }
                else if (kind == ProjectKind.UnitTests)
                {
                    Utilities.CreateFile(CodeTemplates.GenerateAnalyzerTests(analyzer, check.Name),
                                         target,
                                         CodeTemplates.GenerateAnalyzerTestsFileName(check.Name, check.Category, multipleDirs));
                    Utilities.CreateFile(CodeTemplates.GenerateCodeFixTests(analyzer, check.Name),
                                         target,
                                         CodeTemplates.GenerateCodeFixTestsFileName(check.Name, check.Category, multipleDirs));
                }
            }            
        }

        private static string CopyRenamedFile(string source, string target, string fileName, string analyzer, IList<CheckData> checks, bool multipleDirs = false)
        {
            string fileContents = File.ReadAllText(source + @"\" + fileName);
            fileContents = fileContents.Replace("REPLACE.ME", analyzer);
            fileContents = fileContents.Replace("REPLACEME", analyzer.Replace(".", String.Empty));

            if (fileName == "REPLACEMEAnalyzersResources.resx")
            {
                var sb = new StringBuilder();
                // add diagnostic resource strings
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateDiagnosticResourceData(check));
                }

                Debug.Assert(_categories != null && _categories.ContainsKey(analyzer));
                // add category resource strings
                sb.Append(CodeTemplates.GenerateCategoriesResourceData(_categories[analyzer]));

                fileContents = fileContents.Replace("INSERTRESOURCEDATA", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.Analyzers.csproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateAnalyzerFileName(check, multipleDirs)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCodeFixFileName(check, multipleDirs)));
                }
                sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.CategoryFileName));
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.CSharp.Analyzers.csproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCSharpAnalyzerFileName(check, multipleDirs)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCSharpCodeFixFileName(check, multipleDirs)));
                }                                                                            
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.VisualBasic.Analyzers.vbproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateBasicAnalyzerFileName(check, multipleDirs)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateBasicCodeFixFileName(check, multipleDirs)));
                }                                                                                
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.Analyzers.UnitTests.csproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateAnalyzerTestsFileName(check.Name, check.Category, multipleDirs)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCodeFixTestsFileName(check.Name, check.Category, multipleDirs)));
                }                                                                            
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }

            fileName = fileName.Replace("REPLACE.ME", analyzer);
            fileName = fileName.Replace("REPLACEME", analyzer.Replace(".", String.Empty));

            string guid = Utilities.GetGuid();
            fileContents = fileContents.Replace("INSERTGUID", guid);
            // a new GUID for product ID in vsixmanifest only
            fileContents = fileContents.Replace("INSERTNOBRACESGUID", Guid.NewGuid().ToString());

            // insert GUIDs of referenced projects
            if (fileName.EndsWith("proj"))
            {
                foreach (var projectKind in Enum.GetValues(typeof(ProjectKind)).Cast<ProjectKind>())
                {
                    string sub;
                    switch (projectKind)
                    {
                        case ProjectKind.Core:
                            sub = "CORE";
                            break;
                        case ProjectKind.CSharp:
                            sub = "CSHARP";
                            break;
                        case ProjectKind.VisualBasic:
                            sub = "VB";
                            break;
                        case ProjectKind.UnitTests:
                        case ProjectKind.Setup:
                        case ProjectKind.NuGet:
                            continue;
                        default:
                            throw new InvalidOperationException();
                    }
                    string toReplace = string.Format("INSERT{0}GUID", sub);

                    // project GUIDs for this kind
                    Dictionary<string, string> kindGuids;
                    if (_projectGuids.TryGetValue(projectKind.ToString(), out kindGuids))
                    {
                        string projectGuid;
                        if (kindGuids.TryGetValue(analyzer, out projectGuid))
                        {
                            fileContents = fileContents.Replace(toReplace, projectGuid);
                        }
                    }
                }
            }            

            Utilities.CreateFile(fileContents, target, fileName); 
            return guid;
        }

        private static IEnumerable<string> GetCategories(IEnumerable<CheckData> checks)
        {
            SortedSet<string> categories = null;
            categories = new SortedSet<string>();
            foreach (var check in checks)
            {
                categories.Add(check.Category);
            }
            return categories;  
        } 

        private static Dictionary<string, List<CheckData>> BuildAnalyzerToChecksMap(IEnumerable<CheckData> checks)
        {
            var result = new Dictionary<string, List<CheckData>>();
            List<CheckData> analyzerChecks;

            foreach (CheckData checkData in checks)
            {
                if (string.IsNullOrWhiteSpace(checkData.Category))
                {
                    Console.WriteLine("Warning: Rule {0} is not categorized.", checkData.Id);
                }
                if (!result.TryGetValue(checkData.AnalyzerProject, out analyzerChecks))
                {
                    analyzerChecks = result[checkData.AnalyzerProject] = new List<CheckData>();
                }
                analyzerChecks.Add(checkData);
            }

            return result;
        }

        private static Dictionary<string, CheckData> BuildIdToChecksMap(string csvFile)
        {
            var checkDataList = CsvOperations.ParseCheckDatas(csvFile).ToList();

            var result = new Dictionary<string, CheckData>();

            for (int i = 0; i < checkDataList.Count; ++i)
            {
                var checkData = checkDataList[i];
                Debug.Assert(!result.ContainsKey(checkData.Id));
                // a hidden ID used for codefix purpose only
                if (checkData.Id == "RS0021")
                {
                    continue;
                }
                // convert short description to readableID for those missing it
                if (checkData.Name.Trim().Equals(@"#N/A"))
                {
                    checkData.Name = Utilities.ConvertStringToPascalCase(checkData.Title);
                }

                if (string.IsNullOrWhiteSpace(checkData.Category))
                {
                    checkData.Category = "NoCetegory";
                }

                result[checkData.Id] = checkData;
            }
            return result;
        }
    }
}
