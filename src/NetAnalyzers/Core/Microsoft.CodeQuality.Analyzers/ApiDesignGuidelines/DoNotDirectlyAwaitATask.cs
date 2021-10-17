// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.Lightup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines
{
    using static MicrosoftCodeQualityAnalyzersResources;

    /// <summary>
    /// CA2007: Do not directly await a Task in libraries. Append ConfigureAwait(false) to the task.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class DoNotDirectlyAwaitATaskAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2007";
        private const string ConfigureAwait = "ConfigureAwait";
        public static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
            RuleId,
            CreateLocalizableResourceString(nameof(DoNotDirectlyAwaitATaskTitle)),
            CreateLocalizableResourceString(nameof(DoNotDirectlyAwaitATaskMessage)),
            DiagnosticCategory.Reliability,
            RuleLevel.Disabled,
            description: CreateLocalizableResourceString(nameof(DoNotDirectlyAwaitATaskDescription)),
            isPortedFxCopRule: false,
            isDataflowRule: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(context =>
            {
                if (context.Compilation.SyntaxTrees.FirstOrDefault() is not SyntaxTree tree ||
                    !context.Options.GetOutputKindsOption(Rule, tree, context.Compilation).Contains(context.Compilation.Options.OutputKind))
                {
                    // Configured to skip analysis for the compilation's output kind
                    return;
                }

                if (!TryGetTaskTypes(context.Compilation, out ImmutableArray<INamedTypeSymbol> taskTypes))
                {
                    return;
                }

                var configuredAsyncDisposable = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesConfiguredAsyncDisposable);
                var configuredCancelableAsyncEnumerable = context.Compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemRuntimeCompilerServicesConfiguredCancelableAsyncEnumerable);

                context.RegisterOperationBlockStartAction(context =>
                {
                    if (context.OwningSymbol is IMethodSymbol method)
                    {
                        if (method.IsAsync &&
                            method.ReturnsVoid &&
                            context.Options.GetBoolOptionValue(
                                optionName: EditorConfigOptionNames.ExcludeAsyncVoidMethods,
                                rule: Rule,
                                method,
                                context.Compilation,
                                defaultValue: false))
                        {
                            // Configured to skip this analysis in async void methods.
                            return;
                        }

                        context.RegisterOperationAction(context => AnalyzeAwaitOperation(context, taskTypes), OperationKind.Await);
                        if (configuredAsyncDisposable is not null)
                        {
                            context.RegisterOperationAction(context => AnalyzeUsingOperation(context, configuredAsyncDisposable), OperationKind.Using);
                            context.RegisterOperationAction(context => AnalyzeUsingDeclarationOperation(context, configuredAsyncDisposable), OperationKindEx.UsingDeclaration);
                        }
                        if (configuredCancelableAsyncEnumerable is not null)
                        {
                            context.RegisterOperationAction(context => AnalyzeForEachOperation(context, configuredCancelableAsyncEnumerable), OperationKind.Loop);
                        }
                    }
                });
            });
        }

        private static void AnalyzeAwaitOperation(OperationAnalysisContext context, ImmutableArray<INamedTypeSymbol> taskTypes)
        {
            var awaitExpression = (IAwaitOperation)context.Operation;

            // Get the type of the expression being awaited and check it's a task type.
            ITypeSymbol? typeOfAwaitedExpression = awaitExpression.Operation.Type;
            if (typeOfAwaitedExpression != null && taskTypes.Contains(typeOfAwaitedExpression.OriginalDefinition))
            {
                context.ReportDiagnostic(awaitExpression.Operation.Syntax.CreateDiagnostic(Rule));
            }
        }

        private static void AnalyzeForEachOperation(OperationAnalysisContext context,
            INamedTypeSymbol configuredCancelableAsyncEnumerable)
        {
            if (context.Operation is not IForEachLoopOperation operation)
                return;

            if (!operation.IsAsynchronous())
            {
                return;
            }

            // Get the type of the expression being iterated over.
            IOperation collectionOperation = operation.Collection;
            ITypeSymbol? typeOfCollection = collectionOperation.Type;
            if (Equals(typeOfCollection.OriginalDefinition, configuredCancelableAsyncEnumerable))
            {
                // Operation should be conversion to IAsyncEnumerable
                if (collectionOperation is IConversionOperation conversionOperation)
                {
                    if (conversionOperation.Operand is ILocalReferenceOperation localReferenceOperation)
                    {
                        // Can we find the initializer of this local?
                        var location = localReferenceOperation.Local.Locations.First();
                        SyntaxNode? node = location.SourceTree?.GetRoot(context.CancellationToken)
                                                               .FindNode(location.SourceSpan);
                        if (node is not null)
                        {
                            // Checking VariableDeclaratorSyntax.Initializer is CSharp specific
                            // Do we need to move rule to CSharp.NetAnalyzers?
                            if (node.ToString().Contains(ConfigureAwait))
                            {
                                return;
                            }
                        }
                    }
                    else if (conversionOperation.Operand is IInvocationOperation)
                    {
                        // Cannot use pattern matching as we want a nullable declaration.
#pragma warning disable IDE0020 // Use pattern matching
                        IInvocationOperation? invocationOperation = (IInvocationOperation)conversionOperation.Operand;
#pragma warning restore IDE0020 // Use pattern matching

                        // Differentiate between
                        // .WithCancellation(...).ConfigureAwait(...)
                        // and .ConfigureAwait(...).WithCancellation(...)
                        // Fall back to just string comparison as it could be either
                        // the ConfiguredCancelableAsyncEnumerable member method or
                        // the TaskAsyncEnumerableExtensions extension method.
                        if (invocationOperation.TargetMethod.Name == "WithCancellation")
                        {
                            invocationOperation = invocationOperation.Instance as IInvocationOperation;
                        }

                        if (invocationOperation?.TargetMethod.Name == ConfigureAwait)
                        {
                            return;
                        }
                    }
                }
            }

            context.ReportDiagnostic(collectionOperation.Syntax.CreateDiagnostic(Rule));
        }

        private static void AnalyzeUsingOperation(OperationAnalysisContext context, INamedTypeSymbol configuredAsyncDisposable)
        {
            var usingExpression = (IUsingOperation)context.Operation;
            if (!usingExpression.IsAsynchronous())
            {
                return;
            }

            if (usingExpression.Resources is IVariableDeclarationGroupOperation variableDeclarationGroup)
            {
                foreach (var declaration in variableDeclarationGroup.Declarations)
                {
                    foreach (var declarator in declaration.Declarators)
                    {
                        // Get the type of the expression being awaited and check it's a task type.
                        if (declarator.Symbol.Type != configuredAsyncDisposable)
                        {
                            context.ReportDiagnostic(declarator.Initializer.Value.Syntax.CreateDiagnostic(Rule));
                        }
                    }
                }
            }
        }

        private static void AnalyzeUsingDeclarationOperation(OperationAnalysisContext context, INamedTypeSymbol configuredAsyncDisposable)
        {
            var usingExpression = IUsingDeclarationOperationWrapper.FromOperation(context.Operation);
            if (!usingExpression.IsAsynchronous)
            {
                return;
            }

            foreach (var declaration in usingExpression.DeclarationGroup.Declarations)
            {
                foreach (var declarator in declaration.Declarators)
                {
                    // Get the type of the expression being awaited and check it's a task type.
                    if (declarator.Symbol.Type != configuredAsyncDisposable)
                    {
                        context.ReportDiagnostic(declarator.Initializer.Value.Syntax.CreateDiagnostic(Rule));
                    }
                }
            }
        }

        private static bool TryGetTaskTypes(Compilation compilation, out ImmutableArray<INamedTypeSymbol> taskTypes)
        {
            INamedTypeSymbol? taskType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksTask);
            INamedTypeSymbol? taskOfTType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksTask1);

            if (taskType == null || taskOfTType == null)
            {
                taskTypes = ImmutableArray<INamedTypeSymbol>.Empty;
                return false;
            }

            INamedTypeSymbol? valueTaskType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksValueTask);
            INamedTypeSymbol? valueTaskOfTType = compilation.GetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksValueTask1);

            taskTypes = valueTaskType != null && valueTaskOfTType != null ?
                ImmutableArray.Create(taskType, taskOfTType, valueTaskType, valueTaskOfTType) :
                ImmutableArray.Create(taskType, taskOfTType);

            return true;
        }
    }
}
