// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.NetCore.Analyzers.Runtime;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CSharpForwardCancellationTokenToAsyncMethodsAnalyzer : ForwardCancellationTokenToAsyncMethodsAnalyzer
    {
        protected override SyntaxNode? GetMethodNameNode(SyntaxNode invocationNode)
        {
            if (invocationNode is InvocationExpressionSyntax invocationExpression)
            {
                return invocationExpression.Expression;
            }
            return null;
        }
    }
}
