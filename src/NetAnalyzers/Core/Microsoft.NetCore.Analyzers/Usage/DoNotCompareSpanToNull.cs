// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Usage
{
    using static MicrosoftNetCoreAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotCompareSpanToNullAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2262";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(DoNotCompareSpanToNullTitle)),
            CreateLocalizableResourceString(nameof(DoNotCompareSpanToNullMessage)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            description: CreateLocalizableResourceString(nameof(DoNotCompareSpanToNullDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(static context =>
            {
                var typeProvider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                if (!typeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemSpan1, out var spanType)
                    || !typeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemReadOnlySpan1, out var readOnlySpanType))
                {
                    return;
                }

                context.RegisterOperationAction(ctx => AnalyzeComparison(ctx, spanType, readOnlySpanType), OperationKind.Binary);
            });

        }

        private static void AnalyzeComparison(OperationAnalysisContext context, INamedTypeSymbol spanType, INamedTypeSymbol readOnlySpanType)
        {
            var binaryOperation = (IBinaryOperation)context.Operation;
            if (binaryOperation.RightOperand.WalkDownConversion().HasNullConstantValue() && binaryOperation.LeftOperand.Type is not null && IsSpan(binaryOperation.LeftOperand.Type)
                || binaryOperation.LeftOperand.WalkDownConversion().HasNullConstantValue() && binaryOperation.RightOperand.Type is not null && IsSpan(binaryOperation.RightOperand.Type))
            {
                context.ReportDiagnostic(binaryOperation.CreateDiagnostic(Rule));
            }

            bool IsSpan(ITypeSymbol typeSymbol)
            {
                var originalType = typeSymbol.OriginalDefinition;

                return originalType.Equals(spanType, SymbolEqualityComparer.Default)
                       || originalType.Equals(readOnlySpanType, SymbolEqualityComparer.Default);
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
    }
}