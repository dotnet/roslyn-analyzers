// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AnalyzerCodeGenerator
{
    public class IncrementalGenerator
    {
        private static string _masterCsvFile;
        private static string _messageCsvFile;
        private static string _targetDirectory;
        private static string _templateDirectory;
        private static string _sourceFilesDirectory;

        public static void Main(string[] args)
        {
            _masterCsvFile = args[0];
            _messageCsvFile = args[1];
            _templateDirectory = args[2];
            _targetDirectory = args[3];

            if (args.Length != 4)
            {
                Console.WriteLine("Usage: IncrementalGenerator <RuleInventory CSV file> <FxCop Messages CSV file> <Template Files Directory> <Target Directory>");
            }

            if (!Directory.Exists(_targetDirectory))
            {
                Console.WriteLine("ERROR: target directory doesn't exist: " + _targetDirectory);
                Console.WriteLine("       (SolutionGenerator could be used to generate all files from scratch)");
                return;
            }

            _sourceFilesDirectory = Path.Combine(_targetDirectory, "src");
            if (!Directory.Exists(_sourceFilesDirectory))
            {
                throw new ArgumentException($"Error: {_sourceFilesDirectory} doesn't exist.");
            }

            // Build the list of checks from the rule inventory spreadsheet
            var allChecks = Utilities.BuildIdToChecksMap(_masterCsvFile, 
                c => c.Port == PortStatus.Yes && (c.RevisedPriority == Priority.High || c.OriginalPriority == Priority.High ));

            // reorg the dictionary by check name
            Dictionary<string, CheckData> allChecksByName = new Dictionary<string, CheckData>();
            foreach (var pair in allChecks)
            {
                Debug.Assert(!allChecksByName.ContainsKey(pair.Value.Name), $"Error: Duplicate check name found: {pair.Value.Name}");
                allChecksByName[pair.Value.Name] = pair.Value;
            }

            // Read core project files of existing analyzers and build a list of already implemented rules.
            // We assume all the existing analyzers follow the naming convention.
            var existingChecksByName = GetExistingChecksFromCoreProjects(_targetDirectory, allChecksByName);

            // generate a list of existing analyzer projects
            var existingAnalyzers = new HashSet<string>();
            foreach (var analyzer in existingChecksByName.Select(c => c.Value.AnalyzerProject))
            {
                existingAnalyzers.Add(analyzer);
            }
            
            // Build the list of checks that doesn't exist in target directory.
            var newChecksToAdd = new Dictionary<string, CheckData>();
            var newChecksToCreate = new Dictionary<string, CheckData>();

            foreach (var check in allChecksByName)
            {
                if (existingChecksByName.ContainsKey(check.Key))
                {
                    continue;
                }
                if (!existingAnalyzers.Contains(check.Value.AnalyzerProject))
                {
                    newChecksToCreate[check.Value.Id] = check.Value;
                }
                else
                {
                    newChecksToAdd[check.Value.Id] = check.Value;
                }
            }

            // Add FxCop messages
            CsvOperations.ParseCheckMessages(_messageCsvFile, newChecksToAdd);

            // rebuild the data, organized by analyzer project
            var newChecksToAddByAnalyzer = Utilities.BuildAnalyzerToChecksMap(newChecksToAdd.Values);

            // generate new files and insert them to existing projects
            foreach (var pair in newChecksToAddByAnalyzer)
            {
                var analyzer = pair.Key;
                var checks = pair.Value;

                AddNewChecksToCoreProject(analyzer, checks);
                AddNewChecksToCSharpProject(analyzer, checks);
                AddNewChecksToVisualBasicProject(analyzer, checks);
                AddNewChecksToUnitTestsProject(analyzer, checks);
                AddNewStringsToResxFile(analyzer, checks);
            }
            Console.WriteLine("Added checks:");
            foreach (var pair in newChecksToAddByAnalyzer)
            {
                Console.WriteLine($"\t{pair.Key}");
                foreach (var check in pair.Value)
                {
                    Console.WriteLine($"\t\t{check.Id}");
                }
            }

            if (newChecksToCreate.Count > 0)
            {
                if (!Directory.Exists(_templateDirectory))
                {
                    Console.WriteLine("ERROR: Template files directory doesn't exist: " + _templateDirectory);
                    return;
                }
                CsvOperations.ParseCheckMessages(_messageCsvFile, newChecksToCreate);
                var newChecksToCreateByAnalyzer = Utilities.BuildAnalyzerToChecksMap(newChecksToCreate.Values);

                // create new projects
                var projectGuids = new Dictionary<string, Dictionary<string, string>>();
                var categories = new Dictionary<string, IEnumerable<string>>();
                foreach (var pair in newChecksToCreateByAnalyzer)
                {
                    categories[pair.Key] = Utilities.GetCategories(pair.Value);
                }

                Utilities.EmitAnalyzerProjects(newChecksToCreateByAnalyzer, _targetDirectory, _templateDirectory, categories, projectGuids);

                Console.WriteLine("Created checks:");
                foreach (var pair in newChecksToCreateByAnalyzer)
                {
                    Console.WriteLine($"\t{pair.Key}");
                    foreach (var check in pair.Value)
                    {
                        Console.WriteLine($"\t\t{check.Id}");
                    }
                }
            }
        }

        private static void AddNewChecksToCoreProject(string analyzer, List<CheckData> checks)
        {
            var analyzerFullName = analyzer + ".Analyzers";
            var analyzerDir = Path.Combine(_sourceFilesDirectory, analyzerFullName, "Core");
            var coreProjFile = Path.Combine(analyzerDir, analyzerFullName + ".csproj");
            foreach (var check in checks)
            {
                AddCompileItemsToProjectFile(coreProjFile, ProjectKind.Core, check);

                Utilities.CreateFile(CodeTemplates.GenerateAnalyzer(analyzer, check),
                                     analyzerDir,
                                     CodeTemplates.GenerateAnalyzerFileName(check));

                Utilities.CreateFile(CodeTemplates.GenerateCodeFix(analyzer, check),
                                     analyzerDir,
                                     CodeTemplates.GenerateCodeFixFileName(check));
            }
        }

        private static void AddNewChecksToCSharpProject(string analyzer, List<CheckData> checks)
        {
            var analyzerFullName = analyzer + ".Analyzers";
            var analyzerDir = Path.Combine(_sourceFilesDirectory, analyzerFullName, "CSharp");
            var csProjFile = Path.Combine(analyzerDir, analyzer + ".CSharp.Analyzers.csproj");
            foreach (var check in checks)
            {
                AddCompileItemsToProjectFile(csProjFile, ProjectKind.CSharp, check);

                Utilities.CreateFile(CodeTemplates.GenerateCSharpAnalyzer(analyzer, check),
                                     analyzerDir,
                                     CodeTemplates.GenerateCSharpAnalyzerFileName(check));

                Utilities.CreateFile(CodeTemplates.GenerateCSharpCodeFix(analyzer, check),
                                     analyzerDir,
                                     CodeTemplates.GenerateCSharpCodeFixFileName(check));
            }
        }

        private static void AddNewChecksToVisualBasicProject(string analyzer, List<CheckData> checks)
        {
            var analyzerFullName = analyzer + ".Analyzers";
            var analyzerDir = Path.Combine(_sourceFilesDirectory, analyzerFullName, "VisualBasic");
            var vbProjFile = Path.Combine(analyzerDir, analyzer + ".VisualBasic.Analyzers.vbproj");
            foreach (var check in checks)
            {
                AddCompileItemsToProjectFile(vbProjFile, ProjectKind.VisualBasic, check);

                Utilities.CreateFile(CodeTemplates.GenerateBasicAnalyzer(analyzer, check),
                                     analyzerDir,
                                     CodeTemplates.GenerateBasicAnalyzerFileName(check));

                Utilities.CreateFile(CodeTemplates.GenerateBasicCodeFix(analyzer, check),
                                     analyzerDir,
                                     CodeTemplates.GenerateBasicCodeFixFileName(check));
            }
        }

        private static void AddNewChecksToUnitTestsProject(string analyzer, List<CheckData> checks)
        {
            var analyzerFullName = analyzer + ".Analyzers";
            var testsDir = Path.Combine(_sourceFilesDirectory, analyzerFullName, "UnitTests");
            var vbProjFile = Path.Combine(testsDir, analyzerFullName + ".UnitTests.csproj");
            foreach (var check in checks)
            {
                AddCompileItemsToProjectFile(vbProjFile, ProjectKind.UnitTests, check);

                Utilities.CreateFile(CodeTemplates.GenerateAnalyzerTests(analyzer, check),
                                     testsDir,
                                     CodeTemplates.GenerateAnalyzerTestsFileName(check));

                Utilities.CreateFile(CodeTemplates.GenerateCodeFixTests(analyzer, check),
                                     testsDir,
                                     CodeTemplates.GenerateCodeFixTestsFileName(check));
            }
        }

        private static void AddNewStringsToResxFile(string analyzer, List<CheckData> checks)
        {
            var analyzerFullName = analyzer + ".Analyzers";
            var analyzerDir = Path.Combine(_sourceFilesDirectory, analyzerFullName, "Core");
            var resxFile = Path.Combine(analyzerDir, analyzerFullName.Replace(".", string.Empty) + "Resources.resx");

            XDocument xmlFile = XDocument.Load(resxFile);
            Debug.Assert(xmlFile.Root.Name.LocalName == "root");

            var first = xmlFile.Root.Descendants().Where(c => c.Name.LocalName == "data").First();
            foreach (var check in checks)
            {
                XElement elem = null;
                List<XElement> list = new List<XElement>();

                elem = new XElement(first);
                elem.SetAttributeValue("name", check.Name + "Title");
                elem.Element("value").Value = check.Title;
                list.Add(elem);

                elem = new XElement(first);
                elem.SetAttributeValue("name", check.Name + "Description");
                elem.Element("value").Value = check.Description;
                list.Add(elem);

                if (check.Messages != null && check.Messages.Count > 1)
                {
                    foreach (var pair in check.Messages)
                    {
                        elem = new XElement(first);
                        elem.SetAttributeValue("name", check.Name + "Message" + pair.Key);
                        elem.Element("value").Value = pair.Value;
                        list.Add(elem);
                    }
                }
                else
                {
                    // use title as message
                    elem = new XElement(first);
                    elem.SetAttributeValue("name", check.Name + "Message");
                    elem.Element("value").Value = check.Title;
                    list.Add(elem);
                }

                xmlFile.Root.Add(list);
            }

            xmlFile.Save(resxFile);
        }

        private static Dictionary<string, CheckData> GetExistingChecksFromCoreProjects(string targetDir, Dictionary<string, CheckData> allChecksByName)
        {
            var allDirs = Directory.GetDirectories(_sourceFilesDirectory);

            Dictionary<string, CheckData> existingChecks = new Dictionary<string, CheckData>();

            foreach (var dir in allDirs)
            {
                var analyzerName = Path.GetFileName(dir);
                if (!analyzerName.EndsWith(".Analyzers", StringComparison.Ordinal))
                {
                    continue;
                }
                var analyzerCoreDir = Path.Combine(dir, "Core");
                if (!Directory.Exists(analyzerCoreDir))
                {
                    throw new ArgumentException($"Error: {analyzerCoreDir} doesn't exist.");
                }

                var projFile = Path.Combine(analyzerCoreDir, analyzerName + ".csproj");
                if (!File.Exists(projFile))
                {
                    if (analyzerName == "Microsoft.CodeAnalysis.Analyzers")
                    {
                        // Microsoft.CodeAnalysis.Analyzers projects are moved here w/o any change, ignore it
                        continue;
                    }
                    throw new ArgumentException($"Error: {projFile} doesn't exist.");
                }

                var checksFromProj = GetExisingChecksFromCoreProjectFile(projFile, allChecksByName);
                foreach (var pair in checksFromProj)
                {
                    Debug.Assert(!existingChecks.ContainsKey(pair.Key), $"Duplicate check {pair.Key} in {projFile}");
                    existingChecks.Add(pair.Key, pair.Value);
                }
            }

            return existingChecks;
        }

        private static Dictionary<string, CheckData> GetExisingChecksFromCoreProjectFile(string projFile, Dictionary<string, CheckData> allChecksByName)
        {
            var analyzerProject = Path.GetFileName(projFile.Remove(projFile.IndexOf(".Analyzers.csproj")));

            XDocument xmlFile = XDocument.Load(projFile);
            Debug.Assert(xmlFile.Root.Name.LocalName == "Project");

            Dictionary<string, CheckData> existingChecks = new Dictionary<string, CheckData>();
            foreach (var sourceFile in xmlFile.Root.Descendants().Where(c => c.Name.LocalName == "Compile").Select(c => c.Attribute("Include").Value))
            {
                var file = sourceFile.Split('\\').Last();
                if (file.EndsWith(".Fixer.cs"))
                {
                    continue;
                }
                var checkName = Path.GetFileNameWithoutExtension(file);
                if (!allChecksByName.ContainsKey(checkName))
                {
                    continue;
                }

                // if a check is moved to a different project, we need to regen it)
                if (allChecksByName[checkName].AnalyzerProject != analyzerProject) 
                {
                    Console.WriteLine($"Please manually delete\t {checkName} from {analyzerProject}");
                    continue;
                }

                existingChecks[checkName] = allChecksByName[checkName];
            }
            return existingChecks;
        }

        private static void AddCompileItemsToProjectFile(string projFile, ProjectKind kind, CheckData check)
        {
            XDocument xmlFile = XDocument.Load(projFile);
            Debug.Assert(xmlFile.Root.Name.LocalName == "Project");

            var first = xmlFile.Root.Descendants().Where(c => c.Name.LocalName == "Compile").First();

            var parent = first.Parent;
            XElement elem = null;
            switch (kind)
            {
                case ProjectKind.Core:
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateAnalyzerFileName(check));
                    parent.Add(elem);
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateCodeFixFileName(check));
                    parent.Add(elem);
                    break;
                case ProjectKind.CSharp:
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateCSharpAnalyzerFileName(check));
                    parent.Add(elem);
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateCSharpCodeFixFileName(check));
                    parent.Add(elem);
                    break;
                case ProjectKind.VisualBasic:
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateBasicAnalyzerFileName(check));
                    parent.Add(elem);
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateBasicCodeFixFileName(check));
                    parent.Add(elem);
                    break;
                case ProjectKind.UnitTests:
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateAnalyzerTestsFileName(check));
                    parent.Add(elem);
                    elem = new XElement(first);
                    elem.SetAttributeValue("Include", CodeTemplates.GenerateCodeFixTestsFileName(check));
                    parent.Add(elem);
                    break;
                default:
                    throw new ArgumentException(kind.ToString());
            }
            xmlFile.Save(projFile);
            return;
        }
    }
}
