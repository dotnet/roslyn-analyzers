﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseSecureCookiesASPNetCoreTests : DiagnosticAnalyzerTestBase
    {
        protected void VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            string microsoftAspNetCoreHttpNamespaceCSharpSourceCode = @"
namespace Microsoft.AspNetCore.Http
{
    public interface IResponseCookies
    {
        void Append(string key, string value);

        void Append(string key, string value, CookieOptions options);
    }

    public class CookieOptions
    {
        public CookieOptions()
        {
        }

        public bool Secure { get; set; }
    }

    namespace Internal
    {
        public class ResponseCookies : IResponseCookies
        {
            public ResponseCookies()
            {
            }

            public void Append(string key, string value)
            {
            }

            public void Append(string key, string value, CookieOptions options)
            {
            }
        }
    }
}";
            this.VerifyCSharp(
                new[] { source, microsoftAspNetCoreHttpNamespaceCSharpSourceCode }.ToFileAndSource(),
                expected);
        }

        [Fact]
        public void TestHasWrongSecurePropertyAssignmentDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = false;
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(12, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestHasWrongSecurePropertyAssignmentMaybeChangedRightDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = false;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            cookieOptions.Secure = true;
        }

        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestHasRightSecurePropertyAssignmentMaybeChangedWrongDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = true;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            cookieOptions.Secure = false;
        }

        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestAssignSecurePropertyAnUnassignedVariableMaybeChangedWrongDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value, bool secure)
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = secure;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            cookieOptions.Secure = false;
        }

        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestAssignSecurePropertyAnUnassignedVariableMaybeChangedRightDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value, bool secure)
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = secure;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            cookieOptions.Secure = true;
        }

        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestAssignSecurePropertyAnAssignedVariableMaybeChangedDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions();
        var secure = true;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            secure = false;
        }
        
        cookieOptions.Secure = secure;
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(21, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestHasWrongSecurePropertyInitializerDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions() { Secure = false };
        var responseCookies = new ResponseCookies();
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(11, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestWithoutSecurePropertyAssignmentDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions();
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(11, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestParamterLengthLessThan3TrueDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value);
    }
}",
            GetCSharpResultAt(10, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestGetCookieOptionsFromOtherMethodInterproceduralDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, GetCookieOptions());
    }

    public CookieOptions GetCookieOptions()
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = false;

        return cookieOptions;
    }
}",
            GetCSharpResultAt(10, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestPassCookieOptionsAsParameterInterproceduralDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = false;
        TestMethod2(key, value, cookieOptions); 
    }

    public void TestMethod2(string key, string value, CookieOptions cookieOptions)
    {
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}",
            GetCSharpResultAt(17, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule));
        }

        [Fact]
        public void TestHasRightSecurePropertyAssignmentNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions();
        cookieOptions.Secure = true;
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value, cookieOptions);
    }
}");
        }

        [Fact]
        public void TestHasRightSecurePropertyInitializerNoDiagnostic()
        {
            VerifyCSharpWithDependencies(@"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var cookieOptions = new CookieOptions() { Secure = true };
        var responseCookies = new ResponseCookies();
        responseCookies.Append(key, value, cookieOptions);
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseSecureCookiesASPNetCore();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseSecureCookiesASPNetCore();
        }
    }
}
