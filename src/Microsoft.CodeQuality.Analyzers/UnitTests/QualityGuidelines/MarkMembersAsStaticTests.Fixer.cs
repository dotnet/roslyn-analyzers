// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeQuality.CSharp.Analyzers.QualityGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.QualityGuidelines;
using Test.Utilities;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public class MarkMembersAsStaticFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MarkMembersAsStaticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MarkMembersAsStaticAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicMarkMembersAsStaticFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpMarkMembersAsStaticFixer();
        }
    }
}