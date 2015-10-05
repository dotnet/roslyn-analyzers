// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Desktop.Analyzers.Common;

namespace Desktop.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpDoNotUseInsecureCryptographicAlgorithmsAnalyzer : DoNotUseInsecureCryptographicAlgorithmsAnalyzer
    {
        protected override Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes)
        {
            Analyzer analyzer = new Analyzer(cryptTypes);
            context.RegisterSyntaxNodeAction(analyzer.AnalyzeNode,
                                             SyntaxKind.InvocationExpression,
                                             SyntaxKind.ObjectCreationExpression);
            return analyzer;
        }
    }
}
