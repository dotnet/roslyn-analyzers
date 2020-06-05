// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpForwardCancellationTokenToInvocationsFixer : ForwardCancellationTokenToInvocationsFixer
    {
        protected override bool TryGetInvocation(
            SemanticModel model,
            SyntaxNode node,
            CancellationToken ct,
            [NotNullWhen(true)] out IInvocationOperation? invocation)
        {
            invocation = null;

            IOperation operation;

            // If the method was invoked using nullability for the case of attempting to dereference a possibly null reference,
            // then the node.Parent.Parent is the actual invocation (and it will contain the dot as well)
            if (node.Parent is MemberBindingExpressionSyntax)
            {
                operation = model.GetOperation(node.Parent.Parent, ct);
            }
            else
            {
                operation = model.GetOperation(node.Parent, ct);
            }

            if (operation is IInvocationOperation invocationOperation)
            {
                invocation = invocationOperation;
            }

            return invocation != null;
        }

        protected override bool TryGetAncestorDeclarationCancellationTokenParameterName(
            SyntaxNode node,
            [NotNullWhen(returnValue: true)] out string? parameterName)
        {
            parameterName = null;

            SyntaxNode currentNode = node.Parent;
            IEnumerable<ParameterSyntax>? parameters = null;
            while (currentNode != null)
            {
                if (currentNode is ParenthesizedLambdaExpressionSyntax lambdaNode)
                {
                    parameters = lambdaNode.ParameterList.Parameters;
                }
                else if (currentNode is LocalFunctionStatementSyntax localNode)
                {
                    parameters = localNode.ParameterList.Parameters;
                }
                else if (currentNode is MethodDeclarationSyntax methodNode)
                {
                    parameters = methodNode.ParameterList.Parameters;
                }

                if (parameters != null)
                {
                    parameterName = GetCancellationTokenName(parameters);
                    break;
                }

                currentNode = currentNode.Parent;
            }

            // Unexpected CS8752: Parameter 'parameterName' must have a non-null value when exiting with 'true'
            // Active issue: https://github.com/dotnet/roslyn/issues/44526
#pragma warning disable CS8762
            return !string.IsNullOrEmpty(parameterName);
#pragma warning restore CS8762
        }

        protected override bool IsArgumentNamed(IArgumentOperation argumentOperation)
        {
            return argumentOperation.Syntax is ArgumentSyntax argumentNode && argumentNode.NameColon != null;
        }

        protected override SyntaxNode GetConditionalOperationInvocationExpression(SyntaxNode invocationNode)
        {
            return ((InvocationExpressionSyntax)invocationNode).Expression;
        }

        private static string? GetCancellationTokenName(IEnumerable<ParameterSyntax> parameters) =>
            parameters.Last()?.Identifier.ValueText;
    }
}
