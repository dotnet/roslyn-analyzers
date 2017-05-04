// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Microsoft.CodeQuality.CSharp.Analyzers.Documentation;
using Microsoft.CodeQuality.VisualBasic.Analyzers.Documentation;

namespace Microsoft.CodeQuality.Analyzers.Documentation.UnitTests
{
    public class AvoidUsingCrefTagsWithAPrefixFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAvoidUsingCrefTagsWithAPrefixAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAvoidUsingCrefTagsWithAPrefixAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicAvoidUsingCrefTagsWithAPrefixFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpAvoidUsingCrefTagsWithAPrefixFixer();
        }
    }
}