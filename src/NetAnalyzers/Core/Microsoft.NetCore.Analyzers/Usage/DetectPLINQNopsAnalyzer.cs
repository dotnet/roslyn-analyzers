// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Usage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DetectPLINQNopsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2250";
        internal static readonly LocalizableString localizableTitle = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPLINQNopsTitle), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        private static readonly LocalizableString s_localizableMessageDefault = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPLINQNopsMessage), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));
        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(nameof(MicrosoftNetCoreAnalyzersResources.DetectPLINQNopsDescription), MicrosoftNetCoreAnalyzersResources.ResourceManager, typeof(MicrosoftNetCoreAnalyzersResources));

        internal static readonly DiagnosticDescriptor DefaultRule = DiagnosticDescriptorHelper.Create(RuleId,
                                                                             localizableTitle,
                                                                             s_localizableMessageDefault,
                                                                             DiagnosticCategory.Usage,
                                                                             RuleLevel.IdeSuggestion,
                                                                             description: s_localizableDescription,
                                                                             isPortedFxCopRule: false,
                                                                             isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DefaultRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(ctx =>
            {
                if (!ctx.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqParallelEnumerable, out var parallelEnumerable))
                {
                    return;
                }

                var asParallelSymbols = parallelEnumerable.GetMembers("AsParallel").ToImmutableHashSet();
                var toArraySymbols = parallelEnumerable.GetMembers("ToArray").ToImmutableHashSet();
                var toListSymbols = parallelEnumerable.GetMembers("ToList").ToImmutableHashSet();

                ctx.RegisterOperationAction(x => AnalyzeOperation(x, asParallelSymbols, toArraySymbols, toListSymbols), OperationKind.Invocation);
            });
        }

        public static bool ParentIsForEachStatement(IInvocationOperation operation) => operation.Parent is IForEachLoopOperation || operation.Parent?.Parent is IForEachLoopOperation;

        public static bool TryGetParentIsToArrayOrToList(IInvocationOperation operation, ImmutableHashSet<ISymbol> toArraySymbols, ImmutableHashSet<ISymbol> toListSymbols, out IInvocationOperation parentInvocation)
        {
            parentInvocation = null;
            if (operation.Parent?.Parent is not IInvocationOperation invocation)
            {
                return false;
            }
            if (toListSymbols.Contains(invocation.TargetMethod.OriginalDefinition) || toArraySymbols.Contains(invocation.TargetMethod.OriginalDefinition))
            {
                parentInvocation = invocation;
                return true;
            }
            return false;
        }

        private static void AnalyzeOperation(OperationAnalysisContext context, ImmutableHashSet<ISymbol> asParallelSymbols, ImmutableHashSet<ISymbol> toArraySymbols, ImmutableHashSet<ISymbol> toListSymbols)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var reducedMethod = invocation.TargetMethod.OriginalDefinition;
            if (reducedMethod is null)
            {
                return;
            }

            if (!asParallelSymbols.Contains(reducedMethod))
            {
                return;
            }

            IInvocationOperation? diagnosticInvocation = null;
            if (!ParentIsForEachStatement(invocation))
            {
                if (!TryGetParentIsToArrayOrToList(invocation, toArraySymbols, toListSymbols, out var parentInvocation) || !ParentIsForEachStatement(parentInvocation))
                {
                    return;
                }
                diagnosticInvocation = parentInvocation;
            }

            diagnosticInvocation ??= invocation;
            var diagnostic = diagnosticInvocation.CreateDiagnostic(DefaultRule, diagnosticInvocation.Syntax);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
