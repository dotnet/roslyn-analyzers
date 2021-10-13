// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnalyzerCodeGenerator
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
            Dictionary<string, CheckData> checks = Utilities.BuildIdToChecksMap(_masterCsvFile, c => c.Port == PortStatus.Yes || c.Port == PortStatus.Ported);

            // Add FxCop resolutions as messages for checks ported from FxCop
            CsvOperations.ParseCheckMessages(_messageCsvFile, checks);

            // Now we will rebuild the data, organizing it by project
            Dictionary<string, List<CheckData>> analyzers = Utilities.BuildAnalyzerToChecksMap(checks.Values);

            _categories = new Dictionary<string, IEnumerable<string>>();
            foreach (var pair in analyzers)
            {
                _categories[pair.Key] = Utilities.GetCategories(pair.Value);
            }

            // Emit projects associated with each analyzer. These include a core project, a C#
            // project (for C#-specific AST analyzers), a VB project (ditto) and test project
            Utilities.EmitAnalyzerProjects(analyzers, _outputDirectory, _sourceDirectory, _categories, _projectGuids);

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
    }
}
