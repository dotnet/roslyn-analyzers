// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UseSymbolComparerInCollectionsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.CompareSymbolsCorrectlyTitle), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.CompareSymbolsCorrectlyMessage), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(CodeAnalysisDiagnosticsResources.CompareSymbolsCorrectlyDescription), CodeAnalysisDiagnosticsResources.ResourceManager, typeof(CodeAnalysisDiagnosticsResources));

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
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(context =>
            {
                var compilation = context.Compilation;
                var symbolType = compilation.GetTypeByMetadataName(WellKnownTypeNames.MicrosoftCodeAnalysisISymbol);
                if (symbolType is null)
                {
                    return;
                }

                var genericComparerType = compilation.GetTypeByMetadataName(typeof(IEqualityComparer<>).FullName);
                var comparerType = genericComparerType?.Construct(symbolType);
                if (comparerType == null)
                {
                    return;
                }

                context.RegisterOperationAction((context) =>
                {
                    switch (context.Operation)
                    {
                        case IObjectCreationOperation _: OnObjectCreationOperation(context, symbolType, comparerType); break;
                        case IInvocationOperation _: OnInvocationOperation(context, symbolType, comparerType); break;
                    }
                }, OperationKind.ObjectCreation, OperationKind.Invocation, OperationKind.ObjectCreation);
            });
        }

        private void OnInvocationOperation(in OperationAnalysisContext context, INamedTypeSymbol symbolType, INamedTypeSymbol comparerType)
        {
            var invocationOperation = (IInvocationOperation)context.Operation;
            var targetMethod = invocationOperation.TargetMethod;

            switch (targetMethod.ContainingSymbol.Name)
            {
                case nameof(ImmutableArray):
                    {
                        if (targetMethod.Name == nameof(ImmutableArray.BinarySearch))
                        {
                            var thisParameterForExtension = targetMethod.Parameters.First();
                            var typeForThisParam = thisParameterForExtension.Type;
                            if (typeForThisParam.Name == nameof(ImmutableArray) &&
                                FirstTypeArgumentIsSymbolType(typeForThisParam, symbolType))
                            {
                                RequireInvocationHasAnyComparerArgument(context, invocationOperation, comparerType);
                            }
                        }
                    }
                    break;

                case nameof(ImmutableDictionary):
                    {
                        switch (targetMethod.Name)
                        {
                            case nameof(ImmutableDictionary.Create):
                            case nameof(ImmutableDictionary.CreateBuilder):
                                {
                                    // Create and CreateBuilder are static methods on ImmutableDictionary
                                    // with the type argument on the method signature instead 
                                    // of the containing type
                                    if (FirstTypeArgumentIsSymbolType(targetMethod, symbolType))
                                    {
                                        RequireInvocationHasAnyComparerArgument(context, invocationOperation, comparerType);
                                    }
                                }
                                break;
                            case nameof(ImmutableDictionary.ToImmutableDictionary):
                                // ToImmutableDictionary is an extension method, so the first parameter has
                                // the type arguments we need to check
                                if (FirstTypeArgumentIsSymbolType(targetMethod.Parameters.First().ContainingType, symbolType))
                                {
                                    RequireInvocationHasAnyComparerArgument(context, invocationOperation, comparerType);
                                }
                                break;
                        }
                        break;
                    }
            }
        }

        private static void RequireInvocationHasAnyComparerArgument(in OperationAnalysisContext context, IInvocationOperation invocationOperation, INamedTypeSymbol comparerType)
        {
            if (invocationOperation.Arguments.Any(comparerType.IsTypeSymbol))
            {
                return;
            }

            context.ReportDiagnostic(invocationOperation.Syntax.GetLocation().CreateDiagnostic(Rule));
        }

        private static bool FirstTypeArgumentIsSymbolType(ITypeSymbol typeToCheck, INamedTypeSymbol symbolType)
            => typeToCheck is INamedTypeSymbol namedTypeSymbol &&
               namedTypeSymbol.TypeArguments.Any() &&
               symbolType.IsTypeSymbol(namedTypeSymbol.TypeArguments.First());

        private static bool FirstTypeArgumentIsSymbolType(IMethodSymbol methodToCheck, INamedTypeSymbol symbolType)
            => methodToCheck.TypeArguments.Any() &&
               symbolType.IsTypeSymbol(methodToCheck.TypeArguments.First());


        private static bool InvocationContainsEqualityComparerArgument(IInvocationOperation invocationOperation, INamedTypeSymbol comparerType)
            => invocationOperation.Arguments.Any(comparerType.IsTypeSymbol);

        private void OnObjectCreationOperation(in OperationAnalysisContext context, INamedTypeSymbol symbolType, INamedTypeSymbol comparerType)
        {
           
        }
    }
}
