// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using System.Threading;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class IdentifiersShouldDifferByMoreThanCaseAnalyzer : DiagnosticAnalyzer
    {
        public const string RuleId = "CA1708";
        public const string Namespace = "Namespaces";
        public const string Type = "Types";
        public const string Member = "Members";
        public const string Parameter = "Parameters of";

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                                      new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldDifferByMoreThanCaseTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
                                                                                      new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldDifferByMoreThanCaseMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
                                                                                      DiagnosticCategory.Naming,
                                                                                      DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                                      isEnabledByDefault: false,
                                                                                      description: new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.IdentifiersShouldDifferByMoreThanCaseDescription), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
                                                                                      helpLinkUri: "https://docs.microsoft.com/visualstudio/code-quality/ca1708-identifiers-should-differ-by-more-than-case",
                                                                                      customTags: FxCopWellKnownDiagnosticTags.PortedFxCopRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.EnableConcurrentExecution();
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            analysisContext.RegisterCompilationAction(AnalyzeCompilation);
            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            IEnumerable<INamespaceSymbol> globalNamespaces = context.Compilation.GlobalNamespace.GetNamespaceMembers()
                .Where(item => Equals(item.ContainingAssembly, context.Compilation.Assembly));

            IEnumerable<INamedTypeSymbol> globalTypes = context.Compilation.GlobalNamespace.GetTypeMembers().Where(item =>
                    Equals(item.ContainingAssembly, context.Compilation.Assembly) &&
                    MatchesConfiguredVisibility(item, context.Options, context.CancellationToken));

            CheckTypeNames(globalTypes, context.ReportDiagnostic);
            CheckNamespaceMembers(globalNamespaces, context);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = context.Symbol as INamedTypeSymbol;

            // Do not descent into non-publicly visible types by default
            // Note: This is the behavior of FxCop, it might be more correct to descend into internal but not private
            // types because "InternalsVisibleTo" could be set. But it might be bad for users to start seeing warnings
            // where they previously did not from FxCop.
            // Note that end user can now override this default behavior via options.
            if (!namedTypeSymbol.MatchesConfiguredVisibility(context.Options, Rule, context.CancellationToken))
            {
                return;
            }

            // Get externally visible members in the given type
            IEnumerable<ISymbol> members = namedTypeSymbol.GetMembers()
                                                          .Where(item => !item.IsAccessorMethod() &&
                                                                         MatchesConfiguredVisibility(item, context.Options, context.CancellationToken));

            if (members.Any())
            {
                // Check parameters names of externally visible members with parameters
                CheckParameterMembers(members, context.ReportDiagnostic);

                // Check names of externally visible type members and their members
                CheckTypeMembers(members, context.ReportDiagnostic);
            }
        }

        private static void CheckNamespaceMembers(IEnumerable<INamespaceSymbol> namespaces, CompilationAnalysisContext context)
        {
            HashSet<INamespaceSymbol> excludedNamespaces = new HashSet<INamespaceSymbol>();
            foreach (INamespaceSymbol @namespace in namespaces)
            {
                // Get all the potentially externally visible types in the namespace
                IEnumerable<INamedTypeSymbol> typeMembers = @namespace.GetTypeMembers().Where(item =>
                    Equals(item.ContainingAssembly, context.Compilation.Assembly) &&
                    MatchesConfiguredVisibility(item, context.Options, context.CancellationToken));

                if (typeMembers.Any())
                {
                    CheckTypeNames(typeMembers, context.ReportDiagnostic);
                }
                else
                {
                    // If the namespace does not contain any externally visible types then exclude it from name check
                    excludedNamespaces.Add(@namespace);
                }

                IEnumerable<INamespaceSymbol> namespaceMembers = @namespace.GetNamespaceMembers();
                if (namespaceMembers.Any())
                {
                    CheckNamespaceMembers(namespaceMembers, context);

                    // If there is a child namespace that has externally visible types, then remove the parent namespace from exclusion list
                    if (namespaceMembers.Any(item => !excludedNamespaces.Contains(item)))
                    {
                        excludedNamespaces.Remove(@namespace);
                    }
                }
            }

            // Before name check, remove all namespaces that don't contain externally visible types in current scope
            namespaces = namespaces.Where(item => !excludedNamespaces.Contains(item));

            CheckNamespaceNames(namespaces, context);
        }

        private static void CheckTypeMembers(IEnumerable<ISymbol> members, Action<Diagnostic> addDiagnostic)
        {
            // Remove constructors, indexers, operators and destructors for name check
            IEnumerable<ISymbol> membersForNameCheck = members.Where(item => !item.IsConstructor() && !item.IsDestructor() && !item.IsIndexer() && !item.IsUserDefinedOperator());
            if (membersForNameCheck.Any())
            {
                CheckMemberNames(membersForNameCheck, addDiagnostic);
            }
        }

        private static void CheckParameterMembers(IEnumerable<ISymbol> members, Action<Diagnostic> addDiagnostic)
        {
            IEnumerable<ISymbol> violatingMembers = members
                .Where(item => item.ContainingType.DelegateInvokeMethod == null && HasViolatingParameters(item));

            IEnumerable<ISymbol> violatingDelegates = members.Select(item =>
            {
                if (item is INamedTypeSymbol typeSymbol &&
    typeSymbol.DelegateInvokeMethod != null &&
    HasViolatingParameters(typeSymbol.DelegateInvokeMethod))
                {
                    return item;
                }
                else
                {
                    return null;
                }
            }).WhereNotNull();

            foreach (ISymbol symbol in violatingMembers.Concat(violatingDelegates))
            {
                addDiagnostic(symbol.CreateDiagnostic(Rule, Parameter, symbol.ToDisplayString()));
            }
        }

        #region NameCheck Methods

        private static bool HasViolatingParameters(ISymbol symbol)
        {
            ImmutableArray<IParameterSymbol> parameters = symbol.GetParameters();

            // If there is only one parameter, then return an empty collection
            if (!parameters.Skip(1).Any())
            {
                return false;
            }

            return parameters.GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase).Where((group) => group.HasMoreThan(1)).Any();
        }

        private static void CheckMemberNames(IEnumerable<ISymbol> members, Action<Diagnostic> addDiagnostic)
        {
            // If there is only one member, then return
            if (!members.Skip(1).Any())
            {
                return;
            }

            IEnumerable<ISymbol> overloadedMembers = members.Where((item) => !item.IsType()).GroupBy((item) => item.Name).Where((group) => group.HasMoreThan(1)).SelectMany((group) => group.Skip(1));
            IEnumerable<IGrouping<string, ISymbol>> memberList = members.Where((item) => !overloadedMembers.Contains(item)).GroupBy((item) => DiagnosticHelpers.GetMemberName(item), StringComparer.OrdinalIgnoreCase).Where((group) => group.HasMoreThan(1));

            foreach (IGrouping<string, ISymbol> group in memberList)
            {
                ISymbol symbol = group.First().ContainingSymbol;
                addDiagnostic(symbol.CreateDiagnostic(Rule, Member, GetSymbolDisplayString(group)));
            }
        }

        private static void CheckTypeNames(IEnumerable<ITypeSymbol> types, Action<Diagnostic> addDiagnostic)
        {
            // If there is only one type, then return
            if (!types.Skip(1).Any())
            {
                return;
            }

            IEnumerable<IGrouping<string, ITypeSymbol>> typeList = types.GroupBy((item) => DiagnosticHelpers.GetMemberName(item), StringComparer.OrdinalIgnoreCase)
                .Where((group) => group.HasMoreThan(1));

            foreach (IGrouping<string, ITypeSymbol> group in typeList)
            {
                addDiagnostic(Diagnostic.Create(Rule, Location.None, Type, GetSymbolDisplayString(group)));
            }
        }

        private static void CheckNamespaceNames(IEnumerable<INamespaceSymbol> namespaces, CompilationAnalysisContext context)
        {
            // If there is only one namespace, then return
            if (!namespaces.Skip(1).Any())
            {
                return;
            }

            IEnumerable<IGrouping<string, INamespaceSymbol>> namespaceList = namespaces.GroupBy((item) => item.ToDisplayString(), StringComparer.OrdinalIgnoreCase).Where((group) => group.HasMoreThan(1));

            foreach (IGrouping<string, INamespaceSymbol> group in namespaceList)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, Location.None, Namespace, GetSymbolDisplayString(group)));
            }
        }

        #endregion

        #region Helper Methods

        private static string GetSymbolDisplayString(IGrouping<string, ISymbol> group)
        {
            return string.Join(", ", group.Select(s => s.ToDisplayString()).OrderBy(k => k, StringComparer.Ordinal));
        }

        public static bool MatchesConfiguredVisibility(ISymbol symbol, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            var defaultAllowedVisibilties = SymbolVisibilityGroup.Public | SymbolVisibilityGroup.Internal;
            var allowedVisibilities = options.GetSymbolVisibilityGroupOption(Rule, defaultAllowedVisibilties, cancellationToken);
            return allowedVisibilities.Contains(symbol.GetResultantVisibility());
        }

        #endregion
    }
}
