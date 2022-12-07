// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
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
                if (stringType.GetMembers("StartsWith").FirstOrDefault() is not IMethodSymbol)
                {
                    return;
                }

                var indexOf = stringType.GetMembers("IndexOf").OfType<IMethodSymbol>();
                var indexOfMethodsBuilder = ImmutableArray.CreateBuilder<IMethodSymbol>();
                AddIfNotNull(indexOfMethodsBuilder, indexOf.SingleOrDefault(s => s.Parameters is [{ Type.SpecialType: SpecialType.System_String }]));
                AddIfNotNull(indexOfMethodsBuilder, indexOf.SingleOrDefault(s => s.Parameters is [{ Type.SpecialType: SpecialType.System_Char }]));
                AddIfNotNull(indexOfMethodsBuilder, indexOf.SingleOrDefault(s => s.Parameters is [{ Type.SpecialType: SpecialType.System_String }, { Name: "comparisonType" }]));
                if (indexOfMethodsBuilder.Count == 0)
                {
                    return;
                }

                var indexOfMethods = indexOfMethodsBuilder.ToImmutable();

                context.RegisterOperationAction(context =>
                {
                    var binaryOperation = (IBinaryOperation)context.Operation;
                    if (binaryOperation.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
                    {
                        return;
                    }

                    if (IsIndexOfComparedWithZero(binaryOperation.LeftOperand, binaryOperation.RightOperand, indexOfMethods, out var additionalLocations) ||
                        IsIndexOfComparedWithZero(binaryOperation.RightOperand, binaryOperation.LeftOperand, indexOfMethods, out additionalLocations))
                    {
                        var properties = ImmutableDictionary<string, string?>.Empty;
                        if (binaryOperation.OperatorKind == BinaryOperatorKind.NotEquals)
                        {
                            properties = properties.Add(ShouldNegateKey, "");
                        }

                        context.ReportDiagnostic(binaryOperation.CreateDiagnostic(Rule, additionalLocations: additionalLocations, properties: properties));
                    }

                }, OperationKind.Binary);

                static void AddIfNotNull(ImmutableArray<IMethodSymbol>.Builder builder, IMethodSymbol? symbol)
                {
                    if (symbol is not null)
                    {
                        builder.Add(symbol);
                    }
                }
            });
        }

        private static bool IsIndexOfComparedWithZero(IOperation left, IOperation right, ImmutableArray<IMethodSymbol> indexOfMethods, out ImmutableArray<Location> additionalLocations)
        {
            if (right.ConstantValue is { HasValue: true, Value: 0 } &&
                left is IInvocationOperation invocation)
            {
                foreach (var indexOfMethod in indexOfMethods)
                {
                    if (indexOfMethod.Parameters.Length == invocation.Arguments.Length && indexOfMethod.Equals(invocation.TargetMethod, SymbolEqualityComparer.Default))
                    {
                        var locationsBuilder = ImmutableArray.CreateBuilder<Location>();
                        locationsBuilder.Add(invocation.Instance.Syntax.GetLocation());
                        locationsBuilder.AddRange(invocation.Arguments.Select(arg => arg.Syntax.GetLocation()));
                        additionalLocations = locationsBuilder.ToImmutable();
                        return true;
                    }
                }
            }

            additionalLocations = ImmutableArray<Location>.Empty;
            return false;
        }
    }
}
