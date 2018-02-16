// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.DisposeAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Exp.Reliability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DisposeObjectsBeforeLosingScope : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2000";

        private static readonly string[] s_disposeOwnershipTransferLikelyTypes = new string[]
            {
                "System.IO.Stream",
                "System.IO.TextReader",
                "System.IO.TextWriter",
                "System.Resources.IResourceReader",
            };

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftReliabilityAnalyzersResources.DisposeObjectsBeforeLosingScopeTitle), MicrosoftReliabilityAnalyzersResources.ResourceManager, typeof(MicrosoftReliabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(MicrosoftReliabilityAnalyzersResources.DisposeObjectsBeforeLosingScopeMessage), MicrosoftReliabilityAnalyzersResources.ResourceManager, typeof(MicrosoftReliabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftReliabilityAnalyzersResources.DisposeObjectsBeforeLosingScopeDescription), MicrosoftReliabilityAnalyzersResources.ResourceManager, typeof(MicrosoftReliabilityAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Reliability,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2000-dispose-objects-before-losing-scope",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var iDisposable = WellKnownTypes.IDisposable(compilationContext.Compilation);
                if (iDisposable == null)
                {
                    return;
                }

                var iCollection = WellKnownTypes.ICollection(compilationContext.Compilation);
                var genericICollection = WellKnownTypes.GenericICollection(compilationContext.Compilation);
                var disposeOwnershipTransferLikelyTypes = GetDisposeOwnershipTransferLikelyTypes(compilationContext.Compilation);
                compilationContext.RegisterOperationBlockStartAction(operationBlockStartContext =>
                {
                    bool hasDisposableCreation = false;
                    operationBlockStartContext.RegisterOperationAction(operationContext =>
                    {
                        if (!hasDisposableCreation &&
                            operationContext.Operation.Type.IsDisposable(iDisposable))
                        {
                            hasDisposableCreation = true;
                        }
                    },
                    OperationKind.ObjectCreation,
                    OperationKind.TypeParameterObjectCreation,
                    OperationKind.DynamicObjectCreation,
                    OperationKind.Invocation);

                    operationBlockStartContext.RegisterOperationBlockEndAction(operationBlockEndContext =>
                    {
                        if (!hasDisposableCreation ||
                            !(operationBlockEndContext.OwningSymbol is IMethodSymbol containingMethod))
                        {
                            return;
                        }

                        foreach (var operationRoot in operationBlockEndContext.OperationBlocks)
                        {
                            IBlockOperation topmostBlock = operationRoot.GetTopmostParentBlock();
                            if (topmostBlock != null)
                            {
                                var cfg = ControlFlowGraph.Create(topmostBlock);
                                var nullAnalysisResult = NullAnalysis.GetOrComputeResult(cfg, containingMethod.ContainingType);
                                var pointsToAnalysisResult = PointsToAnalysis.GetOrComputeResult(cfg, containingMethod.ContainingType, nullAnalysisResult);
                                var disposeAnalysisResult = DisposeAnalysis.GetOrComputeResult(cfg, iDisposable, iCollection,
                                    genericICollection, disposeOwnershipTransferLikelyTypes, containingMethod.ContainingType, pointsToAnalysisResult, nullAnalysisResult);
                                ImmutableDictionary<AbstractLocation, DisposeAbstractValue> disposeDataAtExit = disposeAnalysisResult[cfg.Exit].InputData;
                                foreach (var kvp in disposeDataAtExit)
                                {
                                    AbstractLocation location = kvp.Key;
                                    DisposeAbstractValue disposeValue = kvp.Value;
                                    if (disposeValue.Kind == DisposeAbstractValueKind.NotDisposed ||
                                        ((disposeValue.Kind == DisposeAbstractValueKind.Disposed ||
                                          disposeValue.Kind == DisposeAbstractValueKind.MaybeDisposed) &&
                                         disposeValue.DisposingOperations.Count > 0 &&
                                         disposeValue.DisposingOperations.All(d => d.IsInsideCatchClause())))
                                    {
                                        Debug.Assert(location.CreationOpt != null);

                                        // CA2000: In method '{0}', call System.IDisposable.Dispose on object created by '{1}' before all references to it are out of scope.
                                        var arg1 = containingMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                                        var arg2 = location.CreationOpt.Syntax.ToString();
                                        var diagnostic = location.CreationOpt.Syntax.CreateDiagnostic(Rule, arg1, arg2);
                                        operationBlockEndContext.ReportDiagnostic(diagnostic);
                                    }
                                }

                                break;
                            }
                        }
                    });
                });
            });
        }

        private static ImmutableHashSet<INamedTypeSymbol> GetDisposeOwnershipTransferLikelyTypes(Compilation compilation)
        {
            var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>();
            foreach (var typeName in s_disposeOwnershipTransferLikelyTypes)
            {
                INamedTypeSymbol typeSymbol = compilation.GetTypeByMetadataName(typeName);
                if (typeSymbol != null)
                {
                    builder.Add(typeSymbol);
                }
            }

            return builder.ToImmutable();
        }
    }
}
