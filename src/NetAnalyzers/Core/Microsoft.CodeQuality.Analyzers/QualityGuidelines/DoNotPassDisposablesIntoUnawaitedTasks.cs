// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines
{
    using static MicrosoftCodeQualityAnalyzersResources;

    /// <summary>
    /// CA2025: <inheritdoc cref="DoNotPassDisposablesIntoUnawaitedTasksTitle"/>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotPassDisposablesIntoUnawaitedTasksAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2025";

        internal static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(DoNotPassDisposablesIntoUnawaitedTasksTitle)),
            CreateLocalizableResourceString(nameof(DoNotPassDisposablesIntoUnawaitedTasksMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            description: CreateLocalizableResourceString(nameof(DoNotPassDisposablesIntoUnawaitedTasksDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(context =>
            {
                var provider = WellKnownTypeProvider.GetOrCreate(context.Compilation);
                if (!provider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIDisposable, out var iDisposable))
                {
                    return;
                }

                var invocation = (IInvocationOperation)context.Operation;

                // Only care about tasks
                if (!invocation.IsTask())
                {
                    return;
                }

                // Ignore if awaited or run synchronously with task.Result or task.Wait()
                if (invocation.IsAwaited())
                {
                    return;
                }

                // Only care about invocations that receive IDisposable's as args
                if (!AnyWhere(arg => arg.Parameter?.Type?.AllInterfaces is { Length: > 0 } allInterfaces &&
                    allInterfaces.Any(i => i.ToString() == WellKnownTypeNames.SystemIDisposable), invocation.Arguments,
                    out var disposableArguments))
                {
                    return;
                }

                // Matching references to disposables
                var disposableReferences = disposableArguments.Select(matchingArg =>
                {
                    // Either a converted reference
                    if (matchingArg.Value is IConversionOperation conversion
                        && conversion.Operand is ILocalReferenceOperation convertedLocalReference)
                    {
                        return convertedLocalReference;
                    }

                    // Or an unconverted reference
                    return (matchingArg.Value as ILocalReferenceOperation)!;
                });

                // We use the inner method body only for checking disposable usage
                IOperation? containingBlock = invocation.GetAncestor<IMethodBodyOperation>(OperationKind.MethodBody);
                // In VB the real containing block is higher, especially if there are using blocks
                containingBlock ??= invocation.GetRoot();

                var descendants = containingBlock.Descendants();
                var localReferences = descendants.OfType<ILocalReferenceOperation>();

                List<ILocalReferenceOperation> referencedDisposableArgs = new();

                if (descendants.OfType<IUsingOperation>().ToList() is { Count: > 0 } usingBlocks)
                {
                    List<ILocalSymbol> usingLocals = new();
                    usingBlocks.ForEach(u => usingLocals.AddRange(u.Locals));

                    referencedDisposableArgs.AddRange(disposableReferences.Where(disposableRef =>
                    {
                        foreach (var usingLocal in usingLocals)
                        {
                            if (usingLocal.Equals(disposableRef))
                            {
                                return true;
                            }
                        }

                        return false;
                    }));
                }
                else if (!descendants.OfType<IUsingDeclarationOperation>().Any() && // check simple using statements
                    !localReferences.Any(r => r.Parent is IInvocationOperation { TargetMethod.Name: nameof(IDisposable.Dispose) }))
                {
                    // If we have no using statements and no Dispose calls, nothing to report
                    return;
                }

                referencedDisposableArgs.AddRange(localReferences.Intersect(disposableReferences));

                foreach (var referencedArg in referencedDisposableArgs)
                {
                    context.ReportDiagnostic(referencedArg.CreateDiagnostic(Rule));
                }
            }, OperationKind.Invocation);
        }

        private static bool AnyWhere<T>(Predicate<T> predicate, IEnumerable<T> collection, out IList<T> matches)
        {
            bool anyMatches = false;
            static IEnumerable<T> GetWhere(Predicate<T> predicate, IEnumerable<T> collection, Action onAny)
            {
                Action triggerOnce = () =>
                {
                    onAny();
                    triggerOnce = () => { };
                };

                foreach (var item in collection)
                {
                    if (predicate(item))
                    {
                        triggerOnce();
                        yield return item;
                    }
                }
            }

            // ToList actually evaluates enumerable so anyMatches will be accurate
            matches = [.. GetWhere(predicate, collection, () => anyMatches = true)];
            return anyMatches;
        }
    }

    internal static class TaskOperationExtensions
    {
        public static bool IsAwaited(this IOperation op)
        {
            return op.Parent is IAwaitOperation or IInvocationOperation { TargetMethod.Name: nameof(Task<>.Wait) }
                or IPropertyReferenceOperation { Property.Name: nameof(Task<>.Result) };
        }

        public static bool IsTask(this IOperation op)
        {
            return op.Type?.ToString() == WellKnownTypeNames.SystemThreadingTasksTask ||
                op.Type?.BaseType?.ToString() == WellKnownTypeNames.SystemThreadingTasksTask;
        }
    }
}
