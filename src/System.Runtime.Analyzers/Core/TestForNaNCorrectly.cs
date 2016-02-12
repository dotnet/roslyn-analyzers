// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Semantics;

namespace System.Runtime.Analyzers
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
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true,
                                                                             description: s_localizableDescription,
                                                                             helpLinkUri: "https://msdn.microsoft.com/en-us/library/bb264491.aspx",
                                                                             customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private readonly BinaryOperationKind[] _comparisonOperators = new BinaryOperationKind[]
        {
            BinaryOperationKind.FloatingEquals,
            BinaryOperationKind.FloatingGreaterThan,
            BinaryOperationKind.FloatingGreaterThanOrEqual,
            BinaryOperationKind.FloatingLessThan,
            BinaryOperationKind.FloatingLessThanOrEqual,
            BinaryOperationKind.FloatingNotEquals
        };

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.RegisterOperationAction(
                operationAnalysisContext =>
                {
                    var binaryOperatorExpression = (IBinaryOperatorExpression)operationAnalysisContext.Operation;
                    if (!_comparisonOperators.Contains(binaryOperatorExpression.BinaryOperationKind))
                    {
                        return;
                    }

                    if (IsNan(binaryOperatorExpression.Left) || IsNan(binaryOperatorExpression.Right))
                    {
                        operationAnalysisContext.ReportDiagnostic(
                            binaryOperatorExpression.Syntax.CreateDiagnostic(Rule));
                    }
                },
                OperationKind.BinaryOperatorExpression);
        }

        private static bool IsNan(IExpression expr)
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