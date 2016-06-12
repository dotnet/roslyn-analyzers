﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers
{
    public abstract partial class DiagnosticAnalyzerCorrectnessAnalyzer : DiagnosticAnalyzer
    {
        protected abstract class SyntaxNodeWithinAnalyzerTypeCompilationAnalyzer<TClassDeclarationSyntax, TSyntaxNodeOfInterest> : DiagnosticAnalyzerSymbolAnalyzer
            where TClassDeclarationSyntax : SyntaxNode
            where TSyntaxNodeOfInterest : SyntaxNode
        {
            protected SyntaxNodeWithinAnalyzerTypeCompilationAnalyzer(INamedTypeSymbol diagnosticAnalyzer, INamedTypeSymbol diagnosticAnalyzerAttribute)
                : base(diagnosticAnalyzer, diagnosticAnalyzerAttribute)
            {
            }

            internal IEnumerable<TClassDeclarationSyntax> GetClassDeclarationNodes(INamedTypeSymbol namedType, CancellationToken cancellationToken)
            {
                foreach (SyntaxNode syntax in namedType.DeclaringSyntaxReferences.Select(s => s.GetSyntax(cancellationToken)))
                {
                    if (syntax != null)
                    {
                        TClassDeclarationSyntax classDecl = syntax.FirstAncestorOrSelf<TClassDeclarationSyntax>(ascendOutOfTrivia: false);
                        if (classDecl != null)
                        {
                            yield return classDecl;
                        }
                    }
                }
            }

            protected override void AnalyzeDiagnosticAnalyzer(SymbolAnalysisContext symbolContext)
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;
                IEnumerable<TClassDeclarationSyntax> classDecls = GetClassDeclarationNodes(namedType, symbolContext.CancellationToken);
                foreach (TClassDeclarationSyntax classDecl in classDecls)
                {
                    IEnumerable<TSyntaxNodeOfInterest> syntaxNodes = classDecl.DescendantNodes(n => !(n is TClassDeclarationSyntax) || ReferenceEquals(n, classDecl)).OfType<TSyntaxNodeOfInterest>();
                    if (syntaxNodes.Any())
                    {
                        SemanticModel semanticModel = symbolContext.Compilation.GetSemanticModel(classDecl.SyntaxTree);
                        foreach (TSyntaxNodeOfInterest syntaxNode in syntaxNodes)
                        {
                            AnalyzeNode(symbolContext, syntaxNode, semanticModel);
                        }
                    }
                }
            }

            protected abstract void AnalyzeNode(SymbolAnalysisContext symbolContext, TSyntaxNodeOfInterest syntaxNode, SemanticModel semanticModel);
        }
    }
}
