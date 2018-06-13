// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GenerateAnalyzerRulesets
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                throw new ArgumentException($"Excepted 4 arguments, found {args.Length}: {string.Join(';', args)}");
            }

            string analyzerRulesetsDir = args[0];
            string analyzerPackageName = args[1];
            string tfm = args[2];
            var assemblyList = args[3].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var allRulesByAssembly = new SortedList<string, SortedList<string, DiagnosticDescriptor>>();
            var categories = new HashSet<string>();
            foreach (string assembly in assemblyList)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                string path = Path.Combine(analyzerRulesetsDir, @"..\..", assemblyName, tfm, assembly);
                if (!File.Exists(path))
                {
                    throw new ArgumentException($"{path} does not exist", "assemblyList");
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
