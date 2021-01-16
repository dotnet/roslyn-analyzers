// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Analyzers.MetaAnalyzers.Fixers;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.CodeAnalysis.CSharp.Analyzers.MetaAnalyzers.Fixers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSharpApplyDiagnosticAnalyzerAttributeFix)), Shared]
    internal sealed class CSharpDoNotCompareSyntaxTokenAgainstDefaultLiteralFix : DoNotCompareSyntaxTokenAgainstDefaultLiteralFix
    {
        protected override ISyntaxFacts SyntaxFacts => CSharpSyntaxFacts.Instance;
    }
}
