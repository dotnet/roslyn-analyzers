// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class AvoidAsyncVoidTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAvoidAsyncVoidAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAvoidAsyncVoidAnalyzer();
        }
    }
}