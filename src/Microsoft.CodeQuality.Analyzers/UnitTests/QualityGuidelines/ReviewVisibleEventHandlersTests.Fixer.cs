// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.QualityGuidelines.CSharp.Analyzers;
using Microsoft.QualityGuidelines.VisualBasic.Analyzers;
using Test.Utilities;

namespace Microsoft.QualityGuidelines.Analyzers.UnitTests
{
    public class ReviewVisibleEventHandlersFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicReviewVisibleEventHandlersAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpReviewVisibleEventHandlersAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicReviewVisibleEventHandlersFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpReviewVisibleEventHandlersFixer();
        }
    }
}