// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.ControlFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.NullAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations.DataFlow.StringContentAnalysis;

namespace Microsoft.CodeQuality.Analyzers.Exp.Maintainability
{
    /// <summary>
    /// CA1508: Flags conditional expressions which are always true/false and null checks for operations that are always null/non-null based on predicate analysis.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class AvoidDeadConditionalCode : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1508";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidDeadConditionalCodeTitle), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableAlwaysTrueFalseOrNullMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidDeadConditionalCodeAlwaysTruFalseOrNullMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));
        private static readonly LocalizableString s_localizableNeverNullMessage = new LocalizableResourceString(nameof(MicrosoftMaintainabilityAnalyzersResources.AvoidDeadConditionalCodeNeverNullMessage), MicrosoftMaintainabilityAnalyzersResources.ResourceManager, typeof(MicrosoftMaintainabilityAnalyzersResources));

        internal static DiagnosticDescriptor AlwaysTrueFalseOrNullRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableAlwaysTrueFalseOrNullMessage,
                                                                             DiagnosticCategory.Maintainability,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             helpLinkUri: null, // TODO: Add helplink
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        internal static DiagnosticDescriptor NeverNullRule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableNeverNullMessage,
                                                                             DiagnosticCategory.Maintainability,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             helpLinkUri: null, // TODO: Add helplink
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AlwaysTrueFalseOrNullRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationContext =>
            {
                compilationContext.RegisterOperationBlockStartAction(operationBlockStartContext =>
                {
                    if (!(operationBlockStartContext.OwningSymbol is IMethodSymbol containingMethod))
                    {
                        return;
                    }

                    foreach (var operationRoot in operationBlockStartContext.OperationBlocks)
                    {
                        IBlockOperation topmostBlock = operationRoot.GetTopmostParentBlock();
                        if (topmostBlock != null && topmostBlock.HasAnyOperationDescendant(op => (op as IBinaryOperation)?.IsComparisonOperator() == true || op.Kind == OperationKind.Coalesce || op.Kind == OperationKind.ConditionalAccess))
                        {
                            var cfg = ControlFlowGraph.Create(topmostBlock);
                            var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(operationBlockStartContext.Compilation);
                            var nullAnalysisResult = NullAnalysis.GetOrComputeResult(cfg, containingMethod, wellKnownTypeProvider);
                            var pointsToAnalysisResult = PointsToAnalysis.GetOrComputeResult(cfg, containingMethod, wellKnownTypeProvider, nullAnalysisResult);
                            var copyAnalysisResult = CopyAnalysis.GetOrComputeResult(cfg, containingMethod, wellKnownTypeProvider, nullAnalysisResultOpt: nullAnalysisResult, pointsToAnalysisResultOpt: pointsToAnalysisResult);
                            // Do another null analysis pass to improve the results from PointsTo and Copy analysis.
                            nullAnalysisResult = NullAnalysis.GetOrComputeResult(cfg, containingMethod, wellKnownTypeProvider, copyAnalysisResult, pointsToAnalysisResultOpt: pointsToAnalysisResult);
                            var stringContentAnalysisResult = StringContentAnalysis.GetOrComputeResult(cfg, containingMethod, wellKnownTypeProvider, copyAnalysisResult, nullAnalysisResult, pointsToAnalysisResult);

                            operationBlockStartContext.RegisterOperationAction(operationContext =>
                            {
                                PredicateValueKind GetPredicateKind(IBinaryOperation operation)
                                {
                                    if (operation.IsComparisonOperator())
                                    {
                                        PredicateValueKind binaryPredicateKind = nullAnalysisResult.GetPredicateKind(operation);
                                        if (binaryPredicateKind != PredicateValueKind.Unknown)
                                        {
                                            return binaryPredicateKind;
                                        }

                                        binaryPredicateKind = copyAnalysisResult.GetPredicateKind(operation);
                                        if (binaryPredicateKind != PredicateValueKind.Unknown)
                                        {
                                            return binaryPredicateKind;
                                        }

                                        binaryPredicateKind = stringContentAnalysisResult.GetPredicateKind(operation);
                                        if (binaryPredicateKind != PredicateValueKind.Unknown)
                                        {
                                            return binaryPredicateKind;
                                        };
                                    }

                                    return PredicateValueKind.Unknown;
                                }

                                var binaryOperation = (IBinaryOperation)operationContext.Operation;
                                PredicateValueKind predicateKind = GetPredicateKind(binaryOperation);
                                if (predicateKind != PredicateValueKind.Unknown &&
                                    (!(binaryOperation.LeftOperand is IBinaryOperation leftBinary) || GetPredicateKind(leftBinary) == PredicateValueKind.Unknown) &&
                                    (!(binaryOperation.RightOperand is IBinaryOperation rightBinary) || GetPredicateKind(rightBinary) == PredicateValueKind.Unknown))
                                {
                                    // '{0}' is always '{1}'. Remove or refactor the condition(s) to avoid dead code.
                                    var arg1 = binaryOperation.Syntax.ToString();
                                    var arg2 = predicateKind == PredicateValueKind.AlwaysTrue ? 
                                        (binaryOperation.Language == LanguageNames.VisualBasic ? "True" : "true") :
                                        (binaryOperation.Language == LanguageNames.VisualBasic ? "False" : "false");
                                    var diagnostic = binaryOperation.CreateDiagnostic(AlwaysTrueFalseOrNullRule, arg1, arg2);
                                    operationContext.ReportDiagnostic(diagnostic);
                                }
                            }, OperationKind.BinaryOperator);

                            operationBlockStartContext.RegisterOperationAction(operationContext =>
                            {
                                IOperation nullCheckedOperation = operationContext.Operation.Kind == OperationKind.Coalesce ?
                                    ((ICoalesceOperation)operationContext.Operation).Value :
                                    ((IConditionalAccessOperation)operationContext.Operation).Operation;

                                // '{0}' is always/never '{1}'. Remove or refactor the condition(s) to avoid dead code.
                                DiagnosticDescriptor rule;
                                switch (nullAnalysisResult[nullCheckedOperation])
                                {
                                    case NullAbstractValue.Null:
                                        rule = AlwaysTrueFalseOrNullRule;
                                        break;

                                    case NullAbstractValue.NotNull:
                                        rule = NeverNullRule;
                                        break;

                                    default:
                                        return;
                                }

                                var arg1 = nullCheckedOperation.Syntax.ToString();
                                var arg2 = nullCheckedOperation.Language == LanguageNames.VisualBasic ? "Nothing" : "null";
                                var diagnostic = nullCheckedOperation.CreateDiagnostic(rule, arg1, arg2);
                                operationContext.ReportDiagnostic(diagnostic);
                            }, OperationKind.Coalesce, OperationKind.ConditionalAccess);
                        }
                    }
                });
            });
        }
    }
}
