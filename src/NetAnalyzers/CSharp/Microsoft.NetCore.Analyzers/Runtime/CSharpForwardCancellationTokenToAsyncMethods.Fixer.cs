// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpForwardCancellationTokenToAsyncMethodsFixer : ForwardCancellationTokenToAsyncMethodsFixer
    {
        private static bool IsCancellationTokenParameter(ParameterSyntax parameter) =>
            parameter.Type is IdentifierNameSyntax typeIdentifier && typeIdentifier.Identifier.ValueText.Equals(CancellationTokenName, StringComparison.Ordinal);

        private static string? GetCancellationTokenName(SeparatedSyntaxList<ParameterSyntax> parameters) =>
            parameters.FirstOrDefault(p => IsCancellationTokenParameter(p))?.Identifier.ValueText;

        protected override bool TryGetAncestorDeclarationCancellationTokenParameterName(SyntaxNode node, out string? parameterName)
        {
            parameterName = null;

            SyntaxNode currentNode = node.Parent;
            while (currentNode != null)
            {
                if (currentNode is ParenthesizedLambdaExpressionSyntax lambdaNode)
                {
                    parameterName = GetCancellationTokenName(lambdaNode.ParameterList.Parameters);
                    break;
                }
                else if (currentNode is LocalFunctionStatementSyntax localNode)
                {
                    parameterName = GetCancellationTokenName(localNode.ParameterList.Parameters);
                    break;
                }
                else if (currentNode is MethodDeclarationSyntax methodNode)
                {
                    parameterName = GetCancellationTokenName(methodNode.ParameterList.Parameters);
                    break;
                }

                currentNode = currentNode.Parent;
            }

            return !string.IsNullOrEmpty(parameterName);
        }
    }
}
