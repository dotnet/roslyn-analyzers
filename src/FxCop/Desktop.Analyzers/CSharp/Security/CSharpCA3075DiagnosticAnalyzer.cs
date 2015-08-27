// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Desktop.Analyzers.Common;

namespace Desktop.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpCA3075DiagnosticAnalyzer : CA3075DiagnosticAnalyzer<SyntaxKind>
    {
        protected override void RegisterAnalyzer(CodeBlockStartAnalysisContext<SyntaxKind> context, CompilationSecurityTypes types, Version frameworkVersion)
        {
            CSharpAnalyzer analyzer = new CSharpAnalyzer(types, CSharpSyntaxNodeHelper.Default, frameworkVersion);
            context.RegisterSyntaxNodeAction(
                analyzer.AnalyzeNode,
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.VariableDeclarator);
            context.RegisterCodeBlockEndAction(analyzer.AnalyzeCodeBlockEnd);  
        }

        private class CSharpAnalyzer : Analyzer
        {
            public CSharpAnalyzer(CompilationSecurityTypes types, CSharpSyntaxNodeHelper helper, Version frameworkVersion) :
                base(types, helper, frameworkVersion)
            { }
             
            protected override bool IsObjectConstructionForTemporaryObject(SyntaxNode node)
            {
                if (node == null)
                {
                    return false;
                }

                SyntaxKind kind = node.Kind();
                if (kind != SyntaxKind.ObjectCreationExpression)
                {
                    return false;
                }                       

                foreach (SyntaxNode ancestor in node.Ancestors())
                {
                    SyntaxKind k = ancestor.Kind();
                    if (k == SyntaxKind.SimpleAssignmentExpression || k == SyntaxKind.VariableDeclarator)
                    {
                        return false;
                    }   
                }

                return true;  
            }
        }
    }
}
