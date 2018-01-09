// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1716: Identifiers should not match keywords
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class IdentifiersShouldNotMatchKeywordsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1716";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotMatchKeywordsTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageMemberParameter = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotMatchKeywordsMessageMemberParameter), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMember = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotMatchKeywordsMessageMember), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageType = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotMatchKeywordsMessageType), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageNamespace = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotMatchKeywordsMessageNamespace), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotMatchKeywordsDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        // Properties common to all DiagnosticDescriptors for this rule:
        private static readonly string s_category = DiagnosticCategory.Naming;
        private const DiagnosticSeverity Severity = DiagnosticHelpers.DefaultDiagnosticSeverity;
        private const string HelpLinkUri = "https://msdn.microsoft.com/en-us/library/ms182248.aspx";
        private static readonly string[] s_customTags = new[] { WellKnownDiagnosticTags.Telemetry };

        internal static DiagnosticDescriptor MemberParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberParameter,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);
        internal static DiagnosticDescriptor MemberRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMember,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);
        internal static DiagnosticDescriptor TypeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageType,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);
        internal static DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNamespace,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);

        // Define the format in which this rule displays namespace names. The format is chosen to be
        // consistent with FxCop's display format for this rule.
        private static readonly SymbolDisplayFormat s_namespaceDisplayFormat =
            SymbolDisplayFormat.CSharpErrorMessageFormat
                // Turn off the EscapeKeywordIdentifiers flag (which is on by default), so that
                // a method named "@for" is displayed as "for"
                .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.None);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MemberParameterRule, MemberRule, TypeRule, NamespaceRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                var namespaceRuleAnalyzer = new NamespaceRuleAnalyzer();

                compilationStartAnalysisContext.RegisterSymbolAction(
                    symbolAnalysisContext => namespaceRuleAnalyzer.Analyze(symbolAnalysisContext),
                    SymbolKind.NamedType);

                compilationStartAnalysisContext.RegisterSymbolAction(AnalyzeTypeRule, SymbolKind.NamedType);

                compilationStartAnalysisContext.RegisterSymbolAction(AnalyzeMemberRule,
                    SymbolKind.Event, SymbolKind.Method, SymbolKind.Property);

                compilationStartAnalysisContext.RegisterSymbolAction(AnalyzeMemberParameterRule, SymbolKind.Method);
            });
        }

        private sealed class NamespaceRuleAnalyzer
        {
            private readonly ISet<string> _namespaceWithKeywordSet = new HashSet<string>();
            private readonly object _lockGuard = new object();

            public void Analyze(SymbolAnalysisContext context)
            {
                INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

                // Don't complain about a namespace unless it contains at least one public type.
                if (!type.IsExternallyVisible())
                {
                    return;
                }

                INamespaceSymbol containingNamespace = type.ContainingNamespace;
                if (containingNamespace.IsGlobalNamespace)
                {
                    return;
                }

                string namespaceDisplayString = containingNamespace.ToDisplayString(s_namespaceDisplayFormat);

                IEnumerable<string> namespaceNameComponents = containingNamespace.ToDisplayParts(s_namespaceDisplayFormat)
                    .Where(dp => dp.Kind == SymbolDisplayPartKind.NamespaceName)
                    .Select(dp => dp.ToString());

                foreach (string component in namespaceNameComponents)
                {
                    if (IsKeyword(component, out string matchingKeyword))
                    {
                        bool doReportDiagnostic;

                        lock (_lockGuard)
                        {
                            string namespaceWithKeyword = namespaceDisplayString + "*" + matchingKeyword;
                            doReportDiagnostic = _namespaceWithKeywordSet.Add(namespaceWithKeyword);
                        }

                        if (doReportDiagnostic)
                        {
                            // Don't report the diagnostic at a specific location. See dotnet/roslyn#8643.
                            var diagnostic = Diagnostic.Create(NamespaceRule, Location.None, namespaceDisplayString, matchingKeyword);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private void AnalyzeTypeRule(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (!type.IsExternallyVisible())
            {
                return;
            }

            if (IsKeyword(type.Name, out string matchingKeyword))
            {
                context.ReportDiagnostic(
                    type.CreateDiagnostic(
                        TypeRule,
                        type.FormatMemberName(),
                        matchingKeyword));
            }
        }

        private void AnalyzeMemberRule(SymbolAnalysisContext context)
        {
            ISymbol symbol = context.Symbol;
            if (!symbol.IsExternallyVisible())
            {
                return;
            }

            if (!IsKeyword(symbol.Name, out string matchingKeyword))
            {
                return;
            }

            // IsAbstract returns true for both abstract class members and interface members.
            if (symbol.IsVirtual || symbol.IsAbstract)
            {
                context.ReportDiagnostic(
                    symbol.CreateDiagnostic(
                        MemberRule,
                        symbol.FormatMemberName(),
                        matchingKeyword));
            }
        }

        private void AnalyzeMemberParameterRule(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            if (!method.IsExternallyVisible())
            {
                return;
            }

            // IsAbstract returns true for both abstract class members and interface members.
            if (!method.IsVirtual && !method.IsAbstract)
            {
                return;
            }

            foreach (IParameterSymbol parameter in method.Parameters)
            {
                if (IsKeyword(parameter.Name, out string matchingKeyword))
                {
                    context.ReportDiagnostic(
                        parameter.CreateDiagnostic(
                            MemberParameterRule,
                            method.FormatMemberName(),
                            parameter.Name,
                            matchingKeyword));
                }
            }
        }

        private static bool IsKeyword(string name, out string keyword)
        {
            if (s_caseSensitiveKeywords.TryGetValue(name, out keyword))
            {
                return true;
            }

            return s_caseInsensitiveKeywords.TryGetKey(name, out keyword);
        }

        private static readonly ImmutableHashSet<string> s_caseSensitiveKeywords = new[]
        {
            // C#
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
            // Listed as a keywords in Microsoft.CodeAnalysis.CSharp.SyntaxKind, but
            // omitted, at least for now, for compatibility with FxCop:
            //"__arglist",
            //"__makeref",
            //"__reftype",
            //"__refvalue",
            //"stackalloc",

            // C++
            "__abstract",
            "__alignof",
            "__asm",
            "__assume",
            "__based",
            "__box",
            "__builtin_alignof",
            "__cdecl",
            "__clrcall",
            "__compileBreak",
            "__CURSOR__",
            "__declspec",
            "__delegate",
            "__event",
            "__except",
            "__fastcall",
            "__feacp_av",
            "__feacpBreak",
            "__finally",
            "__forceinline",
            "__gc",
            "__has_assign",
            "__has_copy",
            "__has_finalizer",
            "__has_nothrow_assign",
            "__has_nothrow_copy",
            "__has_trivial_assign",
            "__has_trivial_constructor",
            "__has_trivial_copy",
            "__has_trivial_destructor",
            "__has_user_destructor",
            "__has_virtual_destructor",
            "__hook",
            "__identifier",
            "__if_exists",
            "__if_not_exists",
            "__inline",
            "__int128",
            "__int16",
            "__int32",
            "__int64",
            "__int8",
            "__interface",
            "__is_abstract",
            "__is_base_of",
            "__is_class",
            "__is_convertible_to",
            "__is_delegate",
            "__is_empty",
            "__is_enum",
            "__is_interface_class",
            "__is_pod",
            "__is_polymorphic",
            "__is_ref_array",
            "__is_ref_class",
            "__is_sealed",
            "__is_simple_value_class",
            "__is_union",
            "__is_value_class",
            "__leave",
            "__multiple_inheritance",
            "__newslot",
            "__nogc",
            "__nounwind",
            "__nvtordisp",
            "__offsetof",
            "__pin",
            "__pragma",
            "__property",
            "__ptr32",
            "__ptr64",
            "__raise",
            "__restrict",
            "__resume",
            "__sealed",
            "__single_inheritance",
            "__stdcall",
            "__super",
            "__thiscall",
            "__try",
            "__try_cast",
            "__typeof",
            "__unaligned",
            "__unhook",
            "__uuidof",
            "__value",
            "__virtual_inheritance",
            "__w64",
            "__wchar_t",
            "and",
            "and_eq",
            "asm",
            "auto",
            "bitand",
            "bitor",
            //"bool",
            //"break",
            //"case",
            //"catch",
            "cdecl",
            //"char",
            //"class",
            "compl",
            //"const",
            "const_cast",
            //"continue",
            //"default",
            "delete",
            //"do",
            //"double",
            "dynamic_cast",
            //"else",
            //"enum",
            //"explicit",
            "export",
            //"extern",
            //"false,
            //"float",
            //"for",
            "friend",
            "gcnew",
            "generic",
            //"goto",
            //"if",
            "inline",
            //"int",
            //"long",
            "mutable",
            //"namespace",
            //"new",
            "not",
            "not_eq",
            "nullptr",
            //"operator",
            "or",
            "or_eq",
            //"private",
            //"protected",
            //"public",
            "register",
            "reinterpret_cast",
            //"return",
            //"short",
            "signed",
            //"sizeof",
            //"static",
            "static_cast",
            //"struct",
            //"switch",
            "template",
            //"this",
            //"throw",
            //"true",
            //"try",
            "typedef",
            "typeid",
            "typename",
            "union",
            "unsigned",
            //"using",
            //"virtual",
            //"void",
            //"volatile",
            "wchar_t",
            //"while",
            "xor",
            "xor_eq"
        }.ToImmutableHashSet(StringComparer.Ordinal);

        private static readonly ImmutableDictionary<string, string> s_caseInsensitiveKeywords = new[]
        {
            "AddHandler",
            "AddressOf",
            "Alias",
            "And",
            "AndAlso",
            "As",
            "Boolean",
            "ByRef",
            "Byte",
            "ByVal",
            "Call",
            "Case",
            "Catch",
            "CBool",
            "CByte",
            "CChar",
            "CDate",
            "CDbl",
            "CDec",
            "Char",
            "CInt",
            "Class",
            "CLng",
            "CObj",
            "Const",
            "Continue",
            "CSByte",
            "CShort",
            "CSng",
            "CStr",
            "CType",
            "CUInt",
            "CULng",
            "CUShort",
            "Date",
            "Decimal",
            "Declare",
            "Default",
            "Delegate",
            "Dim",
            "DirectCast",
            "Do",
            "Double",
            "Each",
            "Else",
            "ElseIf",
            "End",
            "Enum",
            "Erase",
            "Error",
            "Event",
            "Exit",
            "False",
            "Finally",
            "For",
            "Friend",
            "Function",
            "Get",
            "GetType",
            "Global",
            "GoTo",
            "Handles",
            "If",
            "Implements",
            "Imports",
            "In",
            "Inherits",
            "Integer",
            "Interface",
            "Is",
            "IsNot",
            "Lib",
            "Like",
            "Long",
            "Loop",
            "Me",
            "Mod",
            "Module",
            "MustInherit",
            "MustOverride",
            "MyBase",
            "MyClass",
            "Namespace",
            "Narrowing",
            "New",
            "Next",
            "Not",
            "Nothing",
            "NotInheritable",
            "NotOverridable",
            "Object",
            "Of",
            "On",
            "Operator",
            "Option",
            "Optional",
            "Or",
            "OrElse",
            "Overloads",
            "Overridable",
            "Overrides",
            "ParamArray",
            "Partial",
            "Private",
            "Property",
            "Protected",
            "Public",
            "RaiseEvent",
            "ReadOnly",
            "ReDim",
            "REM",
            "RemoveHandler",
            "Resume",
            "Return",
            "SByte",
            "Select",
            "Set",
            "Shadows",
            "Shared",
            "Short",
            "Single",
            "Static",
            "Step",
            "Stop",
            "String",
            "Structure",
            "Sub",
            "SyncLock",
            "Then",
            "Throw",
            "To",
            "True",
            "Try",
            "TryCast",
            "TypeOf",
            "UInteger",
            "ULong",
            "UShort",
            "Using",
            "When",
            "While",
            "Widening",
            "With",
            "WithEvents",
            "WriteOnly",
            "Xor"
            // Listed as a keywords in Microsoft.CodeAnalysis.VisualBasic.SyntaxKind, but
            // omitted, at least for now, for compatibility with FxCop:
            //"Aggregate",
            //"All",
            //"Ansi",
            //"Ascending",
            //"Assembly",
            //"Async",
            //"Await",
            //"Auto",
            //"Binary",
            //"By",
            //"Compare",
            //"Custom",
            //"Descending",
            //"Disable",
            //"Distinct",
            //"Enable",
            //"EndIf",
            //"Equals",
            //"Explicit",
            //"ExternalChecksum",
            //"ExternalSource",
            //"From",
            //"GetXmlNamespace",
            //"Gosub",
            //"Group",
            //"Infer",
            //"Into",
            //"IsFalse",
            //"IsTrue",
            //"Iterator",
            //"Yield",
            //"Join",
            //"Key",
            //"Let",
            //"Mid",
            //"Off",
            //"Order",
            //"Out",
            //"Preserve",
            //"Reference",
            //"Region",
            //"Strict",
            //"Take",
            //"Text",
            //"Type",
            //"Unicode",
            //"Until",
            //"Warning",
            //"Variant",
            //"Wend",
            //"Where",
            //"Xml"
        }.ToImmutableDictionary(key => key, StringComparer.OrdinalIgnoreCase);
    }
}