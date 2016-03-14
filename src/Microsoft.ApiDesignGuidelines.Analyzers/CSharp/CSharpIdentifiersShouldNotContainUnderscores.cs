// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.ApiDesignGuidelines.Analyzers
{
    /// <summary>
    /// CA1707: Identifiers should not contain underscores
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpIdentifiersShouldNotContainUnderscoresAnalyzer : IdentifiersShouldNotContainUnderscoresAnalyzer
    {
        internal override void GetSyntaxNodeDiagnostics(CompilationStartAnalysisContext compilationStartAnalysisContext)
        {
            compilationStartAnalysisContext.RegisterSyntaxNodeAction(syntaxNodeAnalysisContext =>
            {
                AnalyzeSyntaxNode(syntaxNodeAnalysisContext);
            }, SyntaxKind.Parameter, SyntaxKind.TypeParameter);
        }
    }
}