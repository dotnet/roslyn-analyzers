﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpSpecifyCultureForToLowerAndToUpperAnalyzer : SpecifyCultureForToLowerAndToUpperAnalyzer
    {
        protected override Location GetMethodNameLocation(SyntaxNode invocationNode)
        {
            Debug.Assert(invocationNode.IsKind(SyntaxKind.InvocationExpression));

            var invocation = (InvocationExpressionSyntax)invocationNode;
            if (invocation.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                return ((MemberAccessExpressionSyntax)invocation.Expression).Name.GetLocation();
            }
            else if (invocation.Expression.IsKind(SyntaxKind.MemberBindingExpression))
            {
                return ((MemberBindingExpressionSyntax)invocation.Expression).Name.GetLocation();
            }
            return invocation.GetLocation();
        }
    }
}