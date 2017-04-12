// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.ApiDesignGuidelines.CSharp.Analyzers;
using Microsoft.ApiDesignGuidelines.VisualBasic.Analyzers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class IdentifiersShouldNotMatchKeywordsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new IdentifiersShouldNotMatchKeywordsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new IdentifiersShouldNotMatchKeywordsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicIdentifiersShouldNotMatchKeywordsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpIdentifiersShouldNotMatchKeywordsFixer();
        }
    }
}