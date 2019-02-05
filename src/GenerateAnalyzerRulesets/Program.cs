// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GenerateAnalyzerRulesets
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 8)
            {
                Console.Error.WriteLine($"Excepted 8 arguments, found {args.Length}: {string.Join(';', args)}");
                return 1;
            }

            string analyzerRulesetsDir = args[0];
            string binDirectory = args[1];
            string configuration = args[2];
            string tfm = args[3];
            var assemblyList = args[4].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string propsFileDir = args[5];
            string propsFileName = args[6];
            if (!bool.TryParse(args[7], out var containsPortedFxCopRules))
            {
                containsPortedFxCopRules = false;
            }

            var allRulesByAssembly = new SortedList<string, SortedList<string, DiagnosticDescriptor>>();
            var categories = new HashSet<string>();
            foreach (string assembly in assemblyList)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                string path = Path.Combine(binDirectory, assemblyName, configuration, tfm, assembly);
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"'{path}' does not exist");
                    return 1;
                }

                var analyzerFileReference = new AnalyzerFileReference(path, AnalyzerAssemblyLoader.Instance);
                var analyzers = analyzerFileReference.GetAnalyzersForAllLanguages();
                var rules = analyzers.SelectMany(a => a.SupportedDiagnostics);
                if (rules.Any())
                {
                    var rulesById = new SortedList<string, DiagnosticDescriptor>();
                    foreach (DiagnosticDescriptor rule in rules)
                    {
                        rulesById[rule.Id] = rule;
                        categories.Add(rule.Category);
                    }

                    allRulesByAssembly.Add(assemblyName, rulesById);
                }
            }

            createRuleset(
                "AllRulesDefault.ruleset",
                "All Rules with default action",
                @"All Rules with default action. Rules with IsEnabledByDefault = false are disabled.",
                RulesetKind.AllDefault,
                categoryOpt: null);

            createRuleset(
                "AllRulesEnabled.ruleset",
                "All Rules Enabled with default action",
                "All Rules are enabled with default action. Rules with IsEnabledByDefault = false are force enabled with default action.",
                RulesetKind.AllEnabled,
                categoryOpt: null);

            createRuleset(
                "AllRulesDisabled.ruleset",
                "All Rules Disabled",
                @"All Rules are disabled.",
                RulesetKind.AllDisabled,
                categoryOpt: null);

            foreach (var category in categories)
            {
                createRuleset(
                    $"{category}RulesDefault.ruleset",
                    $"{category} Rules with default action",
                    $@"All {category} Rules with default action. Rules with IsEnabledByDefault = false or from a different category are disabled.",
                    RulesetKind.CategoryDefault,
                    categoryOpt: category);

                createRuleset(
                    $"{category}RulesEnabled.ruleset",
                    $"{category} Rules Enabled with default action",
                    $@"All {category} Rules are enabled with default action. {category} Rules with IsEnabledByDefault = false are force enabled with default action. Rules from a different category are disabled.",
                    RulesetKind.CategoryEnabled,
                    categoryOpt: category);
            }

            createPropsFile();

            return 0;

            // Local functions.
            void createRuleset(
                string rulesetFileName,
                string rulesetName,
                string rulesetDescription,
                RulesetKind rulesetKind,
                string categoryOpt)
            {
                Debug.Assert((categoryOpt != null) == (rulesetKind == RulesetKind.CategoryDefault || rulesetKind == RulesetKind.CategoryEnabled));

                var result = new StringBuilder();
                startRuleset();
                if (categoryOpt == null)
                {
                    addRules(categoryPass: false);
                }
                else
                {
                    result.AppendLine($@"   <!-- {categoryOpt} Rules -->");
                    addRules(categoryPass: true);
                    result.AppendLine();
                    result.AppendLine($@"   <!-- Other Rules -->");
                    addRules(categoryPass: false);
                }

                endRuleset();
                var directory = Directory.CreateDirectory(analyzerRulesetsDir);
                var rulesetFilePath = Path.Combine(directory.FullName, rulesetFileName);
                File.WriteAllText(rulesetFilePath, result.ToString());
                return;

                void startRuleset()
                {
                    result.AppendLine(@"<?xml version=""1.0""?>");
                    result.AppendLine($@"<RuleSet Name=""{rulesetName}"" Description=""{rulesetDescription}"" ToolsVersion=""15.0"">");
                }

                void endRuleset()
                {
                    result.AppendLine("</RuleSet>");
                }

                void addRules(bool categoryPass)
                {
                    foreach (var rulesByAssembly in allRulesByAssembly)
                    {
                        string assemblyName = rulesByAssembly.Key;
                        SortedList<string, DiagnosticDescriptor> sortedRulesById = rulesByAssembly.Value;

                        startRules(assemblyName);

                        foreach (var rule in sortedRulesById)
                        {
                            addRule(rule.Value);
                        }

                        endRules();
                    }

                    return;

                    void startRules(string assemblyName)
                    {
                        result.AppendLine($@"   <Rules AnalyzerId=""{assemblyName}"" RuleNamespace=""{assemblyName}"">");
                    }

                    void endRules()
                    {
                        result.AppendLine("   </Rules>");
                    }

                    void addRule(DiagnosticDescriptor rule)
                    {
                        if (shouldSkipRule(rule))
                        {
                            return;
                        }

                        string ruleAction = getRuleAction(rule);
                        var spacing = new string(' ', count: 15 - ruleAction.Length);
                        result.AppendLine($@"      <Rule Id=""{rule.Id}"" Action=""{ruleAction}"" /> {spacing} <!-- {rule.Title} -->");
                    }

                    bool shouldSkipRule(DiagnosticDescriptor rule)
                    {
                        switch (rulesetKind)
                        {
                            case RulesetKind.CategoryDefault:
                            case RulesetKind.CategoryEnabled:
                                if (categoryPass)
                                {
                                    return rule.Category != categoryOpt;
                                }
                                else
                                {
                                    return rule.Category == categoryOpt;
                                }

                            default:
                                return false;
                        }
                    }

                    string getRuleAction(DiagnosticDescriptor rule)
                    {
                        switch (rulesetKind)
                        {
                            case RulesetKind.CategoryDefault:
                                return getRuleActionCore(enable: categoryPass && rule.IsEnabledByDefault);

                            case RulesetKind.CategoryEnabled:
                                return getRuleActionCore(enable: categoryPass);

                            case RulesetKind.AllDefault:
                                return getRuleActionCore(enable: rule.IsEnabledByDefault);

                            case RulesetKind.AllEnabled:
                                return getRuleActionCore(enable: true);

                            case RulesetKind.AllDisabled:
                                return getRuleActionCore(enable: false);

                            default:
                                throw new InvalidProgramException();
                        }

                        string getRuleActionCore(bool enable)
                        {
                            if (enable)
                            {
                                return rule.DefaultSeverity.ToString();
                            }
                            else
                            {
                                return "None";
                            }
                        }
                    }
                }
            }

            void createPropsFile()
            {
                if (string.IsNullOrEmpty(propsFileDir) || string.IsNullOrEmpty(propsFileName))
                {
                    Debug.Assert(!containsPortedFxCopRules);
                    return;
                }

                var fileContents =
$@"<Project DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  {getEditorConfigAsAdditionalFile()}{getCodeAnalysisTreatWarningsNotAsErrors()}{getRulesetOverrides()}
</Project>";
                var directory = Directory.CreateDirectory(propsFileDir);
                var fileWithPath = Path.Combine(directory.FullName, propsFileName);
                File.WriteAllText(fileWithPath, fileContents);
            }

            string getCodeAnalysisTreatWarningsNotAsErrors()
            {
                var allRuleIds = string.Join(';', allRulesByAssembly.Values.SelectMany(l => l.Keys).Distinct());
                return $@"
  <!-- 
    This property group prevents the rule ids implemented in this package to be bumped to errors when
    the 'CodeAnalysisTreatWarningsAsErrors' = 'false'.
  -->
  <PropertyGroup Condition=""'$(CodeAnalysisTreatWarningsAsErrors)' == 'false'"">
    <WarningsNotAsErrors>$(WarningsNotAsErrors);{allRuleIds}</WarningsNotAsErrors>
  </PropertyGroup>";
            }

            string getRulesetOverrides()
            {
                if (containsPortedFxCopRules)
                {
                    var rulesetOverridesBuilder = new StringBuilder();
                    foreach (var category in categories.OrderBy(k => k))
                    {
                        // Each rule entry format is: -[Category]#[ID];
                        // For example, -Microsoft.Design#CA1001;
                        var categoryPrefix = $"      -Microsoft.{category}#";
                        var entries = allRulesByAssembly.Values
                                          .SelectMany(l => l)
                                          .Where(ruleIdAndDescriptor => ruleIdAndDescriptor.Value.Category == category &&
                                                                        FxCopWellKnownDiagnosticTags.IsPortedFxCopRule(ruleIdAndDescriptor.Value))
                                          .OrderBy(ruleIdAndDescriptor => ruleIdAndDescriptor.Key)
                                          .Select(ruleIdAndDescriptor => $"{categoryPrefix}{ruleIdAndDescriptor.Key};")
                                          .Distinct();

                        if (entries.Any())
                        {
                            rulesetOverridesBuilder.AppendLine();
                            rulesetOverridesBuilder.Append(string.Join(Environment.NewLine, entries));
                            rulesetOverridesBuilder.AppendLine();
                        }
                    }

                    if (rulesetOverridesBuilder.Length > 0)
                    {
                        return $@"

  <!-- 
    This property group contains the rules that have been implemented in this package and therefore should be disabled for the binary FxCop.
    The format is -[Category]#[ID], e.g., -Microsoft.Design#CA1001;
  -->
  <PropertyGroup>
    <CodeAnalysisRuleSetOverrides>
      $(CodeAnalysisRuleSetOverrides);{rulesetOverridesBuilder.ToString()}
    </CodeAnalysisRuleSetOverrides>
  </PropertyGroup>";
                    }
                }

                return string.Empty;
            }

            string getEditorConfigAsAdditionalFile()
            {
                return $@"
  <!-- 
    This item group adds any .editorconfig file present at the project root directory
    as an additional file.
  -->  
  <ItemGroup Condition=""'$(SkipDefaultEditorConfigAsAdditionalFile)' != 'true' And Exists('$(MSBuildProjectDirectory)\.editorconfig')"" >
    <AdditionalFiles Include=""$(MSBuildProjectDirectory)\.editorconfig"" />
  </ItemGroup>
";
            }
        }

        private enum RulesetKind
        {
            AllDefault,
            CategoryDefault,
            AllEnabled,
            CategoryEnabled,
            AllDisabled,
        }

        private sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
        {
            public static IAnalyzerAssemblyLoader Instance = new AnalyzerAssemblyLoader();

            private AnalyzerAssemblyLoader() { }
            public void AddDependencyLocation(string fullPath)
            {
            }

            public Assembly LoadFromPath(string fullPath)
            {
                return Assembly.LoadFrom(fullPath);
            }
        }
    }
}
