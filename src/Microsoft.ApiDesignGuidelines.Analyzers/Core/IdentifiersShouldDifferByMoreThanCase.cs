// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis;
using Analyzer.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers
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
                                                                                      new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldDifferByMoreThanCaseTitle), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources)),
                                                                                      new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldDifferByMoreThanCaseMessage), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources)),
                                                                                      DiagnosticCategory.Naming,
                                                                                      DiagnosticSeverity.Warning,
                                                                                      isEnabledByDefault: false,
                                                                                      description: new LocalizableResourceString(nameof(MicrosoftApiDesignGuidelinesAnalyzersResources.IdentifiersShouldDifferByMoreThanCaseDescription), MicrosoftApiDesignGuidelinesAnalyzersResources.ResourceManager, typeof(MicrosoftApiDesignGuidelinesAnalyzersResources)),
                                                                                      helpLinkUri: "http://msdn.microsoft.com/library/ms182242.aspx",
                                                                                      customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterCompilationAction(AnalyzeCompilation);
            analysisContext.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            IEnumerable<INamespaceSymbol> globalNamespaces = context.Compilation.GlobalNamespace.GetNamespaceMembers()
                .Where(item => item.ContainingAssembly == context.Compilation.Assembly);

            IEnumerable<INamedTypeSymbol> globalTypes = context.Compilation.GlobalNamespace.GetTypeMembers().Where(item =>
                    item.ContainingAssembly == context.Compilation.Assembly &&
                    IsExternallyVisible(item));

            CheckTypeNames(globalTypes, context.ReportDiagnostic);
            CheckNamespaceMembers(globalNamespaces, context.Compilation, context.ReportDiagnostic);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = context.Symbol as INamedTypeSymbol;
            // Do not descent into non-publicly visible types
            // Note: This is the behavior of FxCop, it might be more correct to descend into internal but not private
            // types because "InternalsVisibleTo" could be set. But it might be bad for users to start seeing warnings
            // where they previously did not from FxCop.
            if (namedTypeSymbol.GetResultantVisibility() != SymbolVisibility.Public)
            {
                return;
            }

            // Get externally visible members in the given type
            IEnumerable<ISymbol> members = namedTypeSymbol.GetMembers().Where(item => !item.IsAccessorMethod() && IsExternallyVisible(item));

            if (members.Any())
            {
                // Check parameters names of externally visible members with parameters
                CheckParameterMembers(members, context.ReportDiagnostic);

                // Check names of externally visible type members and their members
                CheckTypeMembers(members, context.ReportDiagnostic);
            }
        }

        private void CheckNamespaceMembers(IEnumerable<INamespaceSymbol> namespaces, Compilation compilation, Action<Diagnostic> addDiagnostic)
        {
            HashSet<INamespaceSymbol> excludedNamespaces = new HashSet<INamespaceSymbol>();
            foreach (INamespaceSymbol @namespace in namespaces)
            {
                // Get all the potentially externally visible types in the namespace
                IEnumerable<INamedTypeSymbol> typeMembers = @namespace.GetTypeMembers().Where(item =>
                    item.ContainingAssembly == compilation.Assembly &&
                    IsExternallyVisible(item));

                if (typeMembers.Any())
                {
                    CheckTypeNames(typeMembers, addDiagnostic);
                }
                else
                {
                    // If the namespace does not contain any externally visible types then exclude it from name check
                    excludedNamespaces.Add(@namespace);
                }

                IEnumerable<INamespaceSymbol> namespaceMembers = @namespace.GetNamespaceMembers();
                if (namespaceMembers.Any())
                {
                    CheckNamespaceMembers(namespaceMembers, compilation, addDiagnostic);

                    // If there is a child namespace that has externally visible types, then remove the parent namespace from exclusion list
                    if (namespaceMembers.Any(item => !excludedNamespaces.Contains(item)))
                    {
                        excludedNamespaces.Remove(@namespace);
                    }
                }
            }

            // Before name check, remove all namespaces that don't contain externally visible types in current scope
            namespaces = namespaces.Where(item => !excludedNamespaces.Contains(item));

            CheckNamespaceNames(namespaces, addDiagnostic);
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
                var typeSymbol = item as INamedTypeSymbol;
                if (typeSymbol != null &&
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

        private static void CheckParameterNames(IEnumerable<IParameterSymbol> parameters, Action<Diagnostic> addDiagnostic)
        {
            // If there is only one parameter, then return
            if (!parameters.Skip(1).Any())
            {
                return;
            }

            IEnumerable<IGrouping<string, IParameterSymbol>> parameterList = parameters.GroupBy((item) => item.Name, StringComparer.OrdinalIgnoreCase).Where((group) => group.Count() > 1);

            foreach (IGrouping<string, IParameterSymbol> group in parameterList)
            {
                ISymbol symbol = group.First().ContainingSymbol;
                addDiagnostic(symbol.CreateDiagnostic(Rule, Parameter, symbol.ToDisplayString()));
            }
        }

        private static bool HasViolatingParameters(ISymbol symbol)
        {
            ImmutableArray<IParameterSymbol> parameters = symbol.GetParameters();

            // If there is only one parameter, then return an empty collection
            if (!parameters.Skip(1).Any())
            {
                return false;
            }

            return parameters.GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase).Where((group) => group.Count() > 1).Any();
        }

        private static void CheckMemberNames(IEnumerable<ISymbol> members, Action<Diagnostic> addDiagnostic)
        {
            // If there is only one member, then return
            if (!members.Skip(1).Any())
            {
                return;
            }

            IEnumerable<ISymbol> overloadedMembers = members.Where((item) => !item.IsType()).GroupBy((item) => item.Name).Where((group) => group.Count() > 1).SelectMany((group) => group.Skip(1));
            IEnumerable<IGrouping<string, ISymbol>> memberList = members.Where((item) => !overloadedMembers.Contains(item)).GroupBy((item) => DiagnosticHelpers.GetMemberName(item), StringComparer.OrdinalIgnoreCase).Where((group) => group.Count() > 1);

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
                .Where((group) => group.Count() > 1);

            foreach (IGrouping<string, ITypeSymbol> group in typeList)
            {
                addDiagnostic(Diagnostic.Create(Rule, Location.None, Type, GetSymbolDisplayString(group)));
            }
        }

        private static void CheckNamespaceNames(IEnumerable<INamespaceSymbol> namespaces, Action<Diagnostic> addDiagnostic)
        {
            // If there is only one namespace, then return
            if (!namespaces.Skip(1).Any())
            {
                return;
            }

            IEnumerable<IGrouping<string, INamespaceSymbol>> namespaceList = namespaces.GroupBy((item) => item.ToDisplayString(), StringComparer.OrdinalIgnoreCase).Where((group) => group.Count() > 1);

            foreach (IGrouping<string, INamespaceSymbol> group in namespaceList)
            {
                addDiagnostic(Diagnostic.Create(Rule, Location.None, Namespace, GetSymbolDisplayString(group)));
            }
        }

        #endregion

        #region Helper Methods

        private static string GetSymbolDisplayString(IGrouping<string, ISymbol> group)
        {
            return string.Join(", ", group.Select(s => s.ToDisplayString()).OrderBy(k => k, StringComparer.Ordinal));
        }

        public static bool IsExternallyVisible(ISymbol symbol)
        {
            SymbolVisibility visibility = symbol.GetResultantVisibility();
            return visibility == SymbolVisibility.Public || visibility == SymbolVisibility.Internal;
        }

        #endregion
    }
}
