// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.Usage
{
    using static MicrosoftCodeQualityAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class DoNotCallOrderByMultipleTimes : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2259";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(DoNotCallOrderByMultipleTimesTitle)),
            CreateLocalizableResourceString(nameof(DoNotCallOrderByMultipleTimesDescription)),
            DiagnosticCategory.Usage,
            RuleLevel.BuildWarning,
            description: null,
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var compilation = context.Compilation;

            if (!compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqEnumerable, out var enumerableType))
                return;

            var analyzeOperation = new AnalyzeOperation(enumerableType);

            context.RegisterOperationAction(ctx => analyzeOperation.Analyze(ctx),
                                            OperationKind.Invocation);
        }

        private class AnalyzeOperation
        {
            public INamedTypeSymbol EnumerableType { get; }
            public IInvocationOperation? PreviousOrderByInvocation { get; private set; }

            public AnalyzeOperation(INamedTypeSymbol enumerableType)
            {
                EnumerableType = enumerableType;
            }

            public void Analyze(OperationAnalysisContext context)
            {
                if (context.Operation is not IInvocationOperation invocationOperation)
                    return;

                if (invocationOperation.TargetMethod.ReceiverType != EnumerableType)
                    return;

                if (invocationOperation.TargetMethod.Name is not "OrderBy" and not "OrderByDescending")
                    return;

                if (PreviousOrderByInvocation is null)
                {
                    PreviousOrderByInvocation = invocationOperation;
                }
                else if (PreviousOrderByInvocation != invocationOperation)
                {
                    context.ReportDiagnostic(PreviousOrderByInvocation.CreateDiagnostic(Rule));
                }
            }
        }
    }
}
