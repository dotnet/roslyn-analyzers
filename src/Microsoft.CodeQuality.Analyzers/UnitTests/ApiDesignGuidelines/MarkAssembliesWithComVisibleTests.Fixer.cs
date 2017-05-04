// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class MarkAssembliesWithComVisibleFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MarkAssembliesWithComVisibleAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MarkAssembliesWithComVisibleAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new MarkAssembliesWithComVisibleFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MarkAssembliesWithComVisibleFixer();
        }
    }
}