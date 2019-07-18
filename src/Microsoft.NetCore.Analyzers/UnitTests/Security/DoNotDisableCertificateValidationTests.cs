// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotDisableCertificateValidationTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestLambdaDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => { return true; };
    }
}",
            GetCSharpResultAt(8, 68, DoNotDisableCertificateValidation.Rule));
        }

        [Fact]
        public void TestLambdaWithLiteralValueDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;
    }
}",
            GetCSharpResultAt(8, 68, DoNotDisableCertificateValidation.Rule));
        }

        [Fact]
        public void TestAnonymousMethodDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
    }
}",
            GetCSharpResultAt(8, 68, DoNotDisableCertificateValidation.Rule));
        }

        [Fact]
        public void TestDelegateCreationLocalFunctionDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

        bool AcceptAllCertifications(
                  object sender,
                  X509Certificate certificate,
                  X509Chain chain,
                  SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}",
            GetCSharpResultAt(10, 67, DoNotDisableCertificateValidation.Rule));
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
    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}",
            GetCSharpResultAt(19, 67, DoNotDisableCertificateValidation.Rule));

            VerifyBasic(@"
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates

Namespace TestNamespace
    Class TestClass
        Sub TestMethod()
            System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AcceptAllCertifications)
        End Sub

        Function AcceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
            Return True
        End Function
    End Class
End Namespace",
            GetBasicResultAt(9, 82, DoNotDisableCertificateValidation.Rule));
        }

        [Fact]
        public void TestDelegateCreationNormalMethodWithLambdaDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors) => true;

    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}",
            GetCSharpResultAt(16, 67, DoNotDisableCertificateValidation.Rule));
        }

        [Fact]
        public void TestDelegatedMethodFromDifferentAssemblyNoDiagnostic()
        {
            string source1 = @"

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AcceptAllCertificationsNamespace
{
    public class AcceptAllCertificationsClass
    {
        public static bool AcceptAllCertifications(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}";

            var source2 = @"
using System.Net;
using System.Net.Security;
using AcceptAllCertificationsNamespace;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertificationsClass.AcceptAllCertifications);
    }
}";

            VerifyCSharpAcrossTwoAssemblies(source1, source2);
        }

        [Fact]
        public void TestDelegatedMethodFromLocalFromDifferentAssemblyNoDiagnostic()
        {
            string source1 = @"

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AcceptAllCertificationsNamespace
{
    public class AcceptAllCertificationsClass
    {
        public static bool AcceptAllCertifications2(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

}";

            var source2 = @"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AcceptAllCertificationsNamespace;

class TestClass
{
    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        return AcceptAllCertificationsClass.AcceptAllCertifications2(sender, certificate, chain, sslPolicyErrors);
    }

    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}";

            VerifyCSharpAcrossTwoAssemblies(source1, source2);
        }

        [Fact]
        public void TestLambdaNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => { if(a != null) {return true;} return false;};
    }
}");
        }

        [Fact]
        public void TestLambdaWithLiteralValueNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;

class TestClass
{
    public void TestMethod()
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
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback += delegate { return false; };
    }
}");
        }

        [Fact]
        public void TestDelegateCreationLocalFunctionNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

        bool AcceptAllCertifications(
                  object sender,
                  X509Certificate certificate,
                  X509Chain chain,
                  SslPolicyErrors sslPolicyErrors)
        {
            if(sender != null)
            {
                return true;
            }

            return false;
        }
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
    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        if(sender != null)
        {
            return true;
        }
        return false;
    }

    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}");

            VerifyBasic(@"
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates

Public Module TestModule
    Sub TestMethod()
        System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AcceptAllCertifications)
    End Sub

    Function AcceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
        If sender IsNot Nothing
            Return True
        Else
            Return False
        End If
    End Function
End Module");
        }

        [Fact]
        public void TestDelegateCreationNoDiagnostic2()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void TestDelegateCreationNormalMethodWithLambdaNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors) => false;

    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}");
        }

        [Fact]
        public void TestDelegateCreationFromLocalFromLocalNoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public bool AcceptAllCertifications2(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        return AcceptAllCertifications2(
          sender,
          certificate,
          chain,
          sslPolicyErrors);
    }

    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}");

            VerifyBasic(@"
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates

Public Module TestModule
    Sub TestMethod()
        System.Net.ServicePointManager.ServerCertificateValidationCallback = New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AcceptAllCertifications)
    End Sub

    Function AcceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
        Return AcceptAllCertifications2(sender, certification, chain, sslPolicyErrors)
    End Function

    Function AcceptAllCertifications2(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
        Return True
    End Function
End Module");
        }

        [Fact]
        public void TestDelegateCreationFromLocalFromLocal2NoDiagnostic()
        {
            VerifyCSharp(@"
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public bool AcceptAllCertifications2(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public bool AcceptAllCertifications(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
    {
        var a = 5;
        if(a > 1)
        {
            return true;
        }
        else
        {
            return AcceptAllCertifications2(
              sender,
              certificate,
              chain,
              sslPolicyErrors);
        }
    }

    public void TestMethod()
    {
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
}");
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