// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Analyzer.Utilities;
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
                Console.Error.WriteLine($"Expected {expectedArguments} arguments, found {args.Length}: {string.Join(';', args)}");
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
            var sawShippedFile = false;
            foreach (string assembly in assemblyList)
            {
                var assemblyPath = GetAssemblyPath(assembly);
                var analyzerFileReference = new AnalyzerFileReference(assemblyPath, AnalyzerAssemblyLoader.Instance);
                analyzerFileReference.AnalyzerLoadFailed += AnalyzerFileReference_AnalyzerLoadFailed;
                var analyzers = analyzerFileReference.GetAnalyzersForAllLanguages();

                foreach (var analyzer in analyzers)
                {
                    foreach (var rule in analyzer.SupportedDiagnostics)
                    {
                        allRulesById[rule.Id] = rule;
                    }
                }

                var assemblyDir = Path.GetDirectoryName(assemblyPath);
                if (assemblyDir is null)
                {
                    continue;
                }
                var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                var shippedFile = Path.Combine(assemblyDir, "AnalyzerReleases", assemblyName, ReleaseTrackingHelper.ShippedFileName);
                if (File.Exists(shippedFile))
                {
                    sawShippedFile = true;

                    if (releaseTrackingOptOut)
                    {
                        Console.Error.WriteLine($"'{shippedFile}' exists but was not expected");
                        return 4;
                    }

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

            if (!releaseTrackingOptOut && !sawShippedFile)
            {
                Console.Error.WriteLine($"Could not find any 'AnalyzerReleases.Shipped.md' file");
                return 6;
            }

            // Bail out if there are no shipped releases: User cannot choose a version specific global analyzer config.
            if (versionsBuilder.Count == 0)
            {
                return 0;
            }

            var shippedFilesData = shippedFilesDataBuilder.ToImmutable();

            // Generate global analyzer config files for each shipped version, if required.
            foreach (var version in versionsBuilder)
            {
                var versionString = version.ToString().Replace(".", "_", StringComparison.Ordinal);
                CreateEditorconfigIfRequired(
                    outputDir,
                    $"RulesVersion{versionString}",
                    $"Rules from '{version}' release",
                    $"Rules with default severity and enabled state from '{version}' release. Rules that are first released in a version later then '{version}' are disabled.",
                    allRulesById,
                    (shippedFilesData, version));
            }

            CreateTargetsFile(targetsFileDir, targetsFileName, packageName);

            return 0;

            // Local functions.
            static void AnalyzerFileReference_AnalyzerLoadFailed(object? sender, AnalyzerLoadFailureEventArgs e)
                => throw e.Exception;

            string GetAssemblyPath(string assembly)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(assembly);
                var assemblyDir = Path.Combine(binDirectory, assemblyName, configuration, tfm);
                return Path.Combine(assemblyDir, assembly);
            }
        }

        private static void CreateEditorconfigIfRequired(
            string folder,
            string subFolder,
            string editorconfigTitle,
            string editorconfigDescription,
            SortedList<string, DiagnosticDescriptor> sortedRulesById,
            (ImmutableArray<ReleaseTrackingData> shippedFiles, Version version) shippedReleaseData)
        {
            var text = GetEditorconfigText(
                editorconfigTitle,
                editorconfigDescription,
                sortedRulesById,
                shippedReleaseData);
            if (text == null)
            {
                // Identical set of rules + default severity and enabled state for rules between this release and latest.
                // We don't need to generate a global editor config.
                return;
            }

            var directory = Directory.CreateDirectory(Path.Combine(folder, subFolder));
            var editorconfigFilePath = Path.Combine(directory.FullName, ".editorconfig");
            File.WriteAllText(editorconfigFilePath, text);
            return;

            // Local functions
            static string? GetEditorconfigText(
                string editorconfigTitle,
                string editorconfigDescription,
                SortedList<string, DiagnosticDescriptor> sortedRulesById,
                (ImmutableArray<ReleaseTrackingData> shippedFiles, Version version)? shippedReleaseData)
            {
                var result = new StringBuilder();
                StartEditorconfig();
                var added = AddRules();
                return added ? result.ToString() : null;

                void StartEditorconfig()
                {
                    result.AppendLine(@"# NOTE: Requires **VS2019 16.7** or later");
                    result.AppendLine();
                    result.AppendLine($@"# {editorconfigTitle}");
                    result.AppendLine($@"# Description: {editorconfigDescription}");
                    result.AppendLine();
                    result.AppendLine($@"is_global = true");
                    result.AppendLine();
                }

                bool AddRules()
                {
                    Debug.Assert(sortedRulesById.Count > 0);

                    var addedRule = false;
                    foreach (var rule in sortedRulesById)
                    {
                        if (AddRule(rule.Value))
                        {
                            addedRule = true;
                        }
                    }

                    return addedRule;

                    bool AddRule(DiagnosticDescriptor rule)
                    {
                        var (isEnabledByDefault, severity) = GetEnabledByDefaultAndSeverity(rule);
                        if (rule.IsEnabledByDefault == isEnabledByDefault &&
                            severity == rule.DefaultSeverity)
                        {
                            // Rule had the same default severity and enabled state in the release.
                            // We do not need to generate any entry.
                            return false;
                        }

                        string severityString = GetRuleSeverity(isEnabledByDefault, severity);

                        result.AppendLine();
                        result.AppendLine($"# {rule.Id}: {rule.Title}");
                        result.AppendLine($@"dotnet_diagnostic.{rule.Id}.severity = {severityString}");
                        return true;
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

                    static string GetRuleSeverity(bool isEnabledByDefault, DiagnosticSeverity defaultSeverity)
                    {
                        if (isEnabledByDefault)
                        {
                            return GetSeverityString(defaultSeverity);
                        }
                        else
                        {
                            return GetSeverityString(null);
                        }

                        static string GetSeverityString(DiagnosticSeverity? severity)
                        {
                            if (!severity.HasValue)
                            {
                                return "none";
                            }

                            return severity.Value switch
                            {
                                DiagnosticSeverity.Error => "error",
                                DiagnosticSeverity.Warning => "warning",
                                DiagnosticSeverity.Info => "suggestion",
                                DiagnosticSeverity.Hidden => "silent",
                                _ => throw new NotImplementedException(severity.Value.ToString()),
                            };
                        }
                    }
                }
            }
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
                => GetGlobalAnalyzerConfigTargetContents(packageName) + GetMSBuildContentForPropertyAndItemOptions();

            static string GetGlobalAnalyzerConfigTargetContents(string packageName)
            {
                var trimmedPackageName = packageName.Replace(".", string.Empty, StringComparison.Ordinal);
                string packageVersionPropName = trimmedPackageName + "RulesVersion";
                return $@"
  <Target Name=""AddGlobalAnalyzerConfigForPackage_{trimmedPackageName}"" BeforeTargets=""CoreCompile"" Condition=""'$(SkipGlobalAnalyzerConfigForPackage)' != 'true'"">
    <!-- PropertyGroup to compute global analyzer config file to be used -->
    <PropertyGroup>
      <!-- GlobalAnalyzerConfig folder name based on user specified package version '{packageVersionPropName}', if any. We replace '.' with '_' to map the version string to file name suffix. -->
      <_GlobalAnalyzerConfigFolderName Condition=""'$({packageVersionPropName})' != ''"">RulesVersion$({packageVersionPropName}.Replace(""."",""_""))</_GlobalAnalyzerConfigFolderName>
      
      <_GlobalAnalyzerConfigDir Condition=""'$(_GlobalAnalyzerConfigDir)' == ''"">$(MSBuildThisFileDirectory)config\$(_GlobalAnalyzerConfigFolderName)</_GlobalAnalyzerConfigDir>
      <_GlobalAnalyzerConfigFile>$(_GlobalAnalyzerConfigDir)\.editorconfig</_GlobalAnalyzerConfigFile>
    </PropertyGroup>

    <ItemGroup Condition=""Exists('$(_GlobalAnalyzerConfigFile)')"">
      <EditorConfigFiles Include=""$(_GlobalAnalyzerConfigFile)"" />
    </ItemGroup>
  </Target>
";
            }

            static string GetMSBuildContentForPropertyAndItemOptions()
            {
                var builder = new StringBuilder();

                AddMSBuildContentForPropertyOptions(builder);
                AddMSBuildContentForItemOptions(builder);

                return builder.ToString();

                static void AddMSBuildContentForPropertyOptions(StringBuilder builder)
                {
                    var compilerVisibleProperties = new List<string>();
                    foreach (var field in typeof(MSBuildPropertyOptionNames).GetFields())
                    {
                        compilerVisibleProperties.Add(field.Name);
                    }

                    // Add ItemGroup for MSBuild property names that are required to be threaded as analyzer config options.
                    AddItemGroupForCompilerVisibleProperties(compilerVisibleProperties, builder);
                }

                static void AddItemGroupForCompilerVisibleProperties(List<string> compilerVisibleProperties, StringBuilder builder)
                {
                    builder.AppendLine($@"
  <!-- MSBuild properties to thread to the analyzers as options --> 
  <ItemGroup>");
                    foreach (var compilerVisibleProperty in compilerVisibleProperties)
                    {
                        builder.AppendLine($@"    <CompilerVisibleProperty Include=""{compilerVisibleProperty}"" />");
                    }

                    builder.AppendLine($@"  </ItemGroup>");
                }

                static void AddMSBuildContentForItemOptions(StringBuilder builder)
                {
                    // Add ItemGroup and PropertyGroup for MSBuild item names that are required to be threaded as analyzer config options.
                    // The analyzer config option will have the following key/value:
                    // - Key: Item name prefixed with an '_' and suffixed with a 'List' to reduce chances of conflicts with any existing project property.
                    // - Value: Concatenated item metadata values, separate by a ';' character. See https://github.com/dotnet/sdk/issues/12706#issuecomment-668219422 for details.

                    builder.Append($@"
  <!-- MSBuild item metadata to thread to the analyzers as options -->
  <PropertyGroup>
");
                    var compilerVisibleProperties = new List<string>();
                    foreach (var field in typeof(MSBuildItemOptionNames).GetFields())
                    {
                        // Item option name: "SupportedPlatform"
                        // Generated MSBuild property: "<_SupportedPlatformList>@(SupportedPlatform, '<separator>')</_SupportedPlatformList>"

                        var itemOptionName = field.Name;
                        var propertyName = MSBuildItemOptionNamesHelpers.GetPropertyNameForItemOptionName(itemOptionName);
                        compilerVisibleProperties.Add(propertyName);
                        builder.AppendLine($@"    <{propertyName}>@({itemOptionName}, '{MSBuildItemOptionNamesHelpers.ValuesSeparator}')</{propertyName}>");
                    }

                    builder.AppendLine($@"  </PropertyGroup>");

                    AddItemGroupForCompilerVisibleProperties(compilerVisibleProperties, builder);
                }
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
                else if (packageName == "Microsoft.CodeAnalysis.PublicApiAnalyzers")
                {
                    return @"

  <!-- Workaround for https://github.com/dotnet/roslyn/issues/4655 -->
  <ItemGroup Condition=""Exists('$(MSBuildProjectDirectory)\PublicAPI.Shipped.txt')"" >
	<AdditionalFiles Include=""PublicAPI.Shipped.txt"" />
  </ItemGroup>
  <ItemGroup Condition=""Exists('$(MSBuildProjectDirectory)\PublicAPI.Unshipped.txt')"" >
	<AdditionalFiles Include=""PublicAPI.Unshipped.txt"" />
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
