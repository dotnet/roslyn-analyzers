// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Performance
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class UseStartsWithInsteadOfIndexOfComparisonWithZero : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA1858";
        internal const string ShouldNegateKey = "ShouldNegate";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            id: "CA1858",
            title: CreateLocalizableResourceString(nameof(UseStartsWithInsteadOfIndexOfComparisonWithZeroTitle)),
            messageFormat: CreateLocalizableResourceString(nameof(UseStartsWithInsteadOfIndexOfComparisonWithZeroMessage)),
            category: DiagnosticCategory.Performance,
            ruleLevel: RuleLevel.IdeSuggestion,
            description: CreateLocalizableResourceString(nameof(UseStartsWithInsteadOfIndexOfComparisonWithZeroDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);
                if (stringType.GetMembers("StartsWith").FirstOrDefault() is not IMethodSymbol ||
                    stringType.GetMembers("IndexOf").FirstOrDefault() is not IMethodSymbol indexOfSymbol)
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    var binaryOperation = (IBinaryOperation)context.Operation;
                    if (binaryOperation.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
                    {
                        return;
                    }

                    if (IsIndexOfComparedWithZero(binaryOperation.LeftOperand, binaryOperation.RightOperand, indexOfSymbol, out var instanceLocation, out var argumentLocation) ||
                        IsIndexOfComparedWithZero(binaryOperation.RightOperand, binaryOperation.LeftOperand, indexOfSymbol, out instanceLocation, out argumentLocation))
                    {
                        var properties = ImmutableDictionary<string, string?>.Empty;
                        if (binaryOperation.OperatorKind == BinaryOperatorKind.NotEquals)
                        {
                            properties = properties.Add(ShouldNegateKey, "");
                        }

                        context.ReportDiagnostic(binaryOperation.CreateDiagnostic(Rule, additionalLocations: ImmutableArray.Create(instanceLocation, argumentLocation), properties: properties));
                    }

                }, OperationKind.Binary);
            });
        }

        private static bool IsIndexOfComparedWithZero(IOperation left, IOperation right, IMethodSymbol indexOfSymbol, [NotNullWhen(true)] out Location? instanceLocation, [NotNullWhen(true)] out Location? argumentLocation)
        {
            if (right.ConstantValue is not { HasValue: true, Value: 0 } ||
                left is not IInvocationOperation invocation ||
                invocation.Arguments.Length != 1 ||
                SymbolEqualityComparer.Default.Equals(invocation.TargetMethod, indexOfSymbol))
            {
                instanceLocation = null;
                argumentLocation = null;
                return false;
            }


            instanceLocation = invocation.Instance.Syntax.GetLocation();
            argumentLocation = invocation.Arguments[0].Syntax.GetLocation();
            return true;
        }
    }
}
