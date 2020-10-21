// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                if (!ctx.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqParallelEnumerable, out var parallelEnumerable) || !ctx.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemLinqEnumerable, out var linqEnumerable))
                {
                    return;
                }

                var asParallelSymbols = parallelEnumerable.GetMembers("AsParallel").ToImmutableHashSet();
                var collectionSymbols = parallelEnumerable.GetMembers("ToArray")
                    .Concat(parallelEnumerable.GetMembers("ToList"))
                    .Concat(parallelEnumerable.GetMembers("ToDictionary"))
                    .Concat(linqEnumerable.GetMembers("ToHashSet"))
                    .ToImmutableHashSet();

                ctx.RegisterOperationAction(x => AnalyzeOperation(x, asParallelSymbols, collectionSymbols), OperationKind.Invocation);
            });
        }

        public static bool ParentIsForEachStatement(IInvocationOperation operation) => operation.Parent is IForEachLoopOperation || operation.Parent?.Parent is IForEachLoopOperation;

        private static bool FindFirstInvocationParent(IOperation operation, [NotNullWhen(true)] out IInvocationOperation? invocationOperation)
        {
            invocationOperation = null;
            do
            {
                operation = operation.Parent;
                if (operation is IInvocationOperation invocation)
                {
                    invocationOperation = invocation;
                    return true;
                }

                if (operation is ILocalFunctionOperation or IAnonymousFunctionOperation)
                    return false;
            } while (operation != null);

            return false;
        }

        public static bool TryGetParentIsToCollection(IInvocationOperation operation, ImmutableHashSet<ISymbol> collectionSymbols, out IInvocationOperation parentInvocation)
        {
            parentInvocation = operation;
            var hasParentInvocation = FindFirstInvocationParent(operation, out var invocationParent);
            if (!hasParentInvocation)
            {
                return false;
            }

            var targetMethod = (invocationParent!.TargetMethod.ReducedFrom ?? invocationParent.TargetMethod).OriginalDefinition;
            if (collectionSymbols.Contains(targetMethod))
            {
                parentInvocation = invocationParent;
                return true;
            }

            return false;
        }

        private static void AnalyzeOperation(OperationAnalysisContext context, ImmutableHashSet<ISymbol> asParallelSymbols, ImmutableHashSet<ISymbol> collectionSymbols)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var reducedMethod = invocation.TargetMethod.OriginalDefinition;
            if (reducedMethod is null)
            {
                return;
            }

            if (!(asParallelSymbols.Contains(reducedMethod) || asParallelSymbols.Contains(reducedMethod.ReducedFrom)))
            {
                return;
            }

            IInvocationOperation? diagnosticInvocation = null;
            if (!ParentIsForEachStatement(invocation))
            {
                if (!TryGetParentIsToCollection(invocation, collectionSymbols, out var parentInvocation) || !ParentIsForEachStatement(parentInvocation))
                {
                    return;
                }

                diagnosticInvocation = parentInvocation;
            }

            diagnosticInvocation ??= invocation;
            var diagnostic = diagnosticInvocation.CreateDiagnostic(DefaultRule);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
