// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.NetCore.CSharp.Analyzers.Composition;
using Microsoft.NetCore.VisualBasic.Analyzers.Composition;
using Test.Utilities;

namespace Microsoft.NetCore.Analyzers.Composition.UnitTests
{
    public class DoNotMixAttributesFromDifferentVersionsOfMEFFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotMixAttributesFromDifferentVersionsOfMEFAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotMixAttributesFromDifferentVersionsOfMEFAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotMixAttributesFromDifferentVersionsOfMEFFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotMixAttributesFromDifferentVersionsOfMEFFixer();
        }
    }
}