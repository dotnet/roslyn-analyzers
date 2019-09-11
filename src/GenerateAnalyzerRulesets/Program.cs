// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
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
            const int expectedArguments = 13;

            if (args.Length != expectedArguments)
            {
                Console.Error.WriteLine($"Excepted {expectedArguments} arguments, found {args.Length}: {string.Join(';', args)}");
                return 1;
            }

            string analyzerRulesetsDir = args[0];
            string binDirectory = args[1];
            string configuration = args[2];
            string tfm = args[3];
            var assemblyList = args[4].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string propsFileDir = args[5];
            string propsFileName = args[6];
            string analyzerDocumentationFileDir = args[7];
            string analyzerDocumentationFileName = args[8];
            string analyzerSarifFileDir = args[9];
            string analyzerSarifFileName = args[10];
            var analyzerVersion = args[11];
            if (!bool.TryParse(args[12], out var containsPortedFxCopRules))
            {
                containsPortedFxCopRules = false;
            }

            var allRulesByAssembly = new SortedList<string, SortedList<string, DiagnosticDescriptor>>();
            var allRulesById = new SortedList<string, DiagnosticDescriptor>();
            var fixableDiagnosticIds = new HashSet<string>();
            var categories = new HashSet<string>();
            var rulesMetadata = new SortedList<string, (string path, SortedList<string, (DiagnosticDescriptor rule, string typeName, string[] languages)> rules)>();
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
                var rulesById = new SortedList<string, DiagnosticDescriptor>();

                var assemblyRulesMetadata = (path, rules: new SortedList<string, (DiagnosticDescriptor rule, string typeName, string[] languages)>());

                foreach (var analyzer in analyzers)
                {
                    var analyzerType = analyzer.GetType();

                    foreach (var rule in analyzer.SupportedDiagnostics)
                    {
                        rulesById[rule.Id] = rule;
                        allRulesById[rule.Id] = rule;
                        categories.Add(rule.Category);
                        assemblyRulesMetadata.rules[rule.Id] = (rule, analyzerType.Name, analyzerType.GetCustomAttribute<DiagnosticAnalyzerAttribute>(true)?.Languages);
                    }
                }

                allRulesByAssembly.Add(assemblyName, rulesById);
                rulesMetadata.Add(assemblyName, assemblyRulesMetadata);

                foreach (var id in analyzerFileReference.GetFixers().SelectMany(fixer => fixer.FixableDiagnosticIds))
                {
                    fixableDiagnosticIds.Add(id);
                }
            }

            createRuleset(
                "AllRulesDefault.ruleset",
                "All Rules with default action",
                @"All Rules with default action. Rules with IsEnabledByDefault = false are disabled.",
                RulesetKind.AllDefault);

            createRuleset(
                "AllRulesEnabled.ruleset",
                "All Rules Enabled with default action",
                "All Rules are enabled with default action. Rules with IsEnabledByDefault = false are force enabled with default action.",
                RulesetKind.AllEnabled);

            createRuleset(
                "AllRulesDisabled.ruleset",
                "All Rules Disabled",
                @"All Rules are disabled.",
                RulesetKind.AllDisabled);

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

            // We generate custom tag based rulesets only for select custom tags.
            var customTagsToGenerateRulesets = ImmutableArray.Create(
                WellKnownDiagnosticTagsExtensions.Dataflow,
                FxCopWellKnownDiagnosticTags.PortedFromFxCop);

            foreach (var customTag in customTagsToGenerateRulesets)
            {
                createRuleset(
                    $"{customTag}RulesDefault.ruleset",
                    $"{customTag} Rules with default action",
                    $@"All {customTag} Rules with default action. Rules with IsEnabledByDefault = false and non-{customTag} rules are disabled.",
                    RulesetKind.CustomTagDefault,
                    customTagOpt: customTag);

                createRuleset(
                    $"{customTag}RulesEnabled.ruleset",
                    $"{customTag} Rules Enabled with default action",
                    $@"All {customTag} Rules are enabled with default action. {customTag} Rules with IsEnabledByDefault = false are force enabled with default action. Non-{customTag} Rules are disabled.",
                    RulesetKind.CustomTagEnabled,
                    customTagOpt: customTag);
            }

            createPropsFile();

            createAnalyzerDocumentationFile();

            createAnalyzerSarifFile();

            return 0;

            // Local functions.
            void createRuleset(
                string rulesetFileName,
                string rulesetName,
                string rulesetDescription,
                RulesetKind rulesetKind,
                string categoryOpt = null,
                string customTagOpt = null)
            {
                Debug.Assert(categoryOpt == null || customTagOpt == null);
                Debug.Assert((categoryOpt != null) == (rulesetKind == RulesetKind.CategoryDefault || rulesetKind == RulesetKind.CategoryEnabled));
                Debug.Assert((customTagOpt != null) == (rulesetKind == RulesetKind.CustomTagDefault || rulesetKind == RulesetKind.CustomTagEnabled));

                var result = new StringBuilder();
                startRuleset();
                if (categoryOpt == null && customTagOpt == null)
                {
                    addRules(categoryPass: false, customTagPass: false);
                }
                else
                {
                    result.AppendLine($@"   <!-- {categoryOpt ?? customTagOpt} Rules -->");
                    addRules(categoryPass: categoryOpt != null, customTagPass: customTagOpt != null);
                    result.AppendLine();
                    result.AppendLine($@"   <!-- Other Rules -->");
                    addRules(categoryPass: false, customTagPass: false);
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

                void addRules(bool categoryPass, bool customTagPass)
                {
                    foreach (var rulesByAssembly in allRulesByAssembly)
                    {
                        string assemblyName = rulesByAssembly.Key;
                        SortedList<string, DiagnosticDescriptor> sortedRulesById = rulesByAssembly.Value;

                        if (!sortedRulesById.Any(r => !shouldSkipRule(r.Value)))
                        {
                            // Bail out if we don't have any rule to be added for this assembly.
                            continue;
                        }

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

                            case RulesetKind.CustomTagDefault:
                            case RulesetKind.CustomTagEnabled:
                                if (customTagPass)
                                {
                                    return !rule.CustomTags.Contains(customTagOpt);
                                }
                                else
                                {
                                    return rule.CustomTags.Contains(customTagOpt);
                                }

                            default:
                                return false;
                        }
                    }

                    string getRuleAction(DiagnosticDescriptor rule)
                    {
                        return rulesetKind switch
                        {
                            RulesetKind.CategoryDefault => getRuleActionCore(enable: categoryPass && rule.IsEnabledByDefault),

                            RulesetKind.CategoryEnabled => getRuleActionCore(enable: categoryPass),

                            RulesetKind.CustomTagDefault => getRuleActionCore(enable: customTagPass && rule.IsEnabledByDefault),

                            RulesetKind.CustomTagEnabled => getRuleActionCore(enable: customTagPass),

                            RulesetKind.AllDefault => getRuleActionCore(enable: rule.IsEnabledByDefault),

                            RulesetKind.AllEnabled => getRuleActionCore(enable: true),

                            RulesetKind.AllDisabled => getRuleActionCore(enable: false),

                            _ => throw new InvalidProgramException(),
                        };

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
  {getEditorConfigAsAdditionalFile()}{getCodeAnalysisTreatWarningsNotAsErrors()}{getRulesetOverrides()}{getFlowAnalysisFeatureFlag()}
</Project>";
                var directory = Directory.CreateDirectory(propsFileDir);
                var fileWithPath = Path.Combine(directory.FullName, propsFileName);
                File.WriteAllText(fileWithPath, fileContents);
            }

            static string getFlowAnalysisFeatureFlag()
            {
                return @"

  <PropertyGroup>
    <Features>$(Features);flow-analysis</Features> 
  </PropertyGroup>";
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

            static string getEditorConfigAsAdditionalFile()
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

            void createAnalyzerDocumentationFile()
            {
                if (string.IsNullOrEmpty(analyzerDocumentationFileDir) || string.IsNullOrEmpty(analyzerDocumentationFileName) || allRulesById.Count == 0)
                {
                    Debug.Assert(!containsPortedFxCopRules);
                    return;
                }

                var directory = Directory.CreateDirectory(analyzerDocumentationFileDir);
                var fileWithPath = Path.Combine(directory.FullName, analyzerDocumentationFileName);

                var builder = new StringBuilder();
                builder.Append(@"
Sr. No. | Rule ID | Title | Category | Enabled | CodeFix | Description |
--------|---------|-------|----------|---------|---------|--------------------------------------------------------------------------------------------------------------|
");

                var index = 1;
                foreach (var ruleById in allRulesById)
                {
                    string ruleId = ruleById.Key;
                    DiagnosticDescriptor descriptor = ruleById.Value;

                    var ruleIdWithHyperLink = descriptor.Id;
                    if (!string.IsNullOrWhiteSpace(descriptor.HelpLinkUri))
                    {
                        ruleIdWithHyperLink = $"[{ruleIdWithHyperLink}]({descriptor.HelpLinkUri})";
                    }

                    var hasCodeFix = fixableDiagnosticIds.Contains(descriptor.Id);

                    var description = descriptor.Description.ToString();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        description = descriptor.MessageFormat.ToString();
                    }

                    builder.AppendLine($"{index} | {ruleIdWithHyperLink} | {descriptor.Title} | {descriptor.Category} | {descriptor.IsEnabledByDefault} | {hasCodeFix} | {description} |");
                    index++;
                }

                File.WriteAllText(fileWithPath, builder.ToString());
            }

            // based on https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/CommandLine/ErrorLogger.cs
            void createAnalyzerSarifFile()
            {
                if (string.IsNullOrEmpty(analyzerSarifFileDir) || string.IsNullOrEmpty(analyzerSarifFileName) || allRulesById.Count == 0)
                {
                    Debug.Assert(!containsPortedFxCopRules);
                    return;
                }

                var culture = new CultureInfo("en-us");

                var directory = Directory.CreateDirectory(analyzerSarifFileDir);
                var fileWithPath = Path.Combine(directory.FullName, analyzerSarifFileName);

                using var textWriter = new StreamWriter(fileWithPath, false, Encoding.UTF8);
                using var writer = new Roslyn.Utilities.JsonWriter(textWriter);
                writer.WriteObjectStart(); // root
                writer.Write("$schema", "http://json.schemastore.org/sarif-1.0.0");
                writer.Write("version", "1.0.0");
                writer.WriteArrayStart("runs");

                foreach (var assemblymetadata in rulesMetadata)
                {
                    writer.WriteObjectStart(); // run

                    writer.WriteObjectStart("tool");
                    writer.Write("name", assemblymetadata.Key);

                    if (!string.IsNullOrWhiteSpace(analyzerVersion))
                    {
                        writer.Write("version", analyzerVersion);
                    }

                    writer.Write("language", culture.Name);
                    writer.WriteObjectEnd(); // tool

                    writer.WriteObjectStart("rules"); // rules

                    foreach (var rule in assemblymetadata.Value.rules)
                    {
                        var ruleId = rule.Key;
                        var descriptor = rule.Value.rule;

                        writer.WriteObjectStart(descriptor.Id); // rule
                        writer.Write("id", descriptor.Id);

                        writer.Write("shortDescription", descriptor.Title.ToString(culture));

                        string fullDescription = descriptor.Description.ToString(culture);
                        writer.Write("fullDescription", !string.IsNullOrEmpty(fullDescription) ? fullDescription : descriptor.MessageFormat.ToString());

                        writer.Write("defaultLevel", getLevel(descriptor.DefaultSeverity));

                        if (!string.IsNullOrEmpty(descriptor.HelpLinkUri))
                        {
                            writer.Write("helpUri", descriptor.HelpLinkUri);
                        }

                        writer.WriteObjectStart("properties");

                        writer.Write("category", descriptor.Category);

                        writer.Write("isEnabledByDefault", descriptor.IsEnabledByDefault);

                        writer.Write("typeName", rule.Value.typeName);

                        if ((rule.Value.languages?.Length ?? 0) > 0)
                        {
                            writer.WriteArrayStart("languages");

                            foreach (var language in rule.Value.languages.OrderBy(l => l, StringComparer.InvariantCultureIgnoreCase))
                            {
                                writer.Write(language);
                            }

                            writer.WriteArrayEnd(); // languages
                        }

                        if (descriptor.CustomTags.Any())
                        {
                            writer.WriteArrayStart("tags");

                            foreach (string tag in descriptor.CustomTags)
                            {
                                writer.Write(tag);
                            }

                            writer.WriteArrayEnd(); // tags
                        }

                        writer.WriteObjectEnd(); // properties
                        writer.WriteObjectEnd(); // rule
                    }

                    writer.WriteObjectEnd(); // rules
                    writer.WriteObjectEnd(); // run
                }

                writer.WriteArrayEnd(); // runs
                writer.WriteObjectEnd(); // root

                return;
                static string getLevel(DiagnosticSeverity severity)
                {
                    switch (severity)
                    {
                        case DiagnosticSeverity.Info:
                            return "note";

                        case DiagnosticSeverity.Error:
                            return "error";

                        case DiagnosticSeverity.Warning:
                            return "warning";

                        case DiagnosticSeverity.Hidden:
                        default:
                            // hidden diagnostics are not reported on the command line and therefore not currently given to 
                            // the error logger. We could represent it with a custom property in the SARIF log if that changes.
                            Debug.Assert(false);
                            goto case DiagnosticSeverity.Warning;
                    }
                }
            }
        }

        private enum RulesetKind
        {
            AllDefault,
            CategoryDefault,
            CustomTagDefault,
            AllEnabled,
            CategoryEnabled,
            CustomTagEnabled,
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
