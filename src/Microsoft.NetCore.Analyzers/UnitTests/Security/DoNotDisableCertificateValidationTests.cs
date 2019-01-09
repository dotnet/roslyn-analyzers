// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotDisableCertificateValidationTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestLambdaNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => { if(a != null) return true; return false;};
    }
}");
        }

        [Fact]
        public void TestLambda2NoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => false;
    }
}");
        }

        [Fact]
        public void TestAnonymousMethodNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += delegate { return false; };
    }
}");
        }

        [Fact]
        public void TestDelegateCreationNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public static bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        if(sender != null)
            return true;
        return false;
    }

    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}");
        }
        [Fact]
        public void TestLambdaDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => { return true; };
    }
}",
            GetCSharpResultAt(8, 68, DoNotDisableCertificateValidation.Rule, "AnonymousFunction"));
        }

        [Fact]
        public void TestLambda2Diagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;
    }
}",
            GetCSharpResultAt(8, 68, DoNotDisableCertificateValidation.Rule, "AnonymousFunction"));
        }

        [Fact]
        public void TestAnonymousMethodDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
    }
}",
            GetCSharpResultAt(8, 68, DoNotDisableCertificateValidation.Rule, "AnonymousFunction"));
        }

        [Fact]
        public void TestDelegateCreationDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public static bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    private static void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}",
            GetCSharpResultAt(19, 67, DoNotDisableCertificateValidation.Rule, "MethodReference"));
        }
        
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotDisableCertificateValidation();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotDisableCertificateValidation();
        }
    }
}