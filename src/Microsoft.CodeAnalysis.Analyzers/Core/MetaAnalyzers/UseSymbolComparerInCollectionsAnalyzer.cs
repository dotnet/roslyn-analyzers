// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UseSymbolComparerInCollectionsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.UseComparerInSymbolCollectionsTitle), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.UseComparerInSymbolCollectionsMessage), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.UseComparerInSymbolCollectionsDescription), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));

        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticIds.UseComparerInSymbolCollectionsRuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.MicrosoftCodeAnalysisCorrectness,
            DiagnosticHelpers.DefaultDiagnosticSeverity,
            isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
            description: s_localizableDescription,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var compilation = context.Compilation;
                var symbolType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisISymbol);
                if (symbolType is null)
                {
                    return;
                }

                var genericComparerType = compilation.GetOrCreateTypeByMetadataName(typeof(IEqualityComparer<>).FullName);
                var comparerType = genericComparerType?.Construct(symbolType);
                if (comparerType == null)
                {
                    return;
                }

                context.RegisterOperationAction((context) =>
                {
                    switch (context.Operation)
                    {
                        case IInvocationOperation _: OnInvocationOperation(in context, symbolType, comparerType); break;
                        case IObjectCreationOperation _: OnObjectCreationOperation(in context, symbolType, comparerType); break;
                    }
                }, OperationKind.Invocation, OperationKind.ObjectCreation);
            });
        }

        private void OnInvocationOperation(in OperationAnalysisContext context, INamedTypeSymbol symbolType, INamedTypeSymbol comparerType)
        {
            var invocationOperation = (IInvocationOperation)context.Operation;
            var targetMethod = invocationOperation.TargetMethod;

            var compilation = context.Compilation;

            var immutableDictionarySymbol = compilation.GetOrCreateTypeByMetadataName("System.Collections.Immutable.ImmutableDictionary");
            if (targetMethod.ContainingSymbol.Equals(immutableDictionarySymbol))
            {
                switch (targetMethod.Name)
                {
                    case nameof(ImmutableDictionary.Create):
                    case nameof(ImmutableDictionary.CreateBuilder):
                    case nameof(ImmutableDictionary.ToImmutableDictionary):
                        {
                            // Create, CreateBuilder, and ToImmutableDictionary are static methods on ImmutableDictionary
                            // with the type argument on the method signature instead 
                            // of the containing type
                            if (FirstTypeArgumentIsSymbolType(targetMethod, symbolType))
                            {
                                RequireInvocationHasAnyComparerArgument(in context, invocationOperation, comparerType);
                            }
                        }
                        break;
                }
            }

            var enumerableSymbol = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqEnumerable);
            if (targetMethod.ContainingSymbol.Equals(enumerableSymbol))
            {
                switch (targetMethod.Name)
                {
                    case nameof(Enumerable.Contains):
                    case nameof(Enumerable.Distinct):
                    case nameof(Enumerable.Intersect):
                    case nameof(Enumerable.SequenceEqual):
                    case "ToHashSet":
                    case nameof(Enumerable.Union):
                        {
                            // All of these methods only have a single type argument and accept an equality comparer
                            if (FirstTypeArgumentIsSymbolType(targetMethod, symbolType))
                            {
                                RequireInvocationHasAnyComparerArgument(in context, invocationOperation, comparerType);
                            }
                        }
                        break;

                    case nameof(Enumerable.GroupBy):
                    case nameof(Enumerable.ToDictionary):
                    case nameof(Enumerable.ToLookup):
                        {
                            // GroupBy<TSource, TKey, TElement, TResult> 
                            // ToDictionary<TSource, TKey,  TElement>
                            // 
                            // Only need to use comparer if TKey is ISymbol
                            if (IndexedTypeArgumentIsSymbolType(1, targetMethod, symbolType))
                            {
                                RequireInvocationHasAnyComparerArgument(in context, invocationOperation, comparerType);
                            }
                        }
                        break;

                    case nameof(Enumerable.GroupJoin):
                    case nameof(Enumerable.Join):
                        {
                            // GroupJoin<TOuter, TInner, TKey, TResult>
                            // Join<TOuter, TInner, TKey, TResult>
                            //
                            // Only need to use comparer if TKey is ISymbol
                            if (IndexedTypeArgumentIsSymbolType(2, targetMethod, symbolType))
                            {
                                RequireInvocationHasAnyComparerArgument(in context, invocationOperation, comparerType);
                            }
                        }
                        break;
                }
            }
        }

        private static void OnObjectCreationOperation(in OperationAnalysisContext context, INamedTypeSymbol symbolType, INamedTypeSymbol comparerType)
        {
            var objectCreationOperation = (IObjectCreationOperation)context.Operation;
            var constructedType = objectCreationOperation.Constructor.ContainingType;

            switch (constructedType.Name)
            {
                case nameof(Dictionary<ISymbol, object>):
                case nameof(HashSet<ISymbol>):
                    {
                        if (FirstTypeArgumentIsSymbolType(constructedType, symbolType))
                        {
                            RequireInvocationHasAnyComparerArgument(in context, objectCreationOperation, comparerType);
                        }
                    }
                    break;
            }
        }

        private static void RequireInvocationHasAnyComparerArgument(in OperationAnalysisContext context, IInvocationOperation invocationOperation, INamedTypeSymbol comparerType)
        {
            if (invocationOperation.Arguments.Any(o => IsTypeSymbol(comparerType, o)))
            {
                return;
            }

            context.ReportDiagnostic(invocationOperation.Syntax.GetLocation().CreateDiagnostic(Rule));
        }

        private static void RequireInvocationHasAnyComparerArgument(in OperationAnalysisContext context, IObjectCreationOperation objectCreationOperation, INamedTypeSymbol comparerType)
        {
            if (objectCreationOperation.Arguments.Any(o => IsTypeSymbol(comparerType, o)))
            {
                return;
            }

            context.ReportDiagnostic(objectCreationOperation.Syntax.GetLocation().CreateDiagnostic(Rule));
        }

        private static bool FirstTypeArgumentIsSymbolType(IMethodSymbol methodToCheck, INamedTypeSymbol symbolType)
            => methodToCheck.TypeArguments.Any() &&
               IsTypeSymbol(symbolType, methodToCheck.TypeArguments.First());

        private static bool IndexedTypeArgumentIsSymbolType(int index, IMethodSymbol methodToCheck, INamedTypeSymbol symbolType)
            => methodToCheck.TypeArguments.Any() &&
               methodToCheck.TypeArguments.Count() > index &&
               IsTypeSymbol(symbolType, methodToCheck.TypeArguments.ElementAt(index));


        private static bool InvocationContainsEqualityComparerArgument(IInvocationOperation invocationOperation, INamedTypeSymbol comparerType)
            => invocationOperation.Arguments.Any(o => IsTypeSymbol(comparerType, o));

        private static bool FirstTypeArgumentIsSymbolType(INamedTypeSymbol typeToCheck, INamedTypeSymbol symbolType)
            => typeToCheck.TypeArguments.Any() &&
               IsTypeSymbol(symbolType, typeToCheck.TypeArguments.First());

        private static bool IsTypeSymbol(INamedTypeSymbol namedTypeSymbol, ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            if (typeSymbol.Equals(namedTypeSymbol))
            {
                return true;
            }

            if (typeSymbol.AllInterfaces.Contains(namedTypeSymbol))
            {
                return true;
            }

            return false;
        }

        private static bool IsTypeSymbol(INamedTypeSymbol namedTypeSymbol, IOperation operation)
        {
            if (operation.Type is object && IsTypeSymbol(namedTypeSymbol, operation.Type))
            {
                return true;
            }

            if (operation is IConversionOperation conversion)
            {
                return IsTypeSymbol(namedTypeSymbol, conversion.Operand);
            }

            if (operation is IArgumentOperation argumentOperation)
            {
                return IsTypeSymbol(namedTypeSymbol, argumentOperation.Value);
            }

            return false;
        }

    }
}
