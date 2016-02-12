// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;

namespace System.Collections.Immutable.Analyzers.UnitTests
{
    public class DoNotCallToImmutableArrayOnAnImmutableArrayValueFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotCallToImmutableArrayOnAnImmutableArrayValueAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicDoNotCallToImmutableArrayOnAnImmutableArrayValueFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpDoNotCallToImmutableArrayOnAnImmutableArrayValueFixer();
        }
    }
}