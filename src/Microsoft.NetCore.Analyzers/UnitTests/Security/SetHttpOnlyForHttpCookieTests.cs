// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class SetHttpOnlyForHttpCookieTests : DiagnosticAnalyzerTestBase
    {
        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            string httpCookieCSharpSourceCode = @"
namespace System.Web
{
    public sealed class HttpCookie
    {
        public HttpCookie (string name)
        {
        }

        public HttpCookie (string name, string value)
        {
        }
        
        public bool HttpOnly { get; set; }
    }
}";
            this.VerifyCSharp(
                new[] { source, httpCookieCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        [Fact]
        public void Test_AssignHttpOnlyWithFalse_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        httpCookie.HttpOnly = false;
    }
}",
            GetCSharpResultAt(9, 9, SetHttpOnlyForHttpCookie.Rule));
        }

        [Fact]
        public void Test_AssignHttpOnlyWithFalsePossibly_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            httpCookie.HttpOnly = false;
        }
    }
}",
            GetCSharpResultAt(14, 13, SetHttpOnlyForHttpCookie.Rule));
        }

        [Fact]
        public void Test_ReturnHttpCookieWithFalseHttpOnly_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public HttpCookie TestMethod(HttpCookie httpCookie)
    {
        httpCookie.HttpOnly = false;

        return httpCookie;
    }
}",
            GetCSharpResultAt(8, 9, SetHttpOnlyForHttpCookie.Rule));
        }

        [Fact]
        public void Test_ReturnHttpCookie_WithoutSettingHttpOnly_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public HttpCookie TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");

        return httpCookie;
    }
}",
            GetCSharpResultAt(10, 16, SetHttpOnlyForHttpCookie.Rule));
        }

        [Fact]
        public void Test_PassHttpCookieAsAParamter_WithoutSettingHttpOnly_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        TestMethod2(httpCookie);
    }

    public void TestMethod2(HttpCookie httpCookie)
    {
    }
}",
            GetCSharpResultAt(9, 21, SetHttpOnlyForHttpCookie.Rule));
        }

        [Fact]
        public void Test_PassHttpCookieAsAParamter_WithSettingHttpOnlyAsFalse_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        httpCookie.HttpOnly = false;
        TestMethod2(httpCookie);
    }

    public void TestMethod2(HttpCookie httpCookie)
    {
    }
}",
            GetCSharpResultAt(9, 9, SetHttpOnlyForHttpCookie.Rule));
        }

        [Fact]
        public void Test_PassHttpCookieAsAParamter_WithSettingHttpOnlyAsFalsePossibly_Diagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            httpCookie.HttpOnly = false;
        }

        TestMethod2(httpCookie);
    }

    public void TestMethod2(HttpCookie httpCookie)
    {
    }
}",
            GetCSharpResultAt(14, 13, SetHttpOnlyForHttpCookie.Rule));
        }

        [Fact]
        public void Test_CreateHttpCookieWithNullArguments_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(null, null);
    }
}");
        }

        [Fact]
        public void Test_AssignHttpOnlyWithTrue_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        httpCookie.HttpOnly = true;
    }
}");
        }

        [Fact]
        public void Test_JustObjectCreation_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
    }
}");
        }

        [Fact]
        public void Test_AssignHttpOnlyWithTruePossibly_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            httpCookie.HttpOnly = true;
        }
    }
}");
        }

        [Fact]
        public void Test_ReturnHttpCookieWithUnkownHttpOnly_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public HttpCookie TestMethod(HttpCookie httpCookie)
    {
        return httpCookie;
    }
}");
        }

        [Fact]
        public void Test_ReturnHttpCookieWithTrueHttpOnly_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public HttpCookie TestMethod(HttpCookie httpCookie)
    {
        httpCookie.HttpOnly = true;

        return httpCookie;
    }
}");
        }

        [Fact]
        public void Test_PassHttpCookieAsAParamter_WithSettingHttpOnlyAsTrue_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        httpCookie.HttpOnly = true;
        TestMethod2(httpCookie);
    }

    public HttpCookie TestMethod2(HttpCookie httpCookie)
    {
        return httpCookie;
    }
}");
        }

        [Fact]
        public void Test_PassHttpCookieAsAParamter_WithSettingHttpOnlyAsTruePossibly_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        var httpCookie = new HttpCookie(""cookieName"");
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            httpCookie.HttpOnly = true;
        }

        TestMethod2(httpCookie);
    }

    public void TestMethod2(HttpCookie httpCookie)
    {
    }
}");
        }

        [Fact]
        public void Test_PassHttpCookieWithNullValue_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public void TestMethod()
    {
        TestMethod2(null);
    }

    public void TestMethod2(HttpCookie httpCookie)
    {
    }
}");
        }

        [Fact]
        public void Test_ReturnNull_NoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System.Web;

class TestClass
{
    public HttpCookie TestMethod(HttpCookie httpCookie)
    {
        return null;
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SetHttpOnlyForHttpCookie();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SetHttpOnlyForHttpCookie();
        }
    }
}
