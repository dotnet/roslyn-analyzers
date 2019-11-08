// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotDisableHTTPHeaderChecking,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotDisableHTTPHeaderCheckingTests
    {
        [Fact]
        public async Task TestLiteralDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Web.Configuration;

class TestClass
{
    public void TestMethod()
    {
        var httpRuntimeSection = new HttpRuntimeSection();
        httpRuntimeSection.EnableHeaderChecking = false;
    }
}",
            GetCSharpResultAt(10, 9));
        }

        [Fact]
        public async Task TestConstantDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Web.Configuration;

class TestClass
{
    public void TestMethod()
    {
        const bool flag = false;
        var httpRuntimeSection = new HttpRuntimeSection();
        httpRuntimeSection.EnableHeaderChecking = flag;
    }
}",
            GetCSharpResultAt(11, 9));
        }

        [Fact]
        public async Task TestPropertyInitializerDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Web.Configuration;

class TestClass
{
    public void TestMethod()
    {
        var httpRuntimeSection = new HttpRuntimeSection
        {
            EnableHeaderChecking = false
        };
    }
}",
            GetCSharpResultAt(11, 13));
        }

        //Ideally, we would generate a diagnostic in this case.
        [Fact]
        public async Task TestVariableNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Web.Configuration;

class TestClass
{
    public void TestMethod()
    {
        var flag = false;
        var httpRuntimeSection = new HttpRuntimeSection();
        httpRuntimeSection.EnableHeaderChecking = flag;
    }
}");
        }

        [Fact]
        public async Task TestLiteralNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Web.Configuration;

class TestClass
{
    public void TestMethod()
    {
        var httpRuntimeSection = new HttpRuntimeSection();
        httpRuntimeSection.EnableHeaderChecking = true;
    }
}");
        }

        [Fact]
        public async Task TestConstantNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Web.Configuration;

class TestClass
{
    public void TestMethod()
    {
        const bool flag = true;
        var httpRuntimeSection = new HttpRuntimeSection();
        httpRuntimeSection.EnableHeaderChecking = flag;
    }
}");
        }

        [Fact]
        public async Task TestPropertyInitializerNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Web.Configuration;

class TestClass
{
    public void TestMethod()
    {
        var httpRuntimeSection = new HttpRuntimeSection
        {
            EnableHeaderChecking = true
        };
    }
}");
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column);
    }
}
