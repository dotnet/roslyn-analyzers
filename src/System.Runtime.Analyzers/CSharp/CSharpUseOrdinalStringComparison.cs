// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpUseOrdinalStringComparisonAnalyzer : UseOrdinalStringComparisonAnalyzer
    {
        protected override Location GetMethodNameLocation(SyntaxNode invocationNode)
        {
            Debug.Assert(invocationNode.IsKind(SyntaxKind.InvocationExpression));

            var invocation = invocationNode as InvocationExpressionSyntax;
            if (invocation.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                return ((MemberAccessExpressionSyntax)invocation.Expression).Name.GetLocation();
            }
            else if (invocation.Expression.IsKind(SyntaxKind.ConditionalAccessExpression))
            {
                return ((ConditionalAccessExpressionSyntax)invocation.Expression).WhenNotNull.GetLocation();
            }

            return invocation.GetLocation();
        }

        protected override Location GetOperatorTokenLocation(SyntaxNode binaryOperationNode)
        {
            Debug.Assert(binaryOperationNode is BinaryExpressionSyntax);

            return ((BinaryExpressionSyntax)binaryOperationNode).OperatorToken.GetLocation();
        }
    }
}
