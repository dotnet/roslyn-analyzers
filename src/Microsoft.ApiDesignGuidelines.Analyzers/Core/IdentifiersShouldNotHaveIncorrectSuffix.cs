// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1711: Identifiers should not have incorrect suffix
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class IdentifiersShouldNotHaveIncorrectSuffixAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1711";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageTypeNoAlternate = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageTypeNoAlternate), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberNewerVersion = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageMemberNewerVersion), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTypeNewerVersion = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageTypeNewerVersion), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberWithAlternate = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageMemberWithAlternate), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources));

        private const string HelpLinkUri = "https://msdn.microsoft.com/en-us/library/ms182247.aspx";

        internal static DiagnosticDescriptor TypeNoAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeNoAlternate,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberNewerVersionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberNewerVersion,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor TypeNewerVersionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeNewerVersion,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);
        internal static DiagnosticDescriptor MemberWithAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberWithAlternate,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            TypeNoAlternateRule,
            MemberNewerVersionRule,
            TypeNewerVersionRule,
            MemberWithAlternateRule);

        internal const string AttributeSuffix = "Attribute";
        internal const string CollectionSuffix = "Collection";
        internal const string DictionarySuffix = "Dictionary";
        internal const string EventArgsSuffix = "EventArgs";
        internal const string EventHandlerSuffix = "EventHandler";
        internal const string ExSuffix = "Ex";
        internal const string ExceptionSuffix = "Exception";
        internal const string NewSuffix = "New";
        internal const string PermissionSuffix = "Permission";
        internal const string StreamSuffix = "Stream";
        internal const string DelegateSuffix = "Delegate";
        internal const string EnumSuffix = "Enum";
        internal const string ImplSuffix = "Impl";
        internal const string CoreSuffix = "Core";
        internal const string QueueSuffix = "Queue";
        internal const string StackSuffix = "Stack";

        // Dictionary that maps from a type name suffix to the set of base types from which
        // a type with that suffix is permitted to derive.
        private static readonly ImmutableDictionary<string, ImmutableArray<string>> s_suffixToBaseTypeNamesDictionary = ImmutableDictionary.CreateRange(
            new KeyValuePair<string, ImmutableArray<string>>[]
            {
                new KeyValuePair<string, ImmutableArray<string>>(AttributeSuffix, ImmutableArray.CreateRange(new[] { "System.Attribute" })),
                new KeyValuePair<string, ImmutableArray<string>>(CollectionSuffix, ImmutableArray.CreateRange(new[] { "System.Collections.IEnumerable" })),
                new KeyValuePair<string, ImmutableArray<string>>(DictionarySuffix, ImmutableArray.CreateRange(new[] { "System.Collections.IDictionary", "System.Collections.Generic.IReadOnlyDictionary`2" })),
                new KeyValuePair<string, ImmutableArray<string>>(EventArgsSuffix, ImmutableArray.CreateRange(new[] { "System.EventArgs" })),
                new KeyValuePair<string, ImmutableArray<string>>(ExceptionSuffix, ImmutableArray.CreateRange(new[] { "System.Exception" })),
                new KeyValuePair<string, ImmutableArray<string>>(PermissionSuffix, ImmutableArray.CreateRange(new[] { "System.Security.IPermission" })),
                new KeyValuePair<string, ImmutableArray<string>>(StreamSuffix, ImmutableArray.CreateRange(new[] { "System.IO.Stream" })),
                new KeyValuePair<string, ImmutableArray<string>>(QueueSuffix, ImmutableArray.CreateRange(new[] { "System.Collections.Queue", "System.Collections.Generic.Queue`1" })),
                new KeyValuePair<string, ImmutableArray<string>>(StackSuffix, ImmutableArray.CreateRange(new[] { "System.Collections.Stack", "System.Collections.Generic.Stack`1" }))
            });

        // Dictionary from type name suffix to an array containing the only types that are
        // allowed to have that suffix.
        private static readonly ImmutableDictionary<string, ImmutableArray<string>> s_suffixToAllowedTypesDictionary = ImmutableDictionary.Create<string, ImmutableArray<string>>()
            .AddRange(new KeyValuePair<string, ImmutableArray<string>>[]
            {
                new KeyValuePair<string, ImmutableArray<string>>(DelegateSuffix, ImmutableArray.CreateRange(new[] { "System.Delegate", "System.MulticastDelegate" })),
                new KeyValuePair<string, ImmutableArray<string>>(EventHandlerSuffix, ImmutableArray.CreateRange(new[] { "System.EventHandler" })),
                new KeyValuePair<string, ImmutableArray<string>>(EnumSuffix, ImmutableArray.CreateRange(new[] { "System.Enum" }))
            });

        public override void Initialize(AnalysisContext analysisContext)
        {
            // Analyze type names.
            analysisContext.RegisterCompilationStartAction(
                (CompilationStartAnalysisContext compilationStartAnalysisContext) =>
                {
                    var suffixToBaseTypeDictionaryBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<INamedTypeSymbol>>();

                    foreach (string suffix in s_suffixToBaseTypeNamesDictionary.Keys)
                    {
                        ImmutableArray<string> typeNames = s_suffixToBaseTypeNamesDictionary[suffix];

                        ImmutableArray<INamedTypeSymbol> namedTypeSymbolArray = ImmutableArray.CreateRange(
                            typeNames.Select(typeName => compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeName).OriginalDefinition));

                        suffixToBaseTypeDictionaryBuilder.Add(suffix, namedTypeSymbolArray);
                    }

                    var suffixToBaseTypeDictionary = suffixToBaseTypeDictionaryBuilder.ToImmutableDictionary();

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            var namedTypeSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;
                            if (namedTypeSymbol.GetResultantVisibility()!= SymbolVisibility.Public)
                            {
                                return;
                            }

                            string name = namedTypeSymbol.Name;
                            Compilation compilation = symbolAnalysisContext.Compilation;

                            foreach (string suffix in s_suffixToBaseTypeNamesDictionary.Keys)
                            {
                                if (IsNotChildOfAnyButHasSuffix(
                                        namedTypeSymbol,
                                        suffixToBaseTypeDictionary[suffix],
                                        suffix,
                                        compilation))
                                {
                                    symbolAnalysisContext.ReportDiagnostic(
                                        namedTypeSymbol.CreateDiagnostic(TypeNoAlternateRule, name, suffix));
                                    return;
                                }
                            }

                            foreach (string suffix in s_suffixToAllowedTypesDictionary.Keys)
                            {
                                if (name.HasSuffix(suffix)
                                    && !s_suffixToAllowedTypesDictionary[suffix].Contains(name))
                                {
                                    symbolAnalysisContext.ReportDiagnostic(
                                        namedTypeSymbol.CreateDiagnostic(TypeNoAlternateRule, name, suffix));
                                    return;
                                }
                            }

                            if (name.HasSuffix(ImplSuffix))
                            {
                                symbolAnalysisContext.ReportDiagnostic(
                                    namedTypeSymbol.CreateDiagnostic(MemberWithAlternateRule, ImplSuffix, name, CoreSuffix));
                                return;
                            }

                            // FxCop performed the length check for "Ex", but not for any of the other
                            // suffixes, because alone among the suffixes, "Ex" is the only one that
                            // isn't itself a known type or a language keyword.
                            if (name.HasSuffix(ExSuffix) && name.Length > ExSuffix.Length)
                            {
                                symbolAnalysisContext.ReportDiagnostic(
                                    namedTypeSymbol.CreateDiagnostic(TypeNewerVersionRule, ExSuffix, name));
                                return;
                            }

                            if (name.HasSuffix(NewSuffix))
                            {
                                symbolAnalysisContext.ReportDiagnostic(
                                    namedTypeSymbol.CreateDiagnostic(TypeNewerVersionRule, NewSuffix, name));
                                return;
                            }
                        }, SymbolKind.NamedType);
                });

            // Analyze method names.
            analysisContext.RegisterSymbolAction(
                (SymbolAnalysisContext context) =>
                {
                    var methodSymbol = (IMethodSymbol)context.Symbol;
                    if (methodSymbol.GetResultantVisibility() != SymbolVisibility.Public)
                    {
                        return;
                    }

                    if (methodSymbol.IsOverride || methodSymbol.IsImplementationOfAnyInterfaceMember())
                    {
                        return;
                    }

                    string name = methodSymbol.Name;

                    if (name.HasSuffix(ExSuffix))
                    {
                        context.ReportDiagnostic(
                            methodSymbol.CreateDiagnostic(MemberNewerVersionRule, ExSuffix, name));
                        return;
                    }

                    // We only fire on member suffix "New" if the type already defines
                    // another member minus the suffix, e.g., we only fire on "MemberNew" if
                    // "Member" already exists. For some reason FxCop did not apply the
                    // same logic to the "Ex" suffix, and we follow FxCop's implementation.
                    if (name.HasSuffix(NewSuffix))
                    {
                        string nameWithoutSuffix = name.WithoutSuffix(NewSuffix);
                        INamedTypeSymbol containingType = methodSymbol.ContainingType;

                        if (MethodNameExistsInHierarchy(nameWithoutSuffix, containingType))
                        {
                            context.ReportDiagnostic(
                                methodSymbol.CreateDiagnostic(MemberNewerVersionRule, NewSuffix, name));
                            return;
                        }
                    }

                    if (name.HasSuffix(ImplSuffix))
                    {
                        context.ReportDiagnostic(
                            methodSymbol.CreateDiagnostic(MemberWithAlternateRule, ImplSuffix, name, CoreSuffix));
                    }
                }, SymbolKind.Method);
        }

        private bool MethodNameExistsInHierarchy(string methodName, INamedTypeSymbol containingType)
        {
            for (INamedTypeSymbol baseType = containingType; baseType != null; baseType = baseType.BaseType)
            {
                if (baseType.GetMembers(methodName).Any(member => member.Kind == SymbolKind.Method))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNotChildOfAnyButHasSuffix(
            INamedTypeSymbol namedTypeSymbol,
            IEnumerable<INamedTypeSymbol> parentTypes,
            string suffix,
            Compilation compilation)
        {
            return namedTypeSymbol.Name.HasSuffix(suffix)
                && !parentTypes.Any(parentType => namedTypeSymbol.DerivesFromOrImplementsAnyConstructionOf(parentType, compilation));
        }
    }
}