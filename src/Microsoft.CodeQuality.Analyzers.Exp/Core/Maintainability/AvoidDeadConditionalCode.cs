// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Operations.DataFlow;
using Microsoft.CodeAnalysis.Operations.DataFlow.CopyAnalysis;
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
                compilationContext.RegisterOperationBlockAction(operationBlockContext =>
                {
                    if (!(operationBlockContext.OwningSymbol is IMethodSymbol containingMethod))
                    {
                        return;
                    }

                    foreach (var operationRoot in operationBlockContext.OperationBlocks)
                    {
                        IBlockOperation topmostBlock = operationRoot.GetTopmostParentBlock();

                        bool ShouldAnalyze(IOperation op) =>
                                (op as IBinaryOperation)?.IsComparisonOperator() == true ||
                                (op as IInvocationOperation)?.TargetMethod.ReturnType.SpecialType == SpecialType.System_Boolean ||
                                op.Kind == OperationKind.Coalesce ||
                                op.Kind == OperationKind.ConditionalAccess ||
                                op.Kind == OperationKind.IsNull;

                        if (topmostBlock != null && topmostBlock.HasAnyOperationDescendant(ShouldAnalyze))
                        {
                            var cfg = SemanticModel.GetControlFlowGraph(topmostBlock);
                            var wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(operationBlockContext.Compilation);
                            var pointsToAnalysisResult = PointsToAnalysis.GetOrComputeResult(cfg, topmostBlock, containingMethod, wellKnownTypeProvider);
                            var copyAnalysisResult = CopyAnalysis.GetOrComputeResult(cfg, topmostBlock, containingMethod, wellKnownTypeProvider, pointsToAnalysisResultOpt: pointsToAnalysisResult);
                            // Do another analysis pass to improve the results from PointsTo and Copy analysis.
                            pointsToAnalysisResult = PointsToAnalysis.GetOrComputeResult(cfg, topmostBlock, containingMethod, wellKnownTypeProvider, copyAnalysisResult);
                            var stringContentAnalysisResult = StringContentAnalysis.GetOrComputeResult(cfg, topmostBlock, containingMethod, wellKnownTypeProvider, copyAnalysisResult, pointsToAnalysisResult);

                            foreach (var operation in cfg.DescendantOperations())
                            {
                                switch (operation.Kind)
                                {
                                    case OperationKind.BinaryOperator:
                                        var binaryOperation = (IBinaryOperation)operation;
                                        PredicateValueKind predicateKind = GetPredicateKind(binaryOperation);
                                        if (predicateKind != PredicateValueKind.Unknown &&
                                            (!(binaryOperation.LeftOperand is IBinaryOperation leftBinary) || GetPredicateKind(leftBinary) == PredicateValueKind.Unknown) &&
                                            (!(binaryOperation.RightOperand is IBinaryOperation rightBinary) || GetPredicateKind(rightBinary) == PredicateValueKind.Unknown))
                                        {
                                            ReportAlwaysTrueFalseOrNullDiagnostic(operation, predicateKind);
                                        }

                                        break;

                                    case OperationKind.Invocation:
                                        predicateKind = GetPredicateKind(operation);
                                        if (predicateKind != PredicateValueKind.Unknown)
                                        {
                                            ReportAlwaysTrueFalseOrNullDiagnostic(operation, predicateKind);
                                        }

                                        break;

                                    case OperationKind.IsNull:
                                        // '{0}' is always/never '{1}'. Remove or refactor the condition(s) to avoid dead code.
                                        predicateKind = GetPredicateKind(operation);
                                        DiagnosticDescriptor rule;
                                        switch (predicateKind)
                                        {
                                            case PredicateValueKind.AlwaysTrue:
                                                rule = AlwaysTrueFalseOrNullRule;
                                                break;

                                            case PredicateValueKind.AlwaysFalse:
                                                rule = NeverNullRule;
                                                break;

                                            default:
                                                continue;
                                        }

                                        var arg1 = operation.Syntax.ToString();
                                        var arg2 = operation.Language == LanguageNames.VisualBasic ? "Nothing" : "null";
                                        var diagnostic = operation.CreateDiagnostic(rule, arg1, arg2);
                                        operationBlockContext.ReportDiagnostic(diagnostic);
                                        break;
                                }
                            }

                            PredicateValueKind GetPredicateKind(IOperation operation)
                            {
                                Debug.Assert(operation.Kind == OperationKind.BinaryOperator || operation.Kind == OperationKind.Invocation || operation.Kind == OperationKind.IsNull);

                                if (operation is IBinaryOperation binaryOperation &&
                                    binaryOperation.IsComparisonOperator() ||
                                    operation.Type?.SpecialType == SpecialType.System_Boolean)
                                {
                                    PredicateValueKind predicateKind = pointsToAnalysisResult.GetPredicateKind(operation);
                                    if (predicateKind != PredicateValueKind.Unknown)
                                    {
                                        return predicateKind;
                                    }

                                    predicateKind = copyAnalysisResult.GetPredicateKind(operation);
                                    if (predicateKind != PredicateValueKind.Unknown)
                                    {
                                        return predicateKind;
                                    }

                                    predicateKind = stringContentAnalysisResult.GetPredicateKind(operation);
                                    if (predicateKind != PredicateValueKind.Unknown)
                                    {
                                        return predicateKind;
                                    };
                                }

                                return PredicateValueKind.Unknown;
                            }

                            void ReportAlwaysTrueFalseOrNullDiagnostic(IOperation operation, PredicateValueKind predicateKind)
                            {
                                Debug.Assert(predicateKind != PredicateValueKind.Unknown);

                                // '{0}' is always '{1}'. Remove or refactor the condition(s) to avoid dead code.
                                var arg1 = operation.Syntax.ToString();
                                var arg2 = predicateKind == PredicateValueKind.AlwaysTrue ?
                                    (operation.Language == LanguageNames.VisualBasic ? "True" : "true") :
                                    (operation.Language == LanguageNames.VisualBasic ? "False" : "false");
                                var diagnostic = operation.CreateDiagnostic(AlwaysTrueFalseOrNullRule, arg1, arg2);
                                operationBlockContext.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                });
            });
        }
    }
}
