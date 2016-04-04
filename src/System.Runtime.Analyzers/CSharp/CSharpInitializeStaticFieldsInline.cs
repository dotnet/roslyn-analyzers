// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpInitializeStaticFieldsInlineAnalyzer : InitializeStaticFieldsInlineAnalyzer<SyntaxKind>
    {
        protected override SyntaxKind AssignmentNodeKind => SyntaxKind.SimpleAssignmentExpression;

        protected override bool InitialiesStaticField(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var assignmentNode = (AssignmentExpressionSyntax)node;
            var leftSymbol = semanticModel.GetSymbolInfo(assignmentNode.Left, cancellationToken).Symbol as IFieldSymbol;
            return leftSymbol != null && leftSymbol.IsStatic;
        }
    }
}