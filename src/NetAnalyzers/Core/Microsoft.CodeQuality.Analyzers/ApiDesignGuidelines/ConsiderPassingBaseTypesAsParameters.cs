// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    using static MicrosoftCodeQualityAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ConsiderPassingBaseTypesAsParameters : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1011";

        internal static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorHelper.Create(
                RuleId,
                CreateLocalizableResourceString(nameof(ConsiderPassingBaseTypesAsParametersTitle)),
                CreateLocalizableResourceString(nameof(ConsiderPassingBaseTypesAsParametersMessage)),
                DiagnosticCategory.Design,
                RuleLevel.IdeSuggestion,
                description: CreateLocalizableResourceString(nameof(ConsiderPassingBaseTypesAsParametersDescription)),
                isPortedFxCopRule: true,
                isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var eventArgsTypeSymbol = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemEventArgs);

                context.RegisterOperationBlockStartAction(context => AnalyzeOperationBlockStart(context, eventArgsTypeSymbol));
            });
        }

        private static void AnalyzeOperationBlockStart(OperationBlockStartAnalysisContext context, INamedTypeSymbol? eventArgsTypeSymbol)
        {
            if (context.OwningSymbol is not IMethodSymbol methodSymbol ||
                methodSymbol.IsOverride ||
                methodSymbol.Parameters.Length == 0 ||
                methodSymbol.IsImplementationOfAnyInterfaceMember() ||
                methodSymbol.HasEventHandlerSignature(eventArgsTypeSymbol) ||
                !context.Options.MatchesConfiguredVisibility(Rule, methodSymbol, context.Compilation,
                    defaultRequiredVisibility: SymbolVisibilityGroup.All) ||
                context.Options.IsConfiguredToSkipAnalysis(Rule, methodSymbol,
                    context.Compilation))
            {
                return;
            }

            var typesUsedPerParameter = PooledConcurrentDictionary<IParameterSymbol, PooledConcurrentSet<ITypeSymbol>>.GetInstance(SymbolEqualityComparer.Default);
            foreach (var param in methodSymbol.Parameters)
            {
                typesUsedPerParameter.AddOrUpdate(param, PooledConcurrentSet<ITypeSymbol>.GetInstance(SymbolEqualityComparer.Default), (param, value) => value);
            }

            context.RegisterOperationAction(context =>
            {
                var paramReference = (IParameterReferenceOperation)context.Operation;

                switch (paramReference.Parent.Kind)
                {
                    case OperationKind.Invocation:
                        AddDeclaringTypeSymbol(((IInvocationOperation)paramReference.Parent).TargetMethod);
                        break;

                    case OperationKind.PropertyReference:
                    case OperationKind.EventReference:
                    case OperationKind.FieldReference:
                    case OperationKind.MethodReference:
                        AddDeclaringTypeSymbol(((IMemberReferenceOperation)paramReference.Parent).Member);
                        break;

                    case OperationKind.Argument:
                        AddType(paramReference.Type);
                        break;

                    case OperationKind.Conversion:
                        AddType(paramReference.Parent.Type);
                        break;

                    default:
                        break; // Do nothing
                }

                // Local functions
                void AddType(ITypeSymbol typeSymbol)
                {
                    if (typesUsedPerParameter.TryGetValue(paramReference.Parameter, out var typePool))
                    {
                        typePool.Add(typeSymbol);
                    }
                }

                void AddDeclaringTypeSymbol(ISymbol symbol)
                    => AddType(FindDeclaringType(symbol));
            }, OperationKind.ParameterReference);

            context.RegisterOperationBlockEndAction(context =>
            {
                try
                {
                    var overloads = methodSymbol.GetOverloads();

                    foreach (var (parameter, usedTypesPool) in typesUsedPerParameter)
                    {
                        var usedTypes = usedTypesPool.ToImmutableArray();
                        // If type is unused, we ignore it. There is another rule to detect
                        // unused parameters.

                        if (usedTypes.Length == 1)
                        {
                            ReportIfTypesAreDifferent(parameter, usedTypes[0], overloads);
                        }
                        else if (usedTypes.Length > 1)
                        {
                            ReportIfTypesAreDifferent(parameter, FindMostGenericType(parameter.Type, usedTypes), overloads);
                        }
                    }
                }
                finally
                {
                    typesUsedPerParameter.Free(context.CancellationToken);
                }

                // Local functions
                void ReportIfTypesAreDifferent(IParameterSymbol parameter, ITypeSymbol suggestedBaseTypeSymbol, IEnumerable<IMethodSymbol> overloads)
                {
                    if (SymbolEqualityComparer.Default.Equals(suggestedBaseTypeSymbol, parameter.Type))
                    {
                        return;
                    }

                    if (suggestedBaseTypeSymbol is INamedTypeSymbol suggestedNamedType)
                    {
                        var parameterIndex = methodSymbol.GetParameterIndex(parameter);
                        var matchingOverload = methodSymbol.GetMatchingOverload(overloads, parameterIndex, suggestedNamedType, context.CancellationToken);

                        if (matchingOverload != null)
                        {
                            return;
                        }
                    }

                    // All calls on this parameter are using the same base type which is different
                    // from the one declared, so we can simply report.
                    context.ReportDiagnostic(parameter.CreateDiagnostic(Rule, parameter.Name,
                        parameter.Type.Name, suggestedBaseTypeSymbol.Name));
                }
            });
        }

        private static ITypeSymbol FindDeclaringType(ISymbol symbol)
        {
            if (symbol.IsOverride)
            {
                return FindDeclaringType(symbol.GetOverriddenMember());
            }

            var interfaceMembers = ExplicitOrImplicitInterfaceImplementations(symbol);
            if (interfaceMembers.Length == 1)
            {
                return interfaceMembers[0].ContainingType;
            }
            else
            {
                // When reaching this point, either we found no interface member, which means the
                // member is declared directly on the type OR more than one interface was found
                // as declaring this type so let's assume it is defined on the containing type.
                return symbol.ContainingType;
            }
        }

        // Borrowed from Microsoft.CodeAnalysis.Workspaces/ISymbolExtensions.cs with a small tweak
        // which consist in passing the member name.
        private static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(ISymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Method &&
                symbol.Kind != SymbolKind.Property &&
                symbol.Kind != SymbolKind.Event)
            {
                return ImmutableArray<ISymbol>.Empty;
            }

            var containingType = symbol.ContainingType;
            var query = from iface in containingType.AllInterfaces
                        from interfaceMember in iface.GetMembers(symbol.Name)
                        let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
                        where symbol.Equals(impl)
                        select interfaceMember;
            return query.ToImmutableArray();
        }

        private static ITypeSymbol FindMostGenericType(ITypeSymbol originalType, ImmutableArray<ITypeSymbol> constraints)
        {
            Debug.Assert(constraints.Length >= 2);

            var mostGenericInterface =
                originalType.AllInterfaces.FirstOrDefault(type => constraints.All(x => type.Inherits(x)));
            if (mostGenericInterface != null)
            {
                return mostGenericInterface;
            }

            var mostGenericBaseType = originalType.GetBaseTypesAndThis()
                .Reverse()
                .FirstOrDefault(type => constraints.All(x => type.Inherits(x)));

            return mostGenericBaseType ?? originalType;
        }
    }
}
