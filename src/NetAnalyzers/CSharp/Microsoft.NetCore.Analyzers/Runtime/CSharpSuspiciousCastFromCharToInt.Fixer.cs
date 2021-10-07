// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpSuspiciousCastFromCharToIntFixer : SuspiciousCastFromCharToIntFixer
    {
        internal override SyntaxNode GetMemberAccessExpressionSyntax(SyntaxNode invocationExpressionSyntax)
        {
            return ((InvocationExpressionSyntax)invocationExpressionSyntax).Expression;
        }

        internal override SyntaxNode GetDefaultValueExpression(SyntaxNode parameterSyntax) => ((ParameterSyntax)parameterSyntax).Default.Value;
    }
}
