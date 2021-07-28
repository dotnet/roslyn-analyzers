// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AnalyzerCodeGenerator
{
    public static class Utilities
    {
        public static string GetGuid()
        {
            return "{" + Guid.NewGuid().ToString().ToUpperInvariant() + "}";
        }

        public static List<string> ConvertArrayToStringList(Array array)
        {
            var result = new List<string>(array.Length);

            for (int i = 1; i < array.Length; i++)
            {
                object item = array.GetValue(new int[] { i, 1 });

                if (item == null)
                    break; // we're done

                result.Add(item.ToString());
            }

            return result;
        }

        public static string ConvertStringToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException();
            }

            bool newWord = true;
            StringBuilder sb = new StringBuilder();
            foreach (var c in input)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    newWord = true;
                    continue;
                }
                sb.Append(newWord ? char.ToUpper(c) : c);
                newWord = false;
            }

            return sb.ToString();
        }

        public static void CopyFile(string source, string target, string fileName)
        {
            File.Copy(source + @"\" + fileName, target + @"\" + fileName);
        }

        public static void CreateFile(string content, string target, string fileName)
        {
            File.WriteAllText(target + @"\" + fileName, content);
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static Dictionary<string, CheckData> BuildIdToChecksMap(string csvFile, Func<CheckData, bool> predicate)
        {
            var checkDataList = CsvOperations.ParseCheckDatas(csvFile).Where(predicate).ToList();

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

        public static Dictionary<string, List<CheckData>> BuildAnalyzerToChecksMap(IEnumerable<CheckData> checks)
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

        public static void CreateStubFiles(ProjectKind kind, string target, string analyzer, IList<CheckData> checks, Dictionary<string, IEnumerable<string>> categories)
        {
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
                                         CodeTemplates.GenerateAnalyzerFileName(check));
                    Utilities.CreateFile(CodeTemplates.GenerateCodeFix(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateCodeFixFileName(check));
                    Utilities.CreateFile(CodeTemplates.GenerateCategory(analyzer, categories[analyzer]),
                                         target,
                                         CodeTemplates.CategoryFileName);
                }
                else if (kind == ProjectKind.CSharp)
                {
                    Utilities.CreateFile(CodeTemplates.GenerateCSharpAnalyzer(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateCSharpAnalyzerFileName(check));
                    Utilities.CreateFile(CodeTemplates.GenerateCSharpCodeFix(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateCSharpCodeFixFileName(check));
                }
                else if (kind == ProjectKind.VisualBasic)
                {
                    Utilities.CreateFile(CodeTemplates.GenerateBasicAnalyzer(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateBasicAnalyzerFileName(check));
                    Utilities.CreateFile(CodeTemplates.GenerateBasicCodeFix(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateBasicCodeFixFileName(check));
                }
                else if (kind == ProjectKind.UnitTests)
                {
                    Utilities.CreateFile(CodeTemplates.GenerateAnalyzerTests(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateAnalyzerTestsFileName(check));
                    Utilities.CreateFile(CodeTemplates.GenerateCodeFixTests(analyzer, check),
                                         target,
                                         CodeTemplates.GenerateCodeFixTestsFileName(check));
                }
            }
        }

        public static string CopyRenamedFile(string source, string target, string fileName, string analyzer, IList<CheckData> checks, Dictionary<string, IEnumerable<string>> categories, Dictionary<string, Dictionary<string, string>> projectGuids)
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

                Debug.Assert(categories != null && categories.ContainsKey(analyzer));
                // add category resource strings
                sb.Append(CodeTemplates.GenerateCategoriesResourceData(categories[analyzer]));

                fileContents = fileContents.Replace("INSERTRESOURCEDATA", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.Analyzers.csproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateAnalyzerFileName(check)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCodeFixFileName(check)));
                }
                sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.CategoryFileName));
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.CSharp.Analyzers.csproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCSharpAnalyzerFileName(check)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCSharpCodeFixFileName(check)));
                }
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.VisualBasic.Analyzers.vbproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateBasicAnalyzerFileName(check)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateBasicCodeFixFileName(check)));
                }
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }
            else if (fileName == "REPLACE.ME.Analyzers.UnitTests.csproj")
            {
                var sb = new StringBuilder();
                foreach (var check in checks)
                {
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateAnalyzerTestsFileName(check)));
                    sb.Append(CodeTemplates.GenerateCompileItem(CodeTemplates.GenerateCodeFixTestsFileName(check)));
                }
                fileContents = fileContents.Replace("INSERTSOURCEFILES", sb.ToString());
            }

            fileName = fileName.Replace("REPLACE.ME", analyzer);
            fileName = fileName.Replace("REPLACEME", analyzer.Replace(".", String.Empty));

            string guid = GetGuid();
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
                    if (projectGuids.TryGetValue(projectKind.ToString(), out kindGuids))
                    {
                        string projectGuid;
                        if (kindGuids.TryGetValue(analyzer, out projectGuid))
                        {
                            fileContents = fileContents.Replace(toReplace, projectGuid);
                        }
                    }
                }
            }

            CreateFile(fileContents, target, fileName);
            return guid;
        }

        public static IEnumerable<string> GetCategories(IEnumerable<CheckData> checks)
        {
            SortedSet<string> categories = null;
            categories = new SortedSet<string>();
            foreach (var check in checks)
            {
                categories.Add(check.Category);
            }
            return categories;
        }

        public static void EmitAnalyzerProjects(
            Dictionary<string, List<CheckData>> analyzers,
            string outputDirectory,
            string sourceDirectory,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids)
        {
            foreach (string analyzer in analyzers.Keys)
            {
                var checks = analyzers[analyzer];
                BuildCoreAnalyzerProject(outputDirectory, sourceDirectory, analyzer, categories, projectGuids, checks);
                BuildCSharpAnalyzerProject(outputDirectory, sourceDirectory, analyzer, categories, projectGuids, checks);
                BuildVisualBasicAnalyzerProject(outputDirectory, sourceDirectory, analyzer, categories, projectGuids, checks);
                BuildUnitTestsProject(outputDirectory, sourceDirectory, analyzer, categories, projectGuids, checks);
                BuildSetupProject(outputDirectory, sourceDirectory, analyzer, categories, projectGuids, checks);
                BuildNuGetProject(outputDirectory, sourceDirectory, analyzer, categories, projectGuids, checks);
            }
        }

        public static void BuildNuGetProject(
            string outputDirectory,
            string sourceDirectory,
            string analyzer,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids,
            IList<CheckData> checks)
        {
            string[] copyFiles = new string[]
            {
            };

            string[] renamedFiles = new string[]
            {
            };

            BuildProject(outputDirectory, sourceDirectory, analyzer, ProjectKind.NuGet, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.NuGet.proj", categories, projectGuids, checks);
        }

        public static void BuildSetupProject(
            string outputDirectory,
            string sourceDirectory,
            string analyzer,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids,
            IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "source.extension.vsixmanifest"
            };

            string[] renamedFiles = new string[]
            {
                "source.extension.vsixmanifest"
            };

            BuildProject(outputDirectory, sourceDirectory, analyzer, ProjectKind.Setup, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.Setup.csproj", categories, projectGuids, checks);
        }

        public static void BuildUnitTestsProject(
            string outputDirectory,
            string sourceDirectory,
            string analyzer,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids,
            IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "packages.config"
            };

            string[] renamedFiles = new string[]
            {
            };

            BuildProject(outputDirectory, sourceDirectory, analyzer, ProjectKind.UnitTests, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.UnitTests.csproj", categories, projectGuids, checks);
        }

        public static void BuildVisualBasicAnalyzerProject(
            string outputDirectory,
            string sourceDirectory,
            string analyzer,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids,
            IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "packages.config"
            };

            string[] renamedFiles = new string[]
            {
                "REPLACE.ME.VisualBasic.Analyzers.props"
            };

            BuildProject(outputDirectory, sourceDirectory, analyzer, ProjectKind.VisualBasic, copyFiles, renamedFiles, "REPLACE.ME.VisualBasic.Analyzers.vbproj", categories, projectGuids, checks);
        }

        public static void BuildCSharpAnalyzerProject(
            string outputDirectory,
            string sourceDirectory,
            string analyzer,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids,
            IList<CheckData> checks)
        {
            string[] copyFiles = new string[] {
                "packages.config"
            };

            string[] renamedFiles = new string[]
            {
                "REPLACE.ME.CSharp.Analyzers.props"
            };

            BuildProject(outputDirectory, sourceDirectory, analyzer, ProjectKind.CSharp, copyFiles, renamedFiles, "REPLACE.ME.CSharp.Analyzers.csproj", categories, projectGuids, checks);
        }

        public static void BuildCoreAnalyzerProject(
            string outputDirectory,
            string sourceDirectory,
            string analyzer,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids,
            IList<CheckData> checks)
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

            BuildProject(outputDirectory, sourceDirectory, analyzer, ProjectKind.Core, copyFiles, renamedFiles, "REPLACE.ME.Analyzers.csproj", categories, projectGuids, checks);
        }

        public static void BuildProject(
            string outputDirectory,
            string sourceDirectory,
            string analyzer, 
            ProjectKind kind, 
            IEnumerable<string> copyFiles, 
            IEnumerable<string> renamedFiles, 
            string projectName,
            Dictionary<string, IEnumerable<string>> categories,
            Dictionary<string, Dictionary<string, string>> projectGuids,
            IList<CheckData> checks)
        {
            string target = Path.Combine(outputDirectory, "src", analyzer + ".Analyzers", kind.ToString());
            Directory.CreateDirectory(target);

            // Identically named supporting Files
            string source = Path.Combine(sourceDirectory, "src", "REPLACE.ME", kind.ToString());

            foreach (string copiedFile in copyFiles)
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
                Utilities.CopyRenamedFile(source, target, renamedFile, analyzer, checks, categories, projectGuids);
            }

            Dictionary<string, string> analyzerGuids;

            if (!projectGuids.TryGetValue(kind.ToString(), out analyzerGuids))
            {
                projectGuids[kind.ToString()] = analyzerGuids = new Dictionary<string, string>();
            }

            analyzerGuids[analyzer] = Utilities.CopyRenamedFile(source, target, projectName, analyzer, checks, categories, projectGuids);
            Utilities.CreateStubFiles(kind, target, analyzer, checks, categories);
        }
    }
}
