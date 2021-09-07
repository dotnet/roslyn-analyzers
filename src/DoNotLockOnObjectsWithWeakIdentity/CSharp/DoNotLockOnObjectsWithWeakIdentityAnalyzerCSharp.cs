// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CA2002
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotLockOnObjectsWithWeakIdentityAnalyzerCSharp : DoNotLockOnObjectsWithWeakIdentityAnalyzerBase
    {
        protected override bool IsThisExpression(SyntaxNode node) => node is ThisExpressionSyntax;
    }
}