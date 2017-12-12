// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2242: Test for NaN correctly
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class TestForNaNCorrectlyAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2242";

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForNaNCorrectlyTitle), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForNaNCorrectlyMessage), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(SystemRuntimeAnalyzersResources.TestForNaNCorrectlyDescription), SystemRuntimeAnalyzersResources.ResourceManager, typeof(SystemRuntimeAnalyzersResources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(RuleId,
                                                                             s_localizableTitle,
                                                                             s_localizableMessage,
                                                                             DiagnosticCategory.Usage,
                                                                             DiagnosticHelpers.DefaultDiagnosticSeverity,
                                                                             isEnabledByDefault: DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb264491.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticHelpers.EnabledByDefaultIfNotBuildingVSIX ? ImmutableArray.Create(Rule) : ImmutableArray<DiagnosticDescriptor>.Empty;

        private readonly BinaryOperatorKind[] _comparisonOperators = new[]
        {
            BinaryOperatorKind.Equals,
            BinaryOperatorKind.GreaterThan,
            BinaryOperatorKind.GreaterThanOrEqual,
            BinaryOperatorKind.LessThan,
            BinaryOperatorKind.LessThanOrEqual,
            BinaryOperatorKind.NotEquals
        };

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterOperationAction(
                operationAnalysisContext =>
                {
                    var binaryOperatorExpression = (IBinaryOperation)operationAnalysisContext.Operation;
                    if (!_comparisonOperators.Contains(binaryOperatorExpression.OperatorKind))
                    {
                        return;
                    }

                    if (IsNan(binaryOperatorExpression.LeftOperand) || IsNan(binaryOperatorExpression.RightOperand))
                    {
                        operationAnalysisContext.ReportDiagnostic(
                            binaryOperatorExpression.Syntax.CreateDiagnostic(Rule));
                    }
                },
                OperationKind.BinaryOperator);
        }

        private static bool IsNan(IOperation expr)
        {
            if (expr == null ||
                !expr.ConstantValue.HasValue)
            {
                return false;
            }

            object value = expr.ConstantValue.Value;
            if (value is float)
            {
                return float.IsNaN((float)value);
            }

            if (value is double)
            {
                return double.IsNaN((double)value);
            }

            return false;
        }
    }
}