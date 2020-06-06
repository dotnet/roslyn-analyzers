// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    /// <summary>
    /// CA2016: Forward CancellationToken to invocations.
    /// 
    /// Conditions for positive cases:
    ///     - The containing method signature receives a ct parameter. It can be a method, a nested method, an action or a func.
    ///     - The invocation method is not receiving a ct argument, and...
    ///     - The invocation method either:
    ///         - Has no overloads but its current signature receives an optional ct=default, currently implicit, or...
    ///         - Has a method overload with the exact same arguments in the same order, plus one ct parameter at the end.
    ///         
    /// Conditions for negative cases:
    ///     - The containing method signature does not receive a ct parameter.
    ///     - The invocation method signature receives a ct and one is already being explicitly passed, or...
    ///     - The invocation method does not have an overload with the exact same arguments that also receives a ct, or...
    ///     - The invocation method only has overloads that receive more than one ct.
    /// </summary>
    public abstract class ForwardCancellationTokenToInvocationsAnalyzer : DiagnosticAnalyzer
    {
        internal const string RuleId = "CA2016";

        protected abstract SyntaxNode? GetMethodNameNode(SyntaxNode invocationNode);

        private static readonly LocalizableString s_localizableDescription = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToInvocationsDescription),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources)
        );

        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToInvocationsMessage),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources)
        );

        private static readonly LocalizableString s_localizableTitle = new LocalizableResourceString(
            nameof(MicrosoftNetCoreAnalyzersResources.ForwardCancellationTokenToInvocationsTitle),
            MicrosoftNetCoreAnalyzersResources.ResourceManager,
            typeof(MicrosoftNetCoreAnalyzersResources)
        );
        internal static DiagnosticDescriptor ForwardCancellationTokenToInvocationsRule = DiagnosticDescriptorHelper.Create(
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
            ImmutableArray.Create(ForwardCancellationTokenToInvocationsRule);

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

            context.RegisterOperationAction(context =>
            {
                IInvocationOperation invocation = (IInvocationOperation)context.Operation;

                if (!(context.ContainingSymbol is IMethodSymbol containingSymbol))
                {
                    return;
                }

                if (!ShouldAnalyze(
                    invocation,
                    containingSymbol,
                    cancellationTokenType,
                    out string? cancellationTokenParameterName))
                {
                    return;
                }

                // Only underline the method name, not the whole invocation
                SyntaxNode? expressionNode = GetMethodNameNode(context.Operation.Syntax);
                if (expressionNode != null)
                {
                    context.ReportDiagnostic(expressionNode.CreateDiagnostic(ForwardCancellationTokenToInvocationsRule, cancellationTokenParameterName, invocation.TargetMethod.Name));
                }
            },
            OperationKind.Invocation);
        }

        // Determines if an invocation should trigger a diagnostic for this rule or not.
        private static bool ShouldAnalyze(
            IInvocationOperation invocation,
            IMethodSymbol containingSymbol,
            INamedTypeSymbol cancellationTokenType,
            [NotNullWhen(returnValue: true)] out string? cancellationTokenParameterName)
        {
            cancellationTokenParameterName = null;

            IMethodSymbol method = invocation.TargetMethod;

            // Check if the invocation's method has either an optional implicit ct or a params ct parameter, as well as an overload that takes one ct
            if (!InvocationHasCancellationTokenArgument(method, invocation.Arguments, cancellationTokenType) &&
                !MethodHasCancellationTokenOverload(method, cancellationTokenType))
            {
                return false;
            }

            // Check if the ancestor method has a ct that we can pass to the invocation
            if (!VerifyAncestorOnlyHasOneCancellationTokenAsLastArgument(cancellationTokenType, GetContainingSymbol(invocation, containingSymbol), out cancellationTokenParameterName))
            {
                return false;
            }

            return true;
        }

        // Try to find the most immediate containing symbol (anonymous or local function). If none is found, return the context containing symbol.
        private static IMethodSymbol GetContainingSymbol(IInvocationOperation invocation, IMethodSymbol containingSymbol)
        {
            IOperation currentOperation = invocation.Parent;

            while (currentOperation != null)
            {
                if (currentOperation.Kind == OperationKind.AnonymousFunction)
                {
                    return ((IAnonymousFunctionOperation)currentOperation).Symbol;
                }
                else if (currentOperation.Kind == OperationKind.LocalFunction)
                {
                    return ((ILocalFunctionOperation)currentOperation).Symbol;
                }

                currentOperation = currentOperation.Parent;
            }

            return containingSymbol;
        }

