// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace System.Runtime.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpTypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer : TypesThatOwnDisposableFieldsShouldBeDisposableAnalyzer<TypeDeclarationSyntax>
    {
        protected override DisposableFieldAnalyzer GetAnalyzer(INamedTypeSymbol disposableTypeSymbol)
        {
            return new CSharpDisposableFieldAnalyzer(disposableTypeSymbol);
        }

        protected sealed class CSharpDisposableFieldAnalyzer : DisposableFieldAnalyzer
        {
            public CSharpDisposableFieldAnalyzer(INamedTypeSymbol disposableTypeSymbol)
                : base(disposableTypeSymbol)
            { }

            protected override bool IsDisposableFieldCreation(SyntaxNode node, SemanticModel model, HashSet<ISymbol> disposableFields, CancellationToken cancellationToken)
            { 
                if (node is AssignmentExpressionSyntax)
                {
                    var assignment = (AssignmentExpressionSyntax)node;
                    if (assignment.Right is ObjectCreationExpressionSyntax &&
                        disposableFields.Contains(model.GetSymbolInfo(assignment.Left, cancellationToken).Symbol))
                    {
                        return true;
                    }    
                }
                else if (node is FieldDeclarationSyntax)
                {
                    var fieldDecl = ((FieldDeclarationSyntax)node).Declaration;
                    foreach (var fieldInit in fieldDecl.Variables)
                    {                                               
                        if (fieldInit?.Initializer?.Value is ObjectCreationExpressionSyntax &&
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
