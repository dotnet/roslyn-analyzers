// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
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
        private static string? GetCancellationTokenName(SemanticModel model, IEnumerable<ParameterSyntax> parameters) =>
            parameters.FirstOrDefault(p => IsCancellationTokenParameter(model, p))?.Identifier.ValueText;

        protected override bool TryGetAncestorDeclarationCancellationTokenParameterName(SemanticModel model, SyntaxNode node, out string? parameterName)
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
                    parameterName = GetCancellationTokenName(model, parameters);
                    break;
                }

                currentNode = currentNode.Parent;
            }

            return !string.IsNullOrEmpty(parameterName);
        }
    }
}
