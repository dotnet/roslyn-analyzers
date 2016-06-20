// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using ApiReview.CSharp.Analyzers;
using ApiReview.VisualBasic.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace ApiReview.Analyzers.UnitTests
{
    public class AvoidCallingProblematicMethodsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAvoidCallingProblematicMethodsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAvoidCallingProblematicMethodsAnalyzer();
        }
    }
}