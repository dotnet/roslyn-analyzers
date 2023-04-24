﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable IDE0010 // Add missing cases (noise)
#pragma warning disable IDE0057 // Use range operator (incorrectly reported when Range is not defined)
#pragma warning disable IDE0058 // Expression value is never used (not sure why this is enabled)
#pragma warning disable IDE0066 // Convert switch statement to expression (not always better)

namespace Microsoft.CodeAnalysis.ResxSourceGenerator
{
    internal abstract class AbstractResxGenerator : IIncrementalGenerator
    {
        protected abstract bool SupportsNullable(Compilation compilation);

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Standard practice for diagnosing source generator failures.")]
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var resourceFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase));
            var compilationInformation = context.CompilationProvider.Select(
                (compilation, cancellationToken) =>
                {
                    var methodImplOptions = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesMethodImplOptions);
                    var hasAggressiveInlining = methodImplOptions?.MemberNames.Contains(nameof(MethodImplOptions.AggressiveInlining)) ?? false;
                    var hasNotNullIfNotNull = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemDiagnosticsCodeAnalysisNotNullIfNotNullAttribute) is not null;

                    return new CompilationInformation(
                        AssemblyName: compilation.AssemblyName,
                        CodeLanguage: compilation.Language,
                        SupportsNullable: SupportsNullable(compilation),
                        HasAggressiveInlining: hasAggressiveInlining,
                        HasNotNullIfNotNull: hasNotNullIfNotNull);
                });
            var resourceFilesToGenerateSource = resourceFiles.Combine(context.AnalyzerConfigOptionsProvider.Combine(compilationInformation)).SelectMany(
                static (resourceFileAndOptions, cancellationToken) =>
                {
                    var (resourceFile, (optionsProvider, compilationInfo)) = resourceFileAndOptions;
                    var options = optionsProvider.GetOptions(resourceFile);

                    // Use the GenerateSource property if provided. Otherwise, the value of GenerateSource defaults to
                    // true for resources without an explicit culture.
                    var explicitGenerateSource = IsGenerateSource(options);
                    if (explicitGenerateSource == false)
                    {
                        // Source generation is explicitly disabled for this resource file
                        return Array.Empty<ResourceInformation>();
                    }
                    else if (explicitGenerateSource != true)
                    {
                        var implicitGenerateSource = !IsExplicitWithCulture(options);
                        if (!implicitGenerateSource)
                        {
                            // Source generation is disabled for this resource file
                            return Array.Empty<ResourceInformation>();
                        }
                    }

                    if (!optionsProvider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace))
                    {
                        rootNamespace = compilationInfo.AssemblyName;
                    }

                    var resourceName = Path.GetFileNameWithoutExtension(resourceFile.Path);
                    if (options.TryGetValue("build_metadata.AdditionalFiles.RelativeDir", out var relativeDir))
                    {
                        resourceName = relativeDir.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.') + resourceName;
                    }

                    if (!options.TryGetValue("build_metadata.AdditionalFiles.OmitGetResourceString", out var omitGetResourceStringText)
                        || !bool.TryParse(omitGetResourceStringText, out var omitGetResourceString))
                    {
                        omitGetResourceString = false;
                    }

                    if (!options.TryGetValue("build_metadata.AdditionalFiles.AsConstants", out var asConstantsText)
                        || !bool.TryParse(asConstantsText, out var asConstants))
                    {
                        asConstants = false;
                    }

                    if (!options.TryGetValue("build_metadata.AdditionalFiles.IncludeDefaultValues", out var includeDefaultValuesText)
                        || !bool.TryParse(includeDefaultValuesText, out var includeDefaultValues))
                    {
                        includeDefaultValues = false;
                    }

                    if (!options.TryGetValue("build_metadata.AdditionalFiles.EmitFormatMethods", out var emitFormatMethodsText)
                        || !bool.TryParse(emitFormatMethodsText, out var emitFormatMethods))
                    {
                        emitFormatMethods = false;
                    }

                    return new[]
                    {
                        new ResourceInformation(
                            CompilationInformation: compilationInfo,
                            ResourceFile: resourceFile,
                            ResourceName: string.Join(".", rootNamespace, resourceName),
                            ResourceClassName: null,
                            OmitGetResourceString: omitGetResourceString,
                            AsConstants: asConstants,
                            IncludeDefaultValues: includeDefaultValues,
                            EmitFormatMethods: emitFormatMethods)
                    };
                });

            context.RegisterSourceOutput(
                resourceFilesToGenerateSource,
                static (context, resourceInformation) =>
                {
                    try
                    {
                        var impl = new Impl(resourceInformation);
                        if (impl.Execute(context.CancellationToken))
                        {
                            context.AddSource(impl.OutputTextHintName, impl.OutputText);
                        }
                    }
                    catch (Exception ex)
                    {
                        var exceptionLines = ex.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        var text = string.Join("", exceptionLines.Select(line => "#error " + line + Environment.NewLine));
                        var errorText = SourceText.From(text, Encoding.UTF8, SourceHashAlgorithm.Sha256);
                        context.AddSource($"{Path.GetFileName(resourceInformation.ResourceFile.Path)}.Error", errorText);
                    }
                });
        }

        private static bool? IsGenerateSource(AnalyzerConfigOptions options)
        {
            if (!options.TryGetValue("build_metadata.AdditionalFiles.GenerateSource", out var generateSourceText)
                || !bool.TryParse(generateSourceText, out var generateSource))
            {
                // This resource did not explicitly set GenerateSource to true or false
                return null;
            }

            return generateSource;
        }

        private static bool IsExplicitWithCulture(AnalyzerConfigOptions options)
        {
            if (!options.TryGetValue("build_metadata.AdditionalFiles.WithCulture", out var withCultureText)
                || !bool.TryParse(withCultureText, out var withCulture))
            {
                // Assume the resource does not have a culture when there is no indication otherwise
                return false;
            }

            return withCulture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AssemblyName"></param>
        /// <param name="CodeLanguage">Language of source file to generate. Supported languages: CSharp, VisualBasic.</param>
        /// <param name="SupportsNullable"></param>
        private sealed record CompilationInformation(
            string? AssemblyName,
            string CodeLanguage,
            bool SupportsNullable,
            bool HasAggressiveInlining,
            bool HasNotNullIfNotNull);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CompilationInformation">Information about the compilation.</param>
        /// <param name="ResourceFile">Resources (resx) file.</param>
        /// <param name="ResourceName">Name of the embedded resources to generate accessor class for.</param>
        /// <param name="ResourceClassName">Optionally, a <c>namespace.type</c> name for the generated Resources accessor class. Defaults to <see cref="ResourceName"/> if unspecified.</param>
        /// <param name="OmitGetResourceString">If set to <see langword="true"/>, the <c>GetResourceString</c> method is not included in the generated class and must be specified in a separate source file.</param>
        /// <param name="AsConstants">If set to <see langword="true"/>, emits constant key strings instead of properties that retrieve values.</param>
        /// <param name="IncludeDefaultValues">If set to <see langword="true"/>, calls to <c>GetResourceString</c> receive a default resource string value.</param>
        /// <param name="EmitFormatMethods">If set to <see langword="true"/>, the generated code will include <c>.FormatXYZ(...)</c> methods.</param>
        private sealed record ResourceInformation(
            CompilationInformation CompilationInformation,
            AdditionalText ResourceFile,
            string ResourceName,
            string? ResourceClassName,
            bool OmitGetResourceString,
            bool AsConstants,
            bool IncludeDefaultValues,
            bool EmitFormatMethods);

        private sealed class Impl
        {
            private const int maxDocCommentLength = 256;

            public Impl(ResourceInformation resourceInformation)
            {
                ResourceInformation = resourceInformation;
                OutputText = SourceText.From("", Encoding.UTF8);
            }

            public ResourceInformation ResourceInformation { get; }
            public CompilationInformation CompilationInformation => ResourceInformation.CompilationInformation;

            public string? OutputTextHintName { get; private set; }
            public SourceText OutputText { get; private set; }

            private enum Lang
            {
                CSharp,
                VisualBasic,
            }

            private void LogError(Lang language, string message)
            {
                var result = language switch
                {
                    Lang.CSharp => $"#error {message}",
                    Lang.VisualBasic => $"#Error \"{message}\"",
                    _ => message,
                };

                OutputText = SourceText.From(result, Encoding.UTF8, SourceHashAlgorithm.Sha256);
            }

            [MemberNotNullWhen(true, nameof(OutputTextHintName), nameof(OutputText))]
            public bool Execute(CancellationToken cancellationToken)
            {
                Lang language;
                switch (CompilationInformation.CodeLanguage)
                {
                    case LanguageNames.CSharp:
                        language = Lang.CSharp;
                        break;

                    case LanguageNames.VisualBasic:
                        language = Lang.VisualBasic;
                        break;

                    default:
                        LogError(Lang.CSharp, $"GenerateResxSource doesn't support language: '{CompilationInformation.CodeLanguage}'");
                        return false;
                }

                var extension = language switch
                {
                    Lang.CSharp => "cs",
                    Lang.VisualBasic => "vb",
                    _ => "cs",
                };

                OutputTextHintName = ResourceInformation.ResourceName + $".Designer.{extension}";

                if (string.IsNullOrEmpty(ResourceInformation.ResourceName))
                {
                    LogError(language, "ResourceName not specified");
                    return false;
                }

                var resourceAccessName = RoslynString.IsNullOrEmpty(ResourceInformation.ResourceClassName) ? ResourceInformation.ResourceName : ResourceInformation.ResourceClassName;
                SplitName(resourceAccessName, out var namespaceName, out var className);

                var classIndent = namespaceName == null ? "" : "    ";
                var memberIndent = classIndent + "    ";

                var text = ResourceInformation.ResourceFile.GetText(cancellationToken);
                if (text is null)
                {
                    LogError(language, "ResourceFile was null");
                    return false;
                }

                var strings = new StringBuilder();
                foreach (var node in XDocument.Load(new SourceTextReader(text)).Descendants("data"))
                {
                    var name = node.Attribute("name")?.Value;
                    if (name == null)
                    {
                        LogError(language, "Missing resource name");
                        return false;
                    }

                    var value = node.Elements("value").FirstOrDefault()?.Value.Trim();
                    if (value == null)
                    {
                        LogError(language, $"Missing resource value: '{name}'");
                        return false;
                    }

                    if (name.Length == 0)
                    {
                        LogError(language, $"Empty resource name");
                        return false;
                    }

                    var docCommentString = value.Length > maxDocCommentLength ? value.Substring(0, maxDocCommentLength) + " ..." : value;

                    RenderDocComment(language, memberIndent, strings, docCommentString);

                    var identifier = GetIdentifierFromResourceName(name);

                    var defaultValue = ResourceInformation.IncludeDefaultValues ? ", " + CreateStringLiteral(value, language) : string.Empty;

                    switch (language)
                    {
                        case Lang.CSharp:
                            if (ResourceInformation.AsConstants)
                            {
                                strings.AppendLine($"{memberIndent}internal const string @{identifier} = \"{name}\";");
                            }
                            else
                            {
                                var needSuppression = false;
                                if (CompilationInformation.SupportsNullable)
                                {
                                    // We need a suppression unless default values are included and the NotNullIfNotNull
                                    // attribute has been applied to eliminated the need for a suppression
                                    if (!ResourceInformation.IncludeDefaultValues || !CompilationInformation.HasNotNullIfNotNull)
                                        needSuppression = true;
                                }

                                strings.AppendLine($"{memberIndent}internal static string @{identifier} => GetResourceString(\"{name}\"{defaultValue}){(needSuppression ? "!" : "")};");
                            }

                            if (ResourceInformation.EmitFormatMethods)
                            {
                                var resourceString = new ResourceString(name, value);

                                if (resourceString.HasArguments)
                                {
                                    RenderDocComment(language, memberIndent, strings, docCommentString);
                                    RenderFormatMethod(memberIndent, language, CompilationInformation.SupportsNullable, strings, resourceString);
                                }
                            }

                            break;

                        case Lang.VisualBasic:
                            if (ResourceInformation.AsConstants)
                            {
                                strings.AppendLine($"{memberIndent}Friend Const [{identifier}] As String = \"{name}\"");
                            }
                            else
                            {
                                strings.AppendLine($"{memberIndent}Friend Shared ReadOnly Property [{identifier}] As String");
                                strings.AppendLine($"{memberIndent}  Get");
                                strings.AppendLine($"{memberIndent}    Return GetResourceString(\"{name}\"{defaultValue})");
                                strings.AppendLine($"{memberIndent}  End Get");
                                strings.AppendLine($"{memberIndent}End Property");
                            }

                            if (ResourceInformation.EmitFormatMethods)
                            {
                                throw new NotImplementedException();
                            }

                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                string? getStringMethod;
                if (ResourceInformation.OmitGetResourceString)
                {
                    getStringMethod = null;
                }
                else
                {
                    switch (language)
                    {
                        case Lang.CSharp:
                            var getResourceStringAttributes = new List<string>();
                            if (CompilationInformation.HasAggressiveInlining)
                            {
                                getResourceStringAttributes.Add("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
                            }

                            if (CompilationInformation.HasNotNullIfNotNull)
                            {
                                getResourceStringAttributes.Add("[return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull(\"defaultValue\")]");
                            }

                            getStringMethod = $@"{memberIndent}internal static global::System.Globalization.CultureInfo{(CompilationInformation.SupportsNullable ? "?" : "")} Culture {{ get; set; }}
{string.Join(Environment.NewLine, getResourceStringAttributes.Select(attr => memberIndent + attr))}
{memberIndent}internal static {(CompilationInformation.SupportsNullable ? "string?" : "string")} GetResourceString(string resourceKey, {(CompilationInformation.SupportsNullable ? "string?" : "string")} defaultValue = null) =>  ResourceManager.GetString(resourceKey, Culture) ?? defaultValue;";
                            if (ResourceInformation.EmitFormatMethods)
                            {
                                getStringMethod += $@"

{memberIndent}private static string GetResourceString(string resourceKey, string[]? formatterNames)
{memberIndent}{{
{memberIndent}   var value = GetResourceString(resourceKey) ?? """";
{memberIndent}   if (formatterNames != null)
{memberIndent}   {{
{memberIndent}       for (var i = 0; i < formatterNames.Length; i++)
{memberIndent}       {{
{memberIndent}           value = value.Replace(""{{"" + formatterNames[i] + ""}}"", ""{{"" + i + ""}}"");
{memberIndent}       }}
{memberIndent}   }}
{memberIndent}   return value;
{memberIndent}}}
";
                            }

                            break;

                        case Lang.VisualBasic:
                            getStringMethod = $@"{memberIndent}Friend Shared Property Culture As Global.System.Globalization.CultureInfo
{memberIndent}<Global.System.Runtime.CompilerServices.MethodImpl(Global.System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
{memberIndent}Friend Shared Function GetResourceString(ByVal resourceKey As String, Optional ByVal defaultValue As String = Nothing) As String
{memberIndent}    Return ResourceManager.GetString(resourceKey, Culture)
{memberIndent}End Function";
                            if (ResourceInformation.EmitFormatMethods)
                            {
                                throw new NotImplementedException();
                            }

                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                string? namespaceStart, namespaceEnd;
                if (namespaceName == null)
                {
                    namespaceStart = namespaceEnd = null;
                }
                else
                {
                    switch (language)
                    {
                        case Lang.CSharp:
                            namespaceStart = $@"namespace {namespaceName}{Environment.NewLine}{{";
                            namespaceEnd = "}";
                            break;

                        case Lang.VisualBasic:
                            namespaceStart = $"Namespace Global.{namespaceName}";
                            namespaceEnd = "End Namespace";
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                string resourceTypeName;
                string? resourceTypeDefinition;
                if (string.IsNullOrEmpty(ResourceInformation.ResourceClassName) || ResourceInformation.ResourceName == ResourceInformation.ResourceClassName)
                {
                    // resource name is same as accessor, no need for a second type.
                    resourceTypeName = className;
                    resourceTypeDefinition = null;
                }
                else
                {
                    // resource name differs from the access class, need a type for specifying the resources
                    // this empty type must remain as it is required by the .NETNative toolchain for locating resources
                    // once assemblies have been merged into the application
                    resourceTypeName = ResourceInformation.ResourceName;

                    SplitName(resourceTypeName, out var resourceNamespaceName, out var resourceClassName);
                    var resourceClassIndent = resourceNamespaceName == null ? "" : "    ";

                    switch (language)
                    {
                        case Lang.CSharp:
                            resourceTypeDefinition = $"{resourceClassIndent}internal static class {resourceClassName} {{ }}";
                            if (resourceNamespaceName != null)
                            {
                                resourceTypeDefinition = $@"namespace {resourceNamespaceName}
{{
{resourceTypeDefinition}
}}";
                            }

                            break;

                        case Lang.VisualBasic:
                            resourceTypeDefinition = $@"{resourceClassIndent}Friend Class {resourceClassName}
{resourceClassIndent}End Class";
                            if (resourceNamespaceName != null)
                            {
                                resourceTypeDefinition = $@"Namespace {resourceNamespaceName}
{resourceTypeDefinition}
End Namespace";
                            }

                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                }

                // The ResourceManager property being initialized lazily is an important optimization that lets .NETNative
                // completely remove the ResourceManager class if the disk space saving optimization to strip resources
                // (/DisableExceptionMessages) is turned on in the compiler.
                string result;
                switch (language)
                {
                    case Lang.CSharp:
                        result = $@"// <auto-generated/>

{(CompilationInformation.SupportsNullable ? "#nullable enable" : "")}
using System.Reflection;

{resourceTypeDefinition}
{namespaceStart}
{classIndent}internal static partial class {className}
{classIndent}{{
{memberIndent}private static global::System.Resources.ResourceManager{(CompilationInformation.SupportsNullable ? "?" : "")} s_resourceManager;
{memberIndent}internal static global::System.Resources.ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new global::System.Resources.ResourceManager(typeof({resourceTypeName})));
{getStringMethod}
{strings}
{classIndent}}}
{namespaceEnd}
";
                        break;

                    case Lang.VisualBasic:
                        result = $@"' <auto-generated/>

Imports System.Reflection

{resourceTypeDefinition}
{namespaceStart}
{classIndent}Friend Partial Class {className}
{memberIndent}Private Sub New
{memberIndent}End Sub
{memberIndent}
{memberIndent}Private Shared s_resourceManager As Global.System.Resources.ResourceManager
{memberIndent}Friend Shared ReadOnly Property ResourceManager As Global.System.Resources.ResourceManager
{memberIndent}    Get
{memberIndent}        If s_resourceManager Is Nothing Then
{memberIndent}            s_resourceManager = New Global.System.Resources.ResourceManager(GetType({resourceTypeName}))
{memberIndent}        End If
{memberIndent}        Return s_resourceManager
{memberIndent}    End Get
{memberIndent}End Property
{getStringMethod}
{strings}
{classIndent}End Class
{namespaceEnd}
";
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                OutputText = SourceText.From(result, Encoding.UTF8, SourceHashAlgorithm.Sha256);
                return true;
            }

            internal static string GetIdentifierFromResourceName(string name)
            {
                if (name.All(IsIdentifierPartCharacter))
                {
                    return IsIdentifierStartCharacter(name[0]) ? name : "_" + name;
                }

                var builder = new StringBuilder(name.Length);

                var f = name[0];
                if (IsIdentifierPartCharacter(f) && !IsIdentifierStartCharacter(f))
                {
                    builder.Append('_');
                }

                foreach (var c in name)
                {
                    builder.Append(IsIdentifierPartCharacter(c) ? c : '_');
                }

                return builder.ToString();

                static bool IsIdentifierStartCharacter(char ch)
                    => ch == '_' || IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));

                static bool IsIdentifierPartCharacter(char ch)
                {
                    var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                    return IsLetterChar(cat)
                        || cat == UnicodeCategory.DecimalDigitNumber
                        || cat == UnicodeCategory.ConnectorPunctuation
                        || cat == UnicodeCategory.Format
                        || cat == UnicodeCategory.NonSpacingMark
                        || cat == UnicodeCategory.SpacingCombiningMark;
                }

                static bool IsLetterChar(UnicodeCategory cat)
                {
                    switch (cat)
                    {
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.TitlecaseLetter:
                        case UnicodeCategory.ModifierLetter:
                        case UnicodeCategory.OtherLetter:
                        case UnicodeCategory.LetterNumber:
                            return true;
                    }

                    return false;
                }
            }

            private static void RenderDocComment(Lang language, string memberIndent, StringBuilder strings, string value)
            {
                var docCommentStart = language == Lang.CSharp
                    ? "///"
                    : "'''";

                var escapedTrimmedValue = new XElement("summary", value).ToString();

                foreach (var line in escapedTrimmedValue.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                {
                    strings.Append(memberIndent).Append(docCommentStart).Append(' ');
                    strings.AppendLine(line);
                }
            }

            private static string CreateStringLiteral(string original, Lang lang)
            {
                var stringLiteral = new StringBuilder(original.Length + 3);
                if (lang == Lang.CSharp)
                {
                    stringLiteral.Append('@');
                }

                stringLiteral.Append('\"');
                for (var i = 0; i < original.Length; i++)
                {
                    // duplicate '"' for VB and C#
                    if (original[i] == '\"')
                    {
                        stringLiteral.Append('"');
                    }

                    stringLiteral.Append(original[i]);
                }

                stringLiteral.Append('\"');

                return stringLiteral.ToString();
            }

            private static void SplitName(string fullName, out string? namespaceName, out string className)
            {
                var lastDot = fullName.LastIndexOf('.');
                if (lastDot == -1)
                {
                    namespaceName = null;
                    className = fullName;
                }
                else
                {
                    namespaceName = fullName.Substring(0, lastDot);
                    className = fullName.Substring(lastDot + 1);
                }
            }

            private static void RenderFormatMethod(string indent, Lang language, bool supportsNullable, StringBuilder strings, ResourceString resourceString)
            {
                strings.AppendLine($"{indent}internal static string Format{resourceString.Name}({resourceString.GetMethodParameters(language, supportsNullable)})");
                if (resourceString.UsingNamedArgs)
                {
                    strings.AppendLine($@"{indent}   => string.Format(Culture, GetResourceString(""{resourceString.Name}"", new[] {{ {resourceString.GetArgumentNames()} }}), {resourceString.GetArguments()});");
                }
                else
                {
                    strings.AppendLine($@"{indent}   => string.Format(Culture, GetResourceString(""{resourceString.Name}"") ?? """", {resourceString.GetArguments()});");
                }

                strings.AppendLine();
            }

            private class ResourceString
            {
                private static readonly Regex _namedParameterMatcher = new(@"\{([a-z]\w*)\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                private static readonly Regex _numberParameterMatcher = new(@"\{(\d+)\}", RegexOptions.Compiled);
                private readonly IReadOnlyList<string> _arguments;

                public ResourceString(string name, string value)
                {
                    Name = name;
                    Value = value;

                    var match = _namedParameterMatcher.Matches(value);
                    UsingNamedArgs = match.Count > 0;

                    if (!UsingNamedArgs)
                    {
                        match = _numberParameterMatcher.Matches(value);
                    }

                    var arguments = match.Cast<Match>()
                                         .Select(m => m.Groups[1].Value)
                                         .Distinct();
                    if (!UsingNamedArgs)
                    {
                        arguments = arguments.OrderBy(Convert.ToInt32);
                    }

                    _arguments = arguments.ToList();
                }

                public string Name { get; }

                public string Value { get; }

                public bool UsingNamedArgs { get; }

                public bool HasArguments => _arguments.Count > 0;

                public string GetArgumentNames() => string.Join(", ", _arguments.Select(a => "\"" + a + "\""));

                public string GetArguments() => string.Join(", ", _arguments.Select(GetArgName));

                public string GetMethodParameters(Lang language, bool supportsNullable)
                {
                    switch (language)
                    {
                        case Lang.CSharp:
                            return string.Join(", ", _arguments.Select(a => $"object{(supportsNullable ? "?" : "")} " + GetArgName(a)));
                        case Lang.VisualBasic:
                            return string.Join(", ", _arguments.Select(GetArgName));
                        default:
                            throw new NotImplementedException();
                    }
                }

                private string GetArgName(string name) => UsingNamedArgs ? name : 'p' + name;
            }
        }

        private sealed class SourceTextReader : TextReader
        {
            private readonly SourceText _text;
            private int _position;

            public SourceTextReader(SourceText text)
            {
                _text = text;
            }

            public override int Read(char[] buffer, int index, int count)
            {
                var remaining = _text.Length - _position;
                var charactersToRead = Math.Min(remaining, count);
                _text.CopyTo(_position, buffer, index, charactersToRead);
                _position += charactersToRead;
                return charactersToRead;
            }
        }
    }
}