// Check if the method only takes one ct and is the last parameter in the method signature.
        // We want to compare the current method signature to any others with the exact same arguments in the exact same order.
        private static bool VerifyAncestorOnlyHasOneCancellationTokenAsLastArgument(
            INamedTypeSymbol cancellationTokenType,
            IMethodSymbol methodDeclaration,
            [NotNullWhen(returnValue: true)] out string? cancellationTokenParameterName)
        {
            if (methodDeclaration.Parameters.Count(x => x.Type.Equals(cancellationTokenType)) == 1 &&
                methodDeclaration.Parameters.Last() is IParameterSymbol lastParameter &&
                lastParameter.Type.Equals(cancellationTokenType)) // Covers the case when using an alias for ct
            {
                cancellationTokenParameterName = lastParameter.Name;
                return true;
            }

            cancellationTokenParameterName = null;
            return false;
        }

        // Checks if the invocation has an optional ct argument at the end or a params ct array at the end.
        private static bool InvocationHasCancellationTokenArgument(IMethodSymbol method, ImmutableArray<IArgumentOperation> arguments, INamedTypeSymbol cancellationTokenType)
        {
            return
                !method.Parameters.IsEmpty &&
                method.Parameters[method.Parameters.Length - 1] is IParameterSymbol lastParameter &&
                (InvocationIgnoresOptionalCancellationToken(lastParameter, arguments, cancellationTokenType) ||
                InvocationIsUsingParamsCancellationToken(lastParameter, cancellationTokenType));
        }

        // Check if the currently used overload is the one that takes the ct, but is utilizing the default value offered in the method signature.
        // We want to offer a diagnostic for this case, so the user explicitly passes the ancestor's ct.
        private static bool InvocationIgnoresOptionalCancellationToken(IParameterSymbol lastParameter, ImmutableArray<IArgumentOperation> arguments, INamedTypeSymbol cancellationTokenType)
        {
            if (lastParameter.Type.Equals(cancellationTokenType) &&
                lastParameter.IsOptional) // Has a default value being used
            {
                // Find out if the ct argument is using the default value
                return arguments.Any(x =>
                    x.Parameter.Type.Equals(cancellationTokenType) &&
                    x.ArgumentKind == ArgumentKind.DefaultValue); // The default value is being used
            }

            return false;
        }

        // Checks if the method has a `params CancellationToken[]` argument in the last position.
        private static bool InvocationIsUsingParamsCancellationToken(IParameterSymbol lastParameter, INamedTypeSymbol cancellationTokenType)
        {
            return lastParameter.IsParams &&
                   lastParameter.Type.Kind == SymbolKind.ArrayType &&
                   lastParameter.Type is IArrayTypeSymbol arrayTypeSymbol &&
                   arrayTypeSymbol.ElementType.Equals(cancellationTokenType);
        }

        // Check if there's a method overload with the same parameters as this one, in the same order, plus a ct at the end.
        private static bool MethodHasCancellationTokenOverload(IMethodSymbol method, ITypeSymbol cancellationTokenType)
        {

            IMethodSymbol? overload = method.ContainingType.GetMembers(method.Name)
                                                           .OfType<IMethodSymbol>()
                                                           .FirstOrDefault(methodToCompare =>
                                                                HasSameParametersPlusCancellationToken(cancellationTokenType, method, methodToCompare));

            return overload != null;

            // Checks if the parameters of the two passed methods only differ in a ct.
            static bool HasSameParametersPlusCancellationToken(ITypeSymbol cancellationTokenType, IMethodSymbol originalMethod, IMethodSymbol methodToCompare)
            {
                if (originalMethod.Equals(methodToCompare) ||
                    methodToCompare.Parameters.Length != originalMethod.Parameters.Length + 1 ||
                    !methodToCompare.Parameters[methodToCompare.Parameters.Length - 1].Type.Equals(cancellationTokenType)) // Covers the case when using an alias for ct
                {
                    return false;
                }

                for (int i = 0; i < originalMethod.Parameters.Length; i++)
                {
                    IParameterSymbol? originalParameter = originalMethod.Parameters[i];
                    IParameterSymbol? comparedParameter = methodToCompare.Parameters[i];
                    if (originalParameter == null || comparedParameter == null || !originalParameter.Type.Equals(comparedParameter.Type)) // Covers the case when using an alias for ct
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}