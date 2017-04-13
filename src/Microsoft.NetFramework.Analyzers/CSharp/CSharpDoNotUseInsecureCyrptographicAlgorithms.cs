// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NetFramework.Analyzers;
using Microsoft.NetFramework.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.NetFramework.CSharp.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpDoNotUseInsecureCryptographicAlgorithmsAnalyzer : DoNotUseInsecureCryptographicAlgorithmsAnalyzer
    {
        protected override SyntaxNodeAnalyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes cryptTypes)
        {
            SyntaxNodeAnalyzer analyzer = new SyntaxNodeAnalyzer(cryptTypes);
            context.RegisterSyntaxNodeAction(analyzer.AnalyzeNode,
                                             SyntaxKind.InvocationExpression,
                                             SyntaxKind.ObjectCreationExpression);
            return analyzer;
        }
    }
}
