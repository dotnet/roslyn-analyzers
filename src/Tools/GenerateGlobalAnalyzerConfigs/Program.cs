// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Analyzer.Utilities.PooledObjects;
using Analyzer.Utilities.PooledObjects.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.ReleaseTracking;
using Microsoft.CodeAnalysis.Text;

namespace GenerateGlobalAnalyzerConfigs
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            const int expectedArguments = 9;

            if (args.Length != expectedArguments)
            {
                Console.Error.WriteLine($"Excepted {expectedArguments} arguments, found {args.Length}: {string.Join(';', args)}");
                return 1;
            }

            var outputDir = args[0];
            var packageName = args[1];
            string targetsFileDir = args[2];
            string targetsFileName = args[3];
            var assemblyList = args[4].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var binDirectory = args[5];
            var configuration = args[6];
            var tfm = args[7];
            var releaseTrackingOptOutString = args[8];

            if (!bool.TryParse(releaseTrackingOptOutString, out bool releaseTrackingOptOut))
            {
                releaseTrackingOptOut = false;
            }

            using var shippedFilesDataBuilder = ArrayBuilder<ReleaseTrackingData>.GetInstance();
            using var versionsBuilder = PooledHashSet<Version>.GetInstance();

            // Validate all assemblies exist on disk and can be loaded.
            foreach (string assembly in assemblyList)
            {
                var assemblyPath = GetAssemblyPath(assembly);
                if (!File.Exists(assemblyPath))
                {
                    Console.Error.WriteLine($"'{assemblyPath}' does not exist");
                    return 2;
                }

                try
                {
                    _ = Assembly.LoadFrom(assemblyPath);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    Console.Error.WriteLine(ex.Message);
                    return 3;
                }
            }

            // Compute descriptors by rule ID and shipped analyzer release versions and shipped data.
            var allRulesById = new SortedList<string, DiagnosticDescriptor>();
            var hasInfoOrHiddenDiagnostic = false;
            foreach (string assembly in assemblyList)
            {
                var assemblyPath = GetAssemblyPath(assembly);
                var analyzerFileReference = new AnalyzerFileReference(assemblyPath, AnalyzerAssemblyLoader.Instance);
                var analyzers = analyzerFileReference.GetAnalyzersForAllLanguages();

                foreach (var analyzer in analyzers)
                {
                    foreach (var rule in analyzer.SupportedDiagnostics)
                    {
                        allRulesById[rule.Id] = rule;
                        hasInfoOrHiddenDiagnostic = hasInfoOrHiddenDiagnostic ||
                            rule.IsEnabledByDefault && (rule.DefaultSeverity == DiagnosticSeverity.Info || rule.DefaultSeverity == DiagnosticSeverity.Hidden);
                    }
                }

                var assemblyDir = Path.GetDirectoryName(assemblyPath);
                var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                var shippedFile = Path.Combine(assemblyDir, "AnalyzerReleases", assemblyName, ReleaseTrackingHelper.ShippedFileName);
                if (!File.Exists(shippedFile) && !releaseTrackingOptOut)
                {
                    Console.Error.WriteLine($"'{shippedFile}' does not exist");
                    return 4;
                }
                else
                {
                    try
                    {
                        using var fileStream = File.OpenRead(shippedFile);
                        var sourceText = SourceText.From(fileStream);
                        var releaseTrackingData = ReleaseTrackingHelper.ReadReleaseTrackingData(shippedFile, sourceText,
                            onDuplicateEntryInRelease: (_1, _2, _3, _4, line) => throw new Exception($"Duplicate entry in {shippedFile} at {line.LineNumber}: '{line}'"),
                            onInvalidEntry: (line, _2, _3, _4) => throw new Exception($"Invalid entry in {shippedFile} at {line.LineNumber}: '{line}'"),
                            isShippedFile: true);
                        shippedFilesDataBuilder.Add(releaseTrackingData);
                        versionsBuilder.AddRange(releaseTrackingData.Versions);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Console.Error.WriteLine(ex.Message);
                        return 5;
                    }
                }
            }

            // Bail out if following conditions hold for the analyzer package:
            //  1. No Info/Hidden diagnostic in the package: No need for have different global analyzer config for build and live analysis.
            //  2. No shipped releases: User cannot choose a version specific global analyzer config.
            if (!hasInfoOrHiddenDiagnostic && versionsBuilder.Count == 0)
            {
                return 0;
            }

            var shippedFilesData = shippedFilesDataBuilder.ToImmutable();

            // Generate build and live analysis global analyzer config files for latest/unshipped version.
            CreateGlobalAnalyzerConfig(
                "BuildRules",
                "All build rules with default severity",
                "All build rules (warnings/errors) with default severity. Rules with IsEnabledByDefault = false or default severity Suggestion/Hidden are disabled.",
                EditorConfigKind.CommandLine);

            CreateGlobalAnalyzerConfig(
                "LiveAnalysisRules",
                "All rules with default severity",
                "All rules are enabled with default severity. Rules with IsEnabledByDefault = false are disabled.",
                EditorConfigKind.LiveAnalysis);

            // Generate build and live analysis global analyzer config files for each shipped version.
            foreach (var version in versionsBuilder)
            {
                var versionString = version.ToString().Replace(".", "_", StringComparison.Ordinal);
                CreateGlobalAnalyzerConfig(
                $"BuildRulesVersion{versionString}",
                $"All '{version}' build rules with default severity in",
                $"All '{version}' build rules (warnings/errors) with default severity. Rules with IsEnabledByDefault = false or first released in a version later then {version} or default severity Suggestion/Hidden are disabled.",
                EditorConfigKind.CommandLine,
                (shippedFilesData, version));

                CreateGlobalAnalyzerConfig(
                    $"LiveAnalysisRules{versionString}",
                    $"All '{version}' rules with default severity",
                    $"All '{version}' rules are enabled with default severity. Rules with IsEnabledByDefault = false or first released in a version later then {version} are disabled.",
                    EditorConfigKind.LiveAnalysis,
                    (shippedFilesData, version));
            }

            CreateTargetsFile(targetsFileDir, targetsFileName, packageName);

            return 0;

            // Local functions.
            string GetAssemblyPath(string assembly)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                var assemblyDir = Path.Combine(binDirectory, assemblyName, configuration, tfm);
                return Path.Combine(assemblyDir, assembly);
            }

            void CreateGlobalAnalyzerConfig(
                string fileName,
                string title,
                string description,
                EditorConfigKind editorConfigKind,
                (ImmutableArray<ReleaseTrackingData> shippedFiles, Version version)? shippedReleaseData = null)
            {
                // Only generate live analysis specific global analyzer config if we have at least one info or hidden diagnostic
                // that needs to be skipped during command line build.
                if (!hasInfoOrHiddenDiagnostic && editorConfigKind == EditorConfigKind.LiveAnalysis)
                {
                    return;
                }

                CreateEditorconfig(outputDir, fileName, title, description, editorConfigKind, allRulesById, shippedReleaseData);
            }
        }

        private static void CreateEditorconfig(
            string folder,
            string fileName,
            string editorconfigTitle,
            string editorconfigDescription,
            EditorConfigKind editorConfigKind,
            SortedList<string, DiagnosticDescriptor> sortedRulesById,
            (ImmutableArray<ReleaseTrackingData> shippedFiles, Version version)? shippedReleaseData)
        {
            var text = GetEditorconfigText(
                editorconfigTitle,
                editorconfigDescription,
                editorConfigKind,
                sortedRulesById,
                shippedReleaseData);

            var directory = Directory.CreateDirectory(folder);
            var editorconfigFilePath = Path.Combine(directory.FullName, $"{ fileName}.editorconfig");
            File.WriteAllText(editorconfigFilePath, text);
            return;

            // Local functions
            static string GetEditorconfigText(
                string editorconfigTitle,
                string editorconfigDescription,
                EditorConfigKind editorConfigKind,
                SortedList<string, DiagnosticDescriptor> sortedRulesById,
                (ImmutableArray<ReleaseTrackingData> shippedFiles, Version version)? shippedReleaseData)
            {
                var result = new StringBuilder();
                StartEditorconfig();
                AddRules();
                return result.ToString();

                void StartEditorconfig()
                {
                    result.AppendLine(@"# NOTE: Requires **VS2019 16.3** or later");
                    result.AppendLine();
                    result.AppendLine($@"# {editorconfigTitle}");
                    result.AppendLine($@"# Description: {editorconfigDescription}");
                    result.AppendLine();
                }

                void AddRules()
                {
                    Debug.Assert(sortedRulesById.Count > 0);

                    foreach (var rule in sortedRulesById)
                    {
                        AddRule(rule.Value);
                    }

                    return;

                    void AddRule(DiagnosticDescriptor rule)
                    {
                        var (isEnabledByDefault, severity) = GetEnabledByDefaultAndSeverity(rule);
                        string severityString = GetRuleSeverity(isEnabledByDefault, severity);

                        result.AppendLine();
                        result.AppendLine($"# {rule.Id}: {rule.Title}");
                        result.AppendLine($@"dotnet_diagnostic.{rule.Id}.severity = {severityString}");
                    }

                    (bool isEnabledByDefault, DiagnosticSeverity effectiveSeverity) GetEnabledByDefaultAndSeverity(DiagnosticDescriptor rule)
                    {
                        var isEnabledByDefault = rule.IsEnabledByDefault;
                        var effectiveSeverity = rule.DefaultSeverity;

                        if (shippedReleaseData != null)
                        {
                            isEnabledByDefault = false;
                            var maxVersion = shippedReleaseData.Value.version;
                            foreach (var shippedFile in shippedReleaseData.Value.shippedFiles)
                            {
                                if (shippedFile.TryGetLatestReleaseTrackingLine(rule.Id, maxVersion, out _, out var releaseTrackingLine) &&
                                    releaseTrackingLine.EnabledByDefault.HasValue &&
                                    releaseTrackingLine.DefaultSeverity.HasValue)
                                {
                                    isEnabledByDefault = releaseTrackingLine.EnabledByDefault.Value && !releaseTrackingLine.IsRemovedRule;
                                    effectiveSeverity = releaseTrackingLine.DefaultSeverity.Value;
                                    break;
                                }
                            }
                        }

                        return (isEnabledByDefault, effectiveSeverity);
                    }

                    string GetRuleSeverity(bool isEnabledByDefault, DiagnosticSeverity defaultSeverity)
                    {
                        return editorConfigKind switch
                        {
                            EditorConfigKind.CommandLine => GetRuleSeverityCore(enable: isEnabledByDefault &&
                                (defaultSeverity == DiagnosticSeverity.Warning || defaultSeverity == DiagnosticSeverity.Error)),

                            EditorConfigKind.LiveAnalysis => GetRuleSeverityCore(enable: isEnabledByDefault),

                            _ => throw new InvalidProgramException(),
                        };

                        string GetRuleSeverityCore(bool enable)
                        {
                            if (enable)
                            {
                                return GetSeverityString(defaultSeverity);
                            }
                            else
                            {
                                return GetSeverityString(null);
                            }
                        }

                        static string GetSeverityString(DiagnosticSeverity? severityOpt)
                        {
                            if (!severityOpt.HasValue)
                            {
                                return "none";
                            }

                            return severityOpt.Value switch
                            {
                                DiagnosticSeverity.Error => "error",
                                DiagnosticSeverity.Warning => "warning",
                                DiagnosticSeverity.Info => "suggestion",
                                DiagnosticSeverity.Hidden => "silent",
                                _ => throw new NotImplementedException(severityOpt.Value.ToString()),
                            };
                        }
                    }
                }
            }
        }

        private enum EditorConfigKind
        {
            CommandLine,
            LiveAnalysis
        }

        private static void CreateTargetsFile(string targetsFileDir, string targetsFileName, string packageName)
        {
            if (string.IsNullOrEmpty(targetsFileDir) || string.IsNullOrEmpty(targetsFileName))
            {
                return;
            }

            var fileContents =
$@"<Project>{GetCommonContents(packageName)}{GetPackageSpecificContents(packageName)}
</Project>";
            var directory = Directory.CreateDirectory(targetsFileDir);
            var fileWithPath = Path.Combine(directory.FullName, targetsFileName);
            File.WriteAllText(fileWithPath, fileContents);

            static string GetCommonContents(string packageName)
            {
                string packageVersionPropName = packageName.Replace(".", string.Empty, StringComparison.Ordinal) + "Version";
                return $@"
  <Target Name=""AddGlobalAnalyzerConfigForPackage"" BeforeTargets=""Build""  Condition=""'$(SkipGlobalAnalyzerConfigForPackage)' != 'true'"">
    <!-- PropertyGroup to compute global analyzer config file to be used -->
    <PropertyGroup>
      <!-- Use 'BuildRules' for command line build and 'LiveAnalysisRules' for live analysis -->
      <_GlobalAnalyzerConfigFileNamePrefix Condition=""'$(DesignTimeBuild)' == 'true' or '$(BuildingProject)' != 'true'"">LiveAnalysisRules</_GlobalAnalyzerConfigFileNamePrefix>
      <_GlobalAnalyzerConfigFileNamePrefix Condition=""'$(_GlobalAnalyzerConfigFileNamePrefix)' == ''"">BuildRules</_GlobalAnalyzerConfigFileNamePrefix>
  
      <!-- Optional suffix based on user specified version '{packageVersionPropName}', if any. We replace '.' with '_' to map the version string to file name suffix. -->
      <_GlobalAnalyzerConfigFileNameSuffix Condition=""'$({packageVersionPropName})' != ''"">Version$({packageVersionPropName}.Replace(""."",""_""))</_GlobalAnalyzerConfigFileNameSuffix>

      <_GlobalAnalyzerConfigFileName Condition=""'$(_GlobalAnalyzerConfigFileName)' == ''"">$(_GlobalAnalyzerConfigFileNamePrefix)$(_GlobalAnalyzerConfigFileNameSuffix).editorconfig</_GlobalAnalyzerConfigFileName>
      <_GlobalAnalyzerConfigDir Condition=""'$(_GlobalAnalyzerConfigDir)' == ''"">$(MSBuildThisFileDirectory)config</_GlobalAnalyzerConfigDir>
      <_GlobalAnalyzerConfigFile Condition=""'$(_GlobalAnalyzerConfigFile)' == ''"">$(_GlobalAnalyzerConfigDir)\$(_GlobalAnalyzerConfigFileName)</_GlobalAnalyzerConfigFile>
    </PropertyGroup>

    <ItemGroup Condition=""Exists('$(_GlobalAnalyzerConfigFile)')"">
      <GlobalAnalyzerConfigFiles Include=""$(_GlobalAnalyzerConfigFile)"" />
    </ItemGroup>
  </Target>";
            }

            static string GetPackageSpecificContents(string packageName)
            {
                if (packageName == "Microsoft.CodeAnalysis.Analyzers")
                {
                    return @"

  <!-- Workaround for https://github.com/dotnet/roslyn/issues/4655 -->
  <ItemGroup Condition=""Exists('$(MSBuildProjectDirectory)\AnalyzerReleases.Shipped.md')"" >
	<AdditionalFiles Include=""AnalyzerReleases.Shipped.md"" />
  </ItemGroup>
  <ItemGroup Condition=""Exists('$(MSBuildProjectDirectory)\AnalyzerReleases.Unshipped.md')"" >
	<AdditionalFiles Include=""AnalyzerReleases.Unshipped.md"" />
  </ItemGroup>";
                }

                return string.Empty;
            }
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
