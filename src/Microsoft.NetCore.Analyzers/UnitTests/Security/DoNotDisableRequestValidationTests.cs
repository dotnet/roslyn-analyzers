// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotDisableRequestValidationTests : DiagnosticAnalyzerTestBase
    {
        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            string validateInputAttributeCSharpSourceCode = @"
using System.IO;

namespace System.Web.Mvc
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
    public class ValidateInputAttribute : System.Attribute
    {
        public ValidateInputAttribute (bool enableValidation)
        {
        }
    }
}";
            this.VerifyCSharp(
                new[] { source, validateInputAttributeCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        [Fact]
        public void TestLiteralDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web.Mvc;

class TestClass
{
    [ValidateInput(false)]
    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 17, DoNotDisableRequestValidation.Rule, "TestMethod"));
        }

        [Fact]
        public void TestConstDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web.Mvc;

class TestClass
{
    private const bool flag = false;

    [ValidateInput(flag)]
    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(10, 17, DoNotDisableRequestValidation.Rule, "TestMethod"));
        }

        [Fact]
        public void TestLiteralNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web.Mvc;

class TestClass
{
    [ValidateInput(true)]
    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestConstNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web.Mvc;

class TestClass
{
    private const bool flag = true;

    [ValidateInput(flag)]
    public void TestMethod()
    {
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDisableRequestValidation();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDisableRequestValidation();
        }
    }
}
