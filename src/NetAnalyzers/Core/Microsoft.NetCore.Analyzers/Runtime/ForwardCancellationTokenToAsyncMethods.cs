// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2016: Forward CancellationToken to async methods.
    /// 
    /// Conditions for positive cases:
    ///     - The node to analyze is an awaited invocation, or is the origin invocation of a ConfigureAwait.
    ///     - The parent method signature has a ct parameter.
    ///     - The invocation is not receiving a ct argument.
    ///     - The invocation method either:
    ///         - Only has one method version, but the signature has one ct, set to default.
    ///         - Has a method overload with the exact same arguments in the same order, plus one ct parameter at the end.
    ///     - An Action/Func instance that receives a ct, and there's an awaited invocation inside.
    ///         
    /// Conditions for negative cases:
    ///     - The parent method signature does not have a ct parameter.
    ///     - The node to analyze is not an awaited invocation.
    ///     - The awaited invocation is already receiving a ct argument.
    ///     - The invocation method does not have an overload with the exact same arguments that also receives a ct.
    ///         - Includes the case where the user is *not* passing the parent method ct argument, but rather creating a new ct inside the method and passing that as argument, like with CancellationTokenSource.
    ///         - Includes the case where the user is explicitly passing `default`, `default(CancellationToken)` or `CancellationToken.None` to the invocation.
    ///         - If the overload that receives a ct receives more than one ct, do not suggest it.
    ///
    /// Future improvements:
    ///     - Finding an overload with one ct parameter, but not in last position.
    ///     - Passing a named ct in a different position than default.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ForwardCancellationTokenToAsyncMethodsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2016";

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToAsyncMethodsDescription),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources)
        );

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToAsyncMethodsMessage),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources)
        );

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToAsyncMethodsTitle),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources)
        );
        internal static DiagnosticDescriptor ForwardCancellationTokenToAsyncMethodsRule = DiagnosticDescriptorHelper.Create(
            RuleId,
            s_localizableTitle,
            s_localizableMessage,
            DiagnosticCategory.Reliability,
            RuleLevel.BuildWarning,
            s_localizableDescription,
            isPortedFxCopRule: false,
            isDataflowRule: false
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(ForwardCancellationTokenToAsyncMethodsRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterCompilationStartAction(AnalyzeCompilationStart);
        }

        private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
        {
            if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingCancellationToken, out INamedTypeSymbol? cancellationTokenType))
            {
                return;
            }

            // Retrieve the ConfigureAwait methods that could also be detected, which can come from:
            // - A Task
            // - A generic Task
            // - A ValueTask
            // - A generic ValueTask
            if (!context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksTask, out INamedTypeSymbol? taskType) ||
                !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksValueTask, out INamedTypeSymbol? valueTaskType) ||
                !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksGenericTask, out INamedTypeSymbol? genericTaskType) ||
                !context.Compilation.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemThreadingTasksGenericValueTask, out INamedTypeSymbol? genericValueTaskType))
            {
                return;
            }

            const string configureAwaitName = "ConfigureAwait";
            if (!(taskType.GetMembers(configureAwaitName).OfType<IMethodSymbol>().FirstOrDefault() is IMethodSymbol taskConfigureAwaitMethod) ||
                !(genericTaskType.GetMembers(configureAwaitName).OfType<IMethodSymbol>().FirstOrDefault() is IMethodSymbol genericTaskConfigureAwaitMethod) ||
                !(valueTaskType.GetMembers(configureAwaitName).OfType<IMethodSymbol>().FirstOrDefault() is IMethodSymbol valueTaskConfigureAwaitMethod) ||
                !(genericValueTaskType.GetMembers(configureAwaitName).OfType<IMethodSymbol>().FirstOrDefault() is IMethodSymbol genericValueTaskConfigureAwaitMethod))
            {
                return;
            }

            context.RegisterOperationAction(context =>
            {
                if (ShouldAnalyze(
                    (IAwaitOperation)context.Operation,
                    cancellationTokenType,
                    taskConfigureAwaitMethod,
                    genericTaskConfigureAwaitMethod,
                    valueTaskConfigureAwaitMethod,
                    genericValueTaskConfigureAwaitMethod,
                    out IInvocationOperation? invocation))
                {
                    context.ReportDiagnostic(invocation!.CreateDiagnostic(ForwardCancellationTokenToAsyncMethodsRule));
                }
            },
            OperationKind.Await);
        }

        private static bool ShouldAnalyze(IAwaitOperation awaitOperation, INamedTypeSymbol cancellationTokenType,
            IMethodSymbol taskConfigureAwaitMethod,
            IMethodSymbol genericTaskConfigureAwaitMethod,
            IMethodSymbol valueTaskConfigureAwaitMethod,
            IMethodSymbol genericValueTaskConfigureAwaitMethod,
            out IInvocationOperation? invocation)
        {
            invocation = null;

            if (!(awaitOperation.Operation is IInvocationOperation awaitedInvocation))
            {
                return false;
            }
            invocation = awaitedInvocation;
            IMethodSymbol method = awaitedInvocation.TargetMethod;

            // Check if the child operation of the await is ConfigureAwait
            // in which case we should analyze the grandchild operation
            if (method.OriginalDefinition.Equals(taskConfigureAwaitMethod) ||
                method.OriginalDefinition.Equals(genericTaskConfigureAwaitMethod) ||
                method.OriginalDefinition.Equals(valueTaskConfigureAwaitMethod) ||
                method.OriginalDefinition.Equals(genericValueTaskConfigureAwaitMethod))
            {
                if (awaitedInvocation.Instance is IInvocationOperation instanceOperation)
                {
                    invocation = instanceOperation;
                    method = instanceOperation.TargetMethod;
                }
                else
                {
                    return false;
                }
            }

            // If the invocation has an optional implicit ct or an overload that takes a ct, continue
            if (!InvocationIgnoresOptionalCancellationToken(cancellationTokenType, invocation, method) &&
                !MethodHasCancellationTokenOverload(cancellationTokenType, method))
            {
                return false;
            }

            // Find the ancestor method that contains this invocation
            if (!TryGetAncestorDeclaration(invocation, out IMethodSymbol? methodDeclaration))
            {
                return false;
            }

            // Check if the ancestor method has a ct that we can pass to the invocation
            if (!VerifyMethodOnlyHasOneCancellationTokenAsLastArgument(cancellationTokenType, methodDeclaration!))
            {
                return false;
            }

            return true;
        }

        // Looks for an ancestor that could be a method or function declaration.
        private static bool TryGetAncestorDeclaration(IInvocationOperation invocation, out IMethodSymbol? declaration)
        {
            declaration = null;

            IOperation currentOperation = invocation.Parent;
            while (currentOperation != null)
            {
                if (currentOperation.Kind == OperationKind.AnonymousFunction && currentOperation is IAnonymousFunctionOperation anonymousFunction)
                {
                    declaration = anonymousFunction.Symbol;
                    break;
                }
                else if (currentOperation.Kind == OperationKind.LocalFunction && currentOperation is ILocalFunctionOperation localFunction)
                {
                    declaration = localFunction.Symbol;
                    break;
                }
                else if (currentOperation.Kind == OperationKind.MethodBody && currentOperation is IMethodBodyOperation methodBody)
                {
                    if (methodBody.SemanticModel.GetDeclaredSymbol(methodBody.Syntax) is IMethodSymbol method)
                    {
                        declaration = method;
                        break;
                    }
                }
                else if (currentOperation.Kind == OperationKind.Block && currentOperation is IBlockOperation methodBaseBody)
                {
                    if (methodBaseBody.SemanticModel.GetDeclaredSymbol(methodBaseBody.Syntax) is IMethodSymbol method)
                    {
                        declaration = method;
                        // There are many kinds of blocks, so only break if we found a method symbol for this block.
                        // Otherwise, blocks inside anonymous or local functions would not be detected - those would be the parent of the current operation.
                        break;
                    }
                }

                currentOperation = currentOperation.Parent;
            }

            return declaration != null;
        }

        // Check if the method only takes one ct and is the last parameter in the method signature.
        // We want to compare the current method signature to any others with the exact same arguments in the exact same order.
        private static bool VerifyMethodOnlyHasOneCancellationTokenAsLastArgument(ITypeSymbol cancellationTokenType, IMethodSymbol methodDeclaration)
        {
            return methodDeclaration.Parameters.Count(x => x.Type.Equals(cancellationTokenType)) == 1 &&
                methodDeclaration.Parameters.Last().Type.Equals(cancellationTokenType);
        }

        // Check if the currently used overload is the one that takes the ct, but is utilizing the default value offered in the method signature.
        // We want to offer a diagnostic for this case, so the user explicitly passes the ancestor's ct.
        private static bool InvocationIgnoresOptionalCancellationToken(ITypeSymbol cancellationTokenType, IInvocationOperation invocation, IMethodSymbol method)
        {
            if (method.Parameters.Any() && method.Parameters.Last() is IParameterSymbol lastParameter && lastParameter.Type.Equals(cancellationTokenType))
            {
                // If the ct parameter has a default value, return true if a value is not being explicitly passed in the invocation
                return lastParameter.IsOptional && // Has a default value
                       invocation.Arguments.Last() is IArgumentOperation lastArgument &&
                       lastArgument.IsImplicit; // The default value is being used
            }

            return false;
        }

        // Check if there's a method overload with the same parameters as this one, in the same order, plus a ct at the end.
        private static bool MethodHasCancellationTokenOverload(ITypeSymbol cancellationTokenType, IMethodSymbol method)
        {
            IMethodSymbol? overload = method.ContainingType.GetMembers(method.Name).OfType<IMethodSymbol>().FirstOrDefault(x => HasSameParametersPlusCancellationToken(cancellationTokenType, method, x));

            return overload != null;
        }

        // Checks if the parameters of the two passed methods only differ in a ct.
        private static bool HasSameParametersPlusCancellationToken(ITypeSymbol cancellationTokenType, IMethodSymbol originalMethod, IMethodSymbol methodToCompare)
        {
            if (!originalMethod.Name.Equals(methodToCompare.Name, StringComparison.Ordinal) ||
                originalMethod.Equals(methodToCompare) ||
                methodToCompare.Parameters.Length == 0 ||
                methodToCompare.Parameters.Length != originalMethod.Parameters.Length + 1 ||
                !methodToCompare.Parameters[methodToCompare.Parameters.Length - 1].Type.Equals(cancellationTokenType))
            {
                return false;
            }

            for (int i = 0; i < originalMethod.Parameters.Length; i++)
            {
                IParameterSymbol? originalParameter = originalMethod.Parameters[i];
                IParameterSymbol? comparedParameter = methodToCompare.Parameters[i];
                if (originalParameter == null || comparedParameter == null || !originalParameter.Type.Equals(comparedParameter.Type))
                {
                    return false;
                }
            }

            return true;
        }
    }
}