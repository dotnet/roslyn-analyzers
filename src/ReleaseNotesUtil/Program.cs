// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReleaseNotesUtil
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (!((args.Length == 4 && args[0] == "getrulesjson")
                  || ((args.Length == 4 || args.Length == 5) && args[0] == "diffrules")))
            {
                PrintUsage();
                return;
            }

            string command = args[0];
            switch (command)
            {
                case "getrulesjson":
                    GetRulesJson(args[1], args[2], args[3]);
                    break;

                case "diffrules":
                    DiffRules(args[1], args[2], args[3], args.Length > 4 ? args[4] : null);
                    break;

                default:
                    throw new ArgumentException($"Unhandled command {command}");
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ReleaseNoteUtil command commandArgs ...");
            Console.WriteLine("  getrulesjson pathToNugetInstalledPackages fxCopAnalyzersVersion out.json");
            Console.WriteLine("  diffrules old.json new.json out.md");
        }

        private static void GetRulesJson(string nugetInstalledPackagesPath, string version, string outputPath)
        {
            IEnumerable<string> dllPaths = GetFxCopAnalyzerBinaries(nugetInstalledPackagesPath, version);
            RuleFileContent ruleFileContent = new RuleFileContent
            {
                Rules = GetRules(dllPaths)
            };
            ruleFileContent.Rules.Sort(CategoryThenIdComparer.Instance);
            WriteRuleFileContent(ruleFileContent, outputPath);
        }

        private static void DiffRules(
            string oldRulesJsonPath,
            string newRulesJsonPath,
            string outputPath,
            string latestRulesJsonPath = null)
        {
            RuleFileContent oldContent = ReadRuleFileContent(oldRulesJsonPath);
            RuleFileContent newContent = ReadRuleFileContent(newRulesJsonPath);

            // If we have the latest rules, we can backfill missing help link URLs.
            if (!string.IsNullOrWhiteSpace(latestRulesJsonPath))
            {
                RuleFileContent latestContent = ReadRuleFileContent(latestRulesJsonPath);
                Dictionary<string, RuleInfo> latestRulesById = latestContent.Rules.ToDictionary(r => r.Id);
                foreach (RuleInfo rule in oldContent.Rules.Concat(newContent.Rules))
                {
                    if (string.IsNullOrWhiteSpace(rule.HelpLink)
                        && latestRulesById.TryGetValue(rule.Id, out RuleInfo latestRule))
                    {
                        rule.HelpLink = latestRule.HelpLink;
                    }
                }
            }

            Dictionary<string, RuleInfo> oldRulesById = oldContent.Rules.ToDictionary(r => r.Id);
            Dictionary<string, RuleInfo> newRulesById = newContent.Rules.ToDictionary(r => r.Id);
            IEnumerable<RuleInfo> addedRules =
                newContent.Rules
                    .Where(r => !oldRulesById.ContainsKey(r.Id));
            IEnumerable<RuleInfo> removedRules =
                oldContent.Rules
                    .Where(r => !newRulesById.ContainsKey(r.Id));
            IEnumerable<RuleInfo> changedRules =
                newContent.Rules
                    .Where(r => oldRulesById.TryGetValue(r.Id, out RuleInfo oldRule)
                                && r.IsEnabledByDefault != oldRule.IsEnabledByDefault);
            StringBuilder sb = new StringBuilder();
            GenerateAddRemovedRulesDiffMarkdown(sb, "### Added", addedRules);
            GenerateAddRemovedRulesDiffMarkdown(sb, "### Removed", removedRules);
            GenerateChangedRulesDiffMarkdown(sb, "### Changed", changedRules);
            File.WriteAllText(outputPath, sb.ToString());
        }

        private static void WriteRuleFileContent(RuleFileContent ruleFileContent, string outputPath)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RuleFileContent));
            using FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            serializer.WriteObject(fs, ruleFileContent);
        }

        private static RuleFileContent ReadRuleFileContent(string path)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RuleFileContent));
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (RuleFileContent)serializer.ReadObject(fs);
        }

        private static void GenerateAddRemovedRulesDiffMarkdown(StringBuilder sb, string heading, IEnumerable<RuleInfo> rules)
        {
            if (!rules.Any())
            {
                return;
            }

            IEnumerable<RuleInfo> sortedRules = rules.OrderBy(r => r, CategoryThenIdComparer.Instance);

            sb.AppendLine(heading);
            string previousCategory = null;
            foreach (RuleInfo rule in sortedRules)
            {
                if (rule.Category != previousCategory)
                {
                    sb.AppendLine($"- {rule.Category}");
                    previousCategory = rule.Category;
                }

                sb.AppendLine($"  - {rule.IdWithHelpLinkMarkdown}: {rule.Title}{(rule.IsEnabledByDefault ? " -- **Enabled by default**" : "")}");
            }
        }

        private static void GenerateChangedRulesDiffMarkdown(StringBuilder sb, string heading, IEnumerable<RuleInfo> rules)
        {
            if (!rules.Any())
            {
                return;
            }

            IEnumerable<RuleInfo> sortedRules = rules.OrderBy(r => r, CategoryThenIdComparer.Instance);

            sb.AppendLine(heading);
            string previousCategory = null;
            foreach (RuleInfo rule in sortedRules)
            {
                if (rule.Category != previousCategory)
                {
                    sb.AppendLine($"- {rule.Category}");
                    previousCategory = rule.Category;
                }

                sb.AppendLine($"  - {rule.IdWithHelpLinkMarkdown}: {rule.Title} -- {(rule.IsEnabledByDefault ? "Now **enabled by default**" : "Now disabled by default")}");
            }
        }

        private static IEnumerable<string> GetFxCopAnalyzerBinaries(string nugetInstalledPackagesPath, string version)
        {
            if (!Directory.Exists(nugetInstalledPackagesPath))
            {
                throw new ArgumentException($"'{nugetInstalledPackagesPath}' is not a directory or doesn't exist");
            }

            string[] roslynAnalyzerPackages = new string[] {
                "Microsoft.CodeQuality.Analyzers",
                "Microsoft.NetCore.Analyzers",
                "Microsoft.NetFramework.Analyzers",
                "Text.Analyzers",   // deprecated
                "Microsoft.CodeAnalysis.VersionCheckAnalyzer",
            };
            foreach (string roslynAnalyzerPackage in roslynAnalyzerPackages)
            {
                string packageWithVersion = $"{roslynAnalyzerPackage}.{version}";
                string packageDirectory = Path.Combine(nugetInstalledPackagesPath, packageWithVersion);
                foreach (string dllFile in GetAllAnalyzerBinaries(packageDirectory))
                {
                    yield return dllFile;
                }
            }
        }

        private static readonly string[] LanguageDirectoryNames = new string[] { "cs", "vb" };

        private static IEnumerable<string> GetAllAnalyzerBinaries(string nugetInstalledPackagePath)
        {
            // Assuming analyzers are located in directories as in
            // https://github.com/dotnet/roslyn-analyzers/blob/32d8f1e397439035f0ecb5f61a9e672225f0ecdb/tools/AnalyzerCodeGenerator/template/src/REPLACE.ME/Core/install.ps1
            string analyzersDirectory = Path.Combine(nugetInstalledPackagePath, "analyzers");
            if (!Directory.Exists(analyzersDirectory))
            {
                yield break;
            }

            foreach (string frameworkDirectory in Directory.EnumerateDirectories(analyzersDirectory))
            {
                foreach (string dllFile in Directory.EnumerateFiles(frameworkDirectory, "*.dll"))
                {
                    yield return dllFile;
                }

                foreach (string languageName in LanguageDirectoryNames)
                {
                    string languageDirectory = Path.Combine(frameworkDirectory, languageName);
                    if (Directory.Exists(languageDirectory))
                    {
                        foreach (string languageDllFile in Directory.EnumerateFiles(languageDirectory, "*.dll"))
                        {
                            yield return languageDllFile;
                        }
                    }
                }
            }
        }

        private static List<RuleInfo> GetRules(IEnumerable<string> dllPaths)
        {
            List<RuleInfo> ruleInfos = new List<RuleInfo>();
            foreach (string dllPath in dllPaths)
            {
                AnalyzerFileReference analyzerFileReference = new AnalyzerFileReference(
                    dllPath,
                    AnalyzerAssemblyLoader.Instance);
                ImmutableArray<DiagnosticAnalyzer> analyzers = analyzerFileReference.GetAnalyzersForAllLanguages();
                IEnumerable<DiagnosticDescriptor> descriptors =
                    analyzers
                        .SelectMany(a => a.SupportedDiagnostics)
                        .Distinct(DiagnosticIdComparer.Instance);
                HashSet<string> fixableDiagnosticIds =
                    analyzerFileReference
                        .GetFixers()
                        .SelectMany(fixer => fixer.FixableDiagnosticIds).ToHashSet();
                Console.WriteLine($"{dllPath} has {analyzers.Count()} analyzers, {descriptors.Count()} diagnostics, and {fixableDiagnosticIds.Count} fixers");
                foreach (DiagnosticDescriptor descriptor in descriptors)
                {
                    if (ruleInfos.Any(r => r.Id == descriptor.Id))
                    {
                        continue;
                    }

                    ruleInfos.Add(
                        new RuleInfo(
                            descriptor.Id,
                            descriptor.Title.ToString(),
                            descriptor.Category,
                            descriptor.IsEnabledByDefault,
                            fixableDiagnosticIds.Contains(descriptor.Id),
                            descriptor.MessageFormat.ToString(),
                            descriptor.Description.ToString(),
                            descriptor.HelpLinkUri));
                }
            }

            return ruleInfos;
        }

        private static void AnalyzerFileReference_AnalyzerLoadFailed(object sender, AnalyzerLoadFailureEventArgs e)
        {
            Console.WriteLine($"Analyzer load failed: ErrorCode: {e.ErrorCode} Message: {e.Message} Exception:{e.Exception}");
        }
    }
}