// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;                         

using Desktop.Analyzers.Common;

namespace Desktop.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpDoNotUseInsecureDtdProcessingInApiDesignAnalyzer : DoNotUseInsecureDtdProcessingInApiDesignAnalyzer
    {
        protected override Analyzer GetAnalyzer(CompilationStartAnalysisContext context, CompilationSecurityTypes types, Version targetFrameworkVersion)
        {
            Analyzer analyzer = new Analyzer(types, CSharpSyntaxNodeHelper.Default, targetFrameworkVersion);
            context.RegisterSyntaxNodeAction(analyzer.AnalyzeNode, SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration); 

            return analyzer;
        }
    }
}
