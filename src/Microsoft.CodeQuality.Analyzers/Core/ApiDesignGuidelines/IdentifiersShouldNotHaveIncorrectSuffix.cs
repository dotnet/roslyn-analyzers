// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    /// <summary>
    /// CA1711: Identifiers should not have incorrect suffix
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class IdentifiersShouldNotHaveIncorrectSuffixAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1711";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageTypeNoAlternate = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageTypeNoAlternate), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberNewerVersion = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageMemberNewerVersion), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageTypeNewerVersion = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageTypeNewerVersion), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessageMemberWithAlternate = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixMessageMemberWithAlternate), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldNotHaveIncorrectSuffixDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources));

        private const string HelpLinkUri = "https://docs.microsoft.com/visualstudio/code-quality/ca1711-identifiers-should-not-have-incorrect-suffix";

        internal static DiagnosticDescriptor TypeNoAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeNoAlternate,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MemberNewerVersionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberNewerVersion,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor TypeNewerVersionRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageTypeNewerVersion,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);
        internal static DiagnosticDescriptor MemberWithAlternateRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessageMemberWithAlternate,
                                                                             DiagnosticCategory.Naming,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: false,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: HelpLinkUri,
                                                                             customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

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
            new Dictionary<string, ImmutableArray<string>>
            {
                [AttributeSuffix] = ImmutableArray.CreateRange(new[] { "System.Attribute" }),
                [CollectionSuffix] = ImmutableArray.CreateRange(new[] { "System.Collections.IEnumerable" }),
                [DictionarySuffix] = ImmutableArray.CreateRange(new[] { "System.Collections.IDictionary", "System.Collections.Generic.IDictionary`2", "System.Collections.Generic.IReadOnlyDictionary`2" }),
                [EventArgsSuffix] = ImmutableArray.CreateRange(new[] { "System.EventArgs" }),
                [ExceptionSuffix] = ImmutableArray.CreateRange(new[] { "System.Exception" }),
                [PermissionSuffix] = ImmutableArray.CreateRange(new[] { "System.Security.IPermission" }),
                [StreamSuffix] = ImmutableArray.CreateRange(new[] { "System.IO.Stream" }),
                [QueueSuffix] = ImmutableArray.CreateRange(new[] { "System.Collections.Queue", "System.Collections.Generic.Queue`1" }),
                [StackSuffix] = ImmutableArray.CreateRange(new[] { "System.Collections.Stack", "System.Collections.Generic.Stack`1" })
            });

        // Dictionary from type name suffix to an array containing the only types that are
        // allowed to have that suffix.
        private static readonly ImmutableDictionary<string, ImmutableArray<string>> s_suffixToAllowedTypesDictionary = ImmutableDictionary.CreateRange(
            new Dictionary<string, ImmutableArray<string>>
            {
                [DelegateSuffix] = ImmutableArray.CreateRange(new[] { "System.Delegate", "System.MulticastDelegate" }),
                [EventHandlerSuffix] = ImmutableArray.CreateRange(new[] { "System.EventHandler" }),
                [EnumSuffix] = ImmutableArray.CreateRange(new[] { "System.Enum" })
            });

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // Analyze type names.
            analysisContext.RegisterCompilationStartAction(
                compilationStartAnalysisContext =>
                {
                    var suffixToBaseTypeDictionaryBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<INamedTypeSymbol>>();

                    foreach (string suffix in s_suffixToBaseTypeNamesDictionary.Keys)
                    {
                        ImmutableArray<string> typeNames = s_suffixToBaseTypeNamesDictionary[suffix];

                        ImmutableArray<INamedTypeSymbol> namedTypeSymbolArray = ImmutableArray.CreateRange(
                            typeNames.Select(typeName => compilationStartAnalysisContext.Compilation.GetTypeByMetadataName(typeName)?.OriginalDefinition).WhereNotNull());

                        suffixToBaseTypeDictionaryBuilder.Add(suffix, namedTypeSymbolArray);
                    }

                    var suffixToBaseTypeDictionary = suffixToBaseTypeDictionaryBuilder.ToImmutableDictionary();

                    compilationStartAnalysisContext.RegisterSymbolAction(
                        (SymbolAnalysisContext symbolAnalysisContext) =>
                        {
                            var namedTypeSymbol = (INamedTypeSymbol)symbolAnalysisContext.Symbol;

                            // Note all the descriptors/rules for this analyzer have the same ID and category and hence
                            // will always have identical configured visibility.
                            if (!namedTypeSymbol.MatchesConfiguredVisibility(symbolAnalysisContext.Options, TypeNoAlternateRule, symbolAnalysisContext.CancellationToken))
                            {
                                return;
                            }

                            string name = namedTypeSymbol.Name;
                            Compilation compilation = symbolAnalysisContext.Compilation;

                            foreach (string suffix in s_suffixToBaseTypeNamesDictionary.Keys)
                            {
                                if (IsNotChildOfAnyButHasSuffix(namedTypeSymbol, suffixToBaseTypeDictionary[suffix], suffix))
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
                    var memberSymbol = context.Symbol;

                    // Note all the descriptors/rules for this analyzer have the same ID and category and hence
                    // will always have identical configured visibility.
                    if (!memberSymbol.MatchesConfiguredVisibility(context.Options, TypeNoAlternateRule, context.CancellationToken))
                    {
                        return;
                    }

                    if (memberSymbol.IsOverride || memberSymbol.IsImplementationOfAnyInterfaceMember())
                    {
                        return;
                    }

                    // If this is a method, and it's actually the getter or setter of a property,
                    // then don't complain. We'll complain about the property itself.
                    if (memberSymbol is IMethodSymbol methodSymbol && methodSymbol.IsPropertyAccessor())
                    {
                        return;
                    }

                    string name = memberSymbol.Name;

                    if (name.HasSuffix(ExSuffix))
                    {
                        context.ReportDiagnostic(
                            memberSymbol.CreateDiagnostic(MemberNewerVersionRule, ExSuffix, name));
                        return;
                    }

                    // We only fire on member suffix "New" if the type already defines
                    // another member minus the suffix, e.g., we only fire on "MemberNew" if
                    // "Member" already exists. For some reason FxCop did not apply the
                    // same logic to the "Ex" suffix, and we follow FxCop's implementation.
                    if (name.HasSuffix(NewSuffix))
                    {
                        string nameWithoutSuffix = name.WithoutSuffix(NewSuffix);
                        INamedTypeSymbol containingType = memberSymbol.ContainingType;

                        if (MemberNameExistsInHierarchy(nameWithoutSuffix, containingType, memberSymbol.Kind))
                        {
                            context.ReportDiagnostic(
                                memberSymbol.CreateDiagnostic(MemberNewerVersionRule, NewSuffix, name));
                            return;
                        }
                    }

                    if (name.HasSuffix(ImplSuffix))
                    {
                        context.ReportDiagnostic(
                            memberSymbol.CreateDiagnostic(MemberWithAlternateRule, ImplSuffix, name, CoreSuffix));
                    }
                }, SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.Property);
        }

        private static bool MemberNameExistsInHierarchy(string memberName, INamedTypeSymbol containingType, SymbolKind kind)
        {
            for (INamedTypeSymbol baseType = containingType; baseType != null; baseType = baseType.BaseType)
            {
                if (baseType.GetMembers(memberName).Any(member => member.Kind == kind))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsNotChildOfAnyButHasSuffix(INamedTypeSymbol namedTypeSymbol, ImmutableArray<INamedTypeSymbol> parentTypes, string suffix)
        {
            if (parentTypes.IsEmpty)
            {
                // Bail out if we cannot find any well-known types with the suffix in the compilation.
                return false;
            }

            return namedTypeSymbol.Name.HasSuffix(suffix)
                && !parentTypes.Any(parentType => namedTypeSymbol.DerivesFromOrImplementsAnyConstructionOf(parentType));
        }
    }
}