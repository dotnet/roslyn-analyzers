// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    /// <summary>
    /// CA2201: Do not raise reserved exception types
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotRaiseReservedExceptionTypesAnalyzer : DoNotRaiseReservedExceptionTypesAnalyzer<SyntaxKind, ObjectCreationExpressionSyntax>
    {
        public override SyntaxKind ObjectCreationExpressionKind => SyntaxKind.ObjectCreationExpression;

        public override SyntaxNode GetTypeSyntaxNode(ObjectCreationExpressionSyntax node) => node.Type;
    }
}