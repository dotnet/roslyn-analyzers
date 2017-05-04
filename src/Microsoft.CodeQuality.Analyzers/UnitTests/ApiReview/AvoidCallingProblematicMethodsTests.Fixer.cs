// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiReview;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiReview;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace Microsoft.CodeQuality.Analyzers.ApiReview.UnitTests
{
    public class AvoidCallingProblematicMethodsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAvoidCallingProblematicMethodsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAvoidCallingProblematicMethodsAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicAvoidCallingProblematicMethodsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpAvoidCallingProblematicMethodsFixer();
        }
    }
}