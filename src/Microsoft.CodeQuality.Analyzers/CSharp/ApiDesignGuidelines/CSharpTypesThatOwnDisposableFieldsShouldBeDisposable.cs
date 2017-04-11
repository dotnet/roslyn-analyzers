﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.ApiDesignGuidelines.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.CSharp.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpTypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer : TypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer<TypeDeclarationSyntax>
    {
        protected override DisposableFieldAnalyzer GetAnalyzer(INamedTypeSymbol disposableTypeSymbol)
        {
            return new CSharpDisposableFieldAnalyzer(disposableTypeSymbol);
        }

        private class CSharpDisposableFieldAnalyzer : DisposableFieldAnalyzer
        {
            public CSharpDisposableFieldAnalyzer(INamedTypeSymbol disposableTypeSymbol)
                : base(disposableTypeSymbol)
            { }

            protected override bool IsDisposableFieldCreation(SyntaxNode node, SemanticModel model, HashSet<ISymbol> disposableFields, CancellationToken cancellationToken)
            {
                if (node is AssignmentExpressionSyntax assignment)
                {
                    if (assignment.Right is ObjectCreationExpressionSyntax &&
                        disposableFields.Contains(model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol))
                    {
                        return true;
                    }
                }
                else if (node is FieldDeclarationSyntax)
                {
                    VariableDeclarationSyntax fieldDecl = ((FieldDeclarationSyntax)node).Declaration;
                    foreach (VariableDeclaratorSyntax fieldInit in fieldDecl.Variables)
                    {
                        if (fieldInit.Initializer?.Value is ObjectCreationExpressionSyntax &&
                            disposableFields.Contains(model.GetDeclaredSymbol(fieldInit, cancellationToken)))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
