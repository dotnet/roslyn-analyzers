// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpUseToLowerInvariantOrToUpperInvariantAnalyzer : UseToLowerInvariantOrToUpperInvariantAnalyzer
    {
        protected override Location GetMethodNameLocation(SyntaxNode invocationNode)
        {
            Debug.Assert(invocationNode.IsKind(SyntaxKind.InvocationExpression));

            var invocation = invocationNode as InvocationExpressionSyntax;
            var expression = invocation.Expression;

            if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                return ((MemberAccessExpressionSyntax)expression).Name.GetLocation();
            }
            else if (expression.IsKind(SyntaxKind.ConditionalAccessExpression))
            {
                return ((ConditionalAccessExpressionSyntax)expression).WhenNotNull.GetLocation();
            }
            return invocation.GetLocation();
        }
    }
}
