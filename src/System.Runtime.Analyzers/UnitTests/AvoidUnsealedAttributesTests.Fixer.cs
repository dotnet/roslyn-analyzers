// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class AvoidUnsealedAttributesFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicAvoidUnsealedAttributesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpAvoidUnsealedAttributesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicAvoidUnsealedAttributesFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpAvoidUnsealedAttributesFixer();
        }
    }
}