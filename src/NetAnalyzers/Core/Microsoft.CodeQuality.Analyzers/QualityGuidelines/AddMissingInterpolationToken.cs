// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    public abstract class AbstractAddMissingInterpolationTokenAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2251";

        internal static DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(RuleId,
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenTitle), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
#pragma warning disable RS1032 // Define diagnostic message correctly - the analyzer wants a period after the existing question mark.
            new LocalizableResourceString(nameof(MicrosoftCodeQualityAnalyzersResources.AddMissingInterpolationTokenMessage), MicrosoftCodeQualityAnalyzersResources.ResourceManager, typeof(MicrosoftCodeQualityAnalyzersResources)),
#pragma warning restore RS1032 // Define diagnostic message correctly
            DiagnosticCategory.Usage,
            RuleLevel.IdeSuggestion,
            description: null,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private protected abstract bool ShouldReport(ILiteralOperation operation);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterOperationAction(context =>
            {
                var literalOperation = (ILiteralOperation)context.Operation;
                if (!literalOperation.ConstantValue.HasValue ||
                    literalOperation.ConstantValue.Value is not string stringText)
                {
                    return;
                }

                if (ShouldReport(literalOperation))
                {
                    context.ReportDiagnostic(literalOperation.CreateDiagnostic(Rule));
                }
            }, OperationKind.Literal);
        }
    }
}
