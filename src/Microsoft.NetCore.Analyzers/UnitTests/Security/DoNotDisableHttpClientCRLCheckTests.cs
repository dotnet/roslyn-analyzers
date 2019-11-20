﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Test.Utilities.MinimalImplementations;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotDisableHttpClientCRLCheck,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PropertySetAnalysis)]
    public class DoNotDisableHttpClientCRLCheckTests
    {
        private static readonly DiagnosticDescriptor DefinitelyRule = DoNotDisableHttpClientCRLCheck.DefinitelyDisableHttpClientCRLCheckRule;
        private static readonly DiagnosticDescriptor MaybeRule = DoNotDisableHttpClientCRLCheck.MaybeDisableHttpClientCRLCheckRule;

        private async Task VerifyCSharpWithDependencies(string source, params DiagnosticResult[] expected)
        {
            var csharpTest = new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source, SystemNetHttpApis.CSharp }
                },
            };

            csharpTest.ExpectedDiagnostics.AddRange(expected);

            await csharpTest.RunAsync();
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_NotSet_DefaultWrong_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(9, 26, DefinitelyRule));
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_Wrong_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(10, 26, DefinitelyRule));
        }

        [Fact]
        public async Task Test_HttpClientHandler_CheckCertificateRevocationList_Wrong_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.CheckCertificateRevocationList = false;
        var httpClient = new HttpClient(httpClientHandler);
    }
}",
                GetCSharpResultAt(10, 26, DefinitelyRule));
        }

        [Fact]
        public async Task Test_CurlHandler_CheckCertificateRevocationList_Wrong_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var curlHandler = new CurlHandler();
        curlHandler.CheckCertificateRevocationList = false;
        var httpClient = new HttpClient(curlHandler);
    }
}",
                GetCSharpResultAt(10, 26, DefinitelyRule));
        }

        [Fact]
        public async Task Test_HttpClientWithHttpMessageHandlerAndBooleanParameters_WinHttpHandler_CheckCertificateRevocationList_Wrong_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod(bool disposeHandler)
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        var httpClient = new HttpClient(winHttpHandler, disposeHandler);
    }
}",
                GetCSharpResultAt(10, 26, DefinitelyRule));
        }

        [Fact]
        public async Task Test_WinHttpHandler_PropertyInitializer_CheckCertificateRevocationList_Wrong_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler() { CheckCertificateRevocationList = false };
        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(9, 26, DefinitelyRule));
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_UnknownOrRight_MaybeDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using System.Net.Http;

class TestClass
{
    void TestMethod(bool checkCertificateRevocationList)
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = checkCertificateRevocationList;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            winHttpHandler.CheckCertificateRevocationList = true;
        }

        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(18, 26, MaybeRule));
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_Wrong_ServerCertificateValidationCallback_Null_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        winHttpHandler.ServerCertificateValidationCallback = null;
        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(11, 26, DefinitelyRule));
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_Wrong_ServerCertificateValidationCallback_MaybeNull_MaybeDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            winHttpHandler.ServerCertificateValidationCallback = (HttpRequestMessage,X509Certificate2,X509Chain,SslPolicyErrors) => true;
        }
        
        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(18, 26, MaybeRule));
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_Wrong_ServerCertificateValidationCallback_NotNull_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        winHttpHandler.ServerCertificateValidationCallback = (HttpRequestMessage,X509Certificate2,X509Chain,SslPolicyErrors) => true;
        var httpClient = new HttpClient(winHttpHandler);
    }
}");
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_UnknownOrWrong_MaybeDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using System.Net.Http;

class TestClass
{
    void TestMethod(bool checkCertificateRevocationList)
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = checkCertificateRevocationList;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            winHttpHandler.CheckCertificateRevocationList = false;
        }

        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(18, 26, MaybeRule));
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_WrongOrRight_MaybeDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System;
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        Random r = new Random();

        if (r.Next(6) == 4)
        {
            winHttpHandler.CheckCertificateRevocationList = true;
        }

        var httpClient = new HttpClient(winHttpHandler);
    }
}",
                GetCSharpResultAt(18, 26, MaybeRule));
        }

        [Fact]
        public async Task Test_DerivedClassOfHttpClient_DefinitelyDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class DerivedClass : HttpClient
{
    public DerivedClass (HttpMessageHandler handler)
    {
    }
}

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        var derivedClass = new DerivedClass(winHttpHandler);
    }
}",
                GetCSharpResultAt(17, 28, DefinitelyRule));
        }

        [Fact]
        public async Task Test_HttpClientConstructorWithoutParameter_handlerSetByDefault_NoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = false;
        var httpClient = new HttpClient();
    }
}");
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_Right_NoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod()
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = true;
        var httpClient = new HttpClient(winHttpHandler);
    }
}");
        }

        [Fact]
        public async Task Test_WinHttpHandler_CheckCertificateRevocationList_Unknown_NoDiagnostic()
        {
            await VerifyCSharpWithDependencies(@"
using System.Net.Http;

class TestClass
{
    void TestMethod(bool checkCertificateRevocationList)
    {
        var winHttpHandler = new WinHttpHandler();
        winHttpHandler.CheckCertificateRevocationList = checkCertificateRevocationList;
        var httpClient = new HttpClient(winHttpHandler);
    }
}");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule)
            => VerifyCS.Diagnostic(rule)
                .WithLocation(line, column);
    }
}
