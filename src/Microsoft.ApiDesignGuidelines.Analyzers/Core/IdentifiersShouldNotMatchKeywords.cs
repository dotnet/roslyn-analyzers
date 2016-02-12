// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
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
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;
        private const bool IsEnabledByDefault = true;
        private const string HelpLinkUri = "https://msdn.microsoft.com/en-us/library/ms182248.aspx";
        private static readonly string[] s_customTags = new[] { WellKnownDiagnosticTags.Telemetry };

        internal static DiagnosticDescriptor MemberParameterRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberParameter,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: IsEnabledByDefault,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);
        internal static DiagnosticDescriptor MemberRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMember,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: IsEnabledByDefault,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);
        internal static DiagnosticDescriptor TypeRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageType,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: IsEnabledByDefault,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);
        internal static DiagnosticDescriptor NamespaceRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageNamespace,
                                                                             s_category,
                                                                             Severity,
                                                                             isEnabledByDefault: IsEnabledByDefault,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: s_customTags);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MemberParameterRule, MemberRule, TypeRule, NamespaceRule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterSymbolAction(AnalyzeTypeRule,
                SymbolKind.NamedType);

            analysisContext.RegisterSymbolAction(AnalyzeMemberRule,
                SymbolKind.Event, SymbolKind.Method, SymbolKind.Property);

            analysisContext.RegisterSymbolAction(AnalyzeMemberParameterRule,
                SymbolKind.Method);
        }

        private void AnalyzeTypeRule(SymbolAnalysisContext context)
        {
            INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;
            if (type.GetResultantVisibility() != SymbolVisibility.Public)
            {
                return;
            }

            string matchingKeyword;
            if (IsKeyword(type.Name, out matchingKeyword))
            {
                context.ReportDiagnostic(
                    type.CreateDiagnostic(
                        TypeRule,
                        FormatSymbolName(type),
                        matchingKeyword));
            }
        }

        private void AnalyzeMemberRule(SymbolAnalysisContext context)
        {
            ISymbol symbol = context.Symbol;
            if (symbol.GetResultantVisibility() != SymbolVisibility.Public)
            {
                return;
            }

            string matchingKeyword;
            if (!IsKeyword(symbol.Name, out matchingKeyword))
            {
                return;
            }

            // IsAbstract returns true for both abstract class members and interface members.
            if (symbol.IsVirtual || symbol.IsAbstract)
            {
                context.ReportDiagnostic(
                    symbol.CreateDiagnostic(
                        MemberRule,
                        FormatSymbolName(symbol),
                        matchingKeyword));
            }
        }

        private void AnalyzeMemberParameterRule(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            if (method.GetResultantVisibility() != SymbolVisibility.Public)
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
                string matchingKeyword;
                if (IsKeyword(parameter.Name, out matchingKeyword))
                {
                    context.ReportDiagnostic(
                        parameter.CreateDiagnostic(
                            MemberParameterRule,
                            FormatSymbolName(method),
                            parameter.Name,
                            matchingKeyword));
                }
            }
        }

        private bool IsKeyword(string name, out string keyword)
        {
            if (_caseSensitiveKeywords.TryGetValue(name, out keyword))
            {
                return true;
            }

            return _caseInsensitiveKeywords.TryGetKey(name, out keyword);
        }

        // Format the symbol name in a way consistent with FxCop's display for this rule.
        private static string FormatSymbolName(ISymbol symbol)
        {
            return symbol.ToDisplayString(
                // This format omits the namespace.
                SymbolDisplayFormat.CSharpShortErrorMessageFormat
                    // Turn off the EscapeKeywordIdentifiers flag (which is on by default), so that
                    // a method named "@for" is displayed as "for".
                    // Turn on the UseSpecialTypes flat (which is off by default), so that parameter
                    // names of "special" types such as Int32 are displayed as their language alias,
                    // such as int for C# and Integer for VB.
                    .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }

        private readonly ImmutableHashSet<string> _caseSensitiveKeywords = new string[]
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

        private readonly ImmutableDictionary<string, string> _caseInsensitiveKeywords = new string[]
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