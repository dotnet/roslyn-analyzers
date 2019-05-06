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
        public void TestLiteralAtActionLevelDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

class TestControllerClass
{
    [ValidateInput(false)]
    public void TestActionMethod()
    {
    }
}",
            GetCSharpResultAt(7, 17, DoNotDisableRequestValidation.Rule, "TestActionMethod"));
        }

        [Fact]
        public void TestConstAtActionLevelDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

class TestControllerClass
{
    private const bool flag = false;

    [ValidateInput(flag)]
    public void TestActionMethod()
    {
    }
}",
            GetCSharpResultAt(9, 17, DoNotDisableRequestValidation.Rule, "TestActionMethod"));
        }

        [Fact]
        public void TestLiteralAtControllerLevelDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

[ValidateInput(false)]
class TestControllerClass
{
    public void TestActionMethod()
    {
    }
}",
            GetCSharpResultAt(5, 7, DoNotDisableRequestValidation.Rule, "TestControllerClass"));
        }

        [Fact]
        public void TestSetBothControllerLevelAndActionLevelDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

[ValidateInput(true)]
class TestControllerClass
{
    [ValidateInput(false)]
    public void TestActionMethod()
    {
    }
}",
            GetCSharpResultAt(8, 17, DoNotDisableRequestValidation.Rule, "TestActionMethod"));
        }

        [Fact]
        public void TestLiteralAtActionLevelNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

class TestControllerClass
{
    [ValidateInput(true)]
    public void TestActionMethod()
    {
    }
}");
        }

        [Fact]
        public void TestConstAtActionLevelNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

class TestControllerClass
{
    private const bool flag = true;

    [ValidateInput(flag)]
    public void TestActionMethod()
    {
    }
}");
        }

        [Fact]
        public void TestLiteralAtControllerLevelNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

[ValidateInput(true)]
class TestControllerClass
{
    public void TestActionMethod()
    {
    }
}");
        }

        [Fact]
        public void TestSetBothControllerLevelAndActionLevelNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

[ValidateInput(false)]
class TestControllerClass
{
    [ValidateInput(true)]
    public void TestActionMethod()
    {
    }
}");
        }

        [Fact]
        public void TestWithoutValidateInputAttributeNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web.Mvc;

class TestControllerClass
{
    public void TestActionMethod()
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
