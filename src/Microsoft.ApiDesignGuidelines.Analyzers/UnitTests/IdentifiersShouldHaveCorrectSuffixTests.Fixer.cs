// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class IdentifiersShouldHaveCorrectSuffixFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicIdentifiersShouldHaveCorrectSuffixAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpIdentifiersShouldHaveCorrectSuffixAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicIdentifiersShouldHaveCorrectSuffixFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpIdentifiersShouldHaveCorrectSuffixFixer();
        }
    }
}