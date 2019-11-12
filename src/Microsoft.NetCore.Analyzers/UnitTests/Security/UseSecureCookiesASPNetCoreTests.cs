// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.UseSecureCookiesASPNetCore,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseSecureCookiesASPNetCoreTests
    {
        [Fact]
        public async Task TestHasWrongSecurePropertyAssignmentDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(12, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestHasWrongSecurePropertyAssignmentMaybeChangedRightDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestHasRightSecurePropertyAssignmentMaybeChangedWrongDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestAssignSecurePropertyAnUnassignedVariableMaybeChangedWrongDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestAssignSecurePropertyAnUnassignedVariableMaybeChangedRightDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(20, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestAssignSecurePropertyAnAssignedVariableMaybeChangedDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(21, 9, UseSecureCookiesASPNetCore.MaybeUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestHasWrongSecurePropertyInitializerDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(11, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestWithoutSecurePropertyAssignmentDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(11, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestParamterLengthLessThan3TrueDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

class TestClass
{
    public void TestMethod(string key, string value)
    {
        var responseCookies = new ResponseCookies(); 
        responseCookies.Append(key, value);
    }
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(10, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestGetCookieOptionsFromOtherMethodInterproceduralDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(10, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestPassCookieOptionsAsParameterInterproceduralDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(17, 9, UseSecureCookiesASPNetCore.DefinitelyUseSecureCookiesASPNetCoreRule)
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestHasRightSecurePropertyAssignmentNoDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}", ASPNetCoreApis.CSharp,
                    }
                }
            }.RunAsync();
        }

        [Fact]
        public async Task TestHasRightSecurePropertyInitializerNoDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}",  ASPNetCoreApis.CSharp,
                    }
                }
            }.RunAsync();
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule)
           => VerifyCS.Diagnostic(rule)
               .WithLocation(line, column);
    }
}
