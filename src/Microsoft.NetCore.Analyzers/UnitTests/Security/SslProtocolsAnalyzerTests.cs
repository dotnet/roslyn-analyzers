// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class SslProtocolsAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        public SslProtocolsAnalyzerTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void DocSample1_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5395 violation for using Tls11
        SslProtocols protocols = SslProtocols.Tls11 | SslProtocols.Tls12;
    }
}",
            GetCSharpResultAt(10, 34, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"),
            GetCSharpResultAt(10, 55, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample1_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Security.Authentication

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5395 violation for using Tls11
        Dim sslProtocols As SslProtocols = SslProtocols.Tls11 Or SslProtocols.Tls12
    End Sub
End Class
",
            GetBasicResultAt(8, 44, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"),
            GetBasicResultAt(8, 66, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample2_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5395 violation
        SslProtocols sslProtocols = (SslProtocols) 768;    // TLS 1.1
    }
}",
            GetCSharpResultAt(10, 37, SslProtocolsAnalyzer.DeprecatedRule, "768"));
        }

        [Fact]
        public void DocSample2_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Security.Authentication

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5395 violation
        Dim sslProtocols As SslProtocols = CType(768, SslProtocols)   ' TLS 1.1
    End Sub
End Class
",
            GetBasicResultAt(8, 44, SslProtocolsAnalyzer.DeprecatedRule, "768"));
        }

        [Fact]
        public void DocSample1_CSharp_Solution()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

public class TestClass
{
    public void Method()
    {
        // In .NET Framework 4.7.1 or later, let the operating system decide what TLS protocol version to use.
        // See https://docs.microsoft.com/dotnet/framework/network-programming/tls
        SslProtocols sslProtocols = SslProtocols.None;
    }
}");
        }

        [Fact]
        public void DocSample1_VB_Solution()
        {
            VerifyBasic(@"
Imports System
Imports System.Security.Authentication

Public Class TestClass
    Public Sub ExampleMethod()
        ' In .NET Framework 4.7.1 or later, let the operating system decide what TLS protocol version to use.
        ' See https://docs.microsoft.com/dotnet/framework/network-programming/tls
        Dim sslProtocols As SslProtocols = SslProtocols.None
    End Sub
End Class
");
        }

        [Fact]
        public void DocSample3_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5396 violation
        SslProtocols sslProtocols = SslProtocols.Tls12;
    }
}",
            GetCSharpResultAt(10, 37, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample3_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Security.Authentication

Public Class TestClass
    Public Function ExampleMethod() As SslProtocols
        ' CA5396 violation
        Return SslProtocols.Tls12
    End Function
End Class
",
            GetBasicResultAt(8, 16, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample4_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

public class ExampleClass
{
    public SslProtocols ExampleMethod()
    {
        // CA5396 violation
        return (SslProtocols) 3072;    // TLS 1.2
    }
}",
            GetCSharpResultAt(10, 16, SslProtocolsAnalyzer.HardcodedRule, "3072"));
        }

        [Fact]
        public void DocSample4_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Security.Authentication

Public Class TestClass
    Public Function ExampleMethod() As SslProtocols
        ' CA5396 violation
        Return CType(3072, SslProtocols)   ' TLS 1.2
    End Function
End Class
",
            GetBasicResultAt(8, 16, SslProtocolsAnalyzer.HardcodedRule, "3072"));
        }

        [Fact]
        public void Argument_Ssl2_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void Method(SslStream sslStream, string targetHost, X509CertificateCollection clientCertificates)
    {
        sslStream.AuthenticateAsClient(targetHost, clientCertificates, SslProtocols.Ssl2, false);
    }
}",
            GetCSharpResultAt(11, 72, SslProtocolsAnalyzer.DeprecatedRule, "Ssl2"));
        }

        [Fact]
        public void Argument_Tls12_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void Method(SslStream sslStream, string targetHost, X509CertificateCollection clientCertificates)
    {
        sslStream.AuthenticateAsClient(targetHost, clientCertificates, SslProtocols.Tls12, false);
    }
}",
            GetCSharpResultAt(11, 72, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [Fact]
        public void Argument_None_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

class TestClass
{
    public void Method(SslStream sslStream, string targetHost, X509CertificateCollection clientCertificates)
    {
        sslStream.AuthenticateAsClient(targetHost, clientCertificates, SslProtocols.None, false);
    }
}");
        }

        [Fact]
        public void UseSsl3_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        var a = SslProtocols.Ssl3;
    }
}",
            GetCSharpResultAt(9, 17, SslProtocolsAnalyzer.DeprecatedRule, "Ssl3"));
        }

        [Fact]
        public void UseTls_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        var a = SslProtocols.Tls;
    }
}",
            GetCSharpResultAt(9, 17, SslProtocolsAnalyzer.DeprecatedRule, "Tls"));
        }

        [Fact]
        public void UseTls11_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        SslProtocols protocols = SslProtocols.Tls11;
    }
}",
            GetCSharpResultAt(9, 34, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void UseSystemDefault_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        var a = SslProtocols.Default;
    }
}",
                GetCSharpResultAt(9, 17, SslProtocolsAnalyzer.DeprecatedRule, "Default"));
        }

        [Fact]
        public void UseTls12_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        SslProtocols protocols = SslProtocols.Tls12;
    }
}",
                GetCSharpResultAt(9, 34, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [FactUnlessTls13Unavailable]
        public void UseTls13_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        SslProtocols protocols = SslProtocols.Tls13;
    }
}",
                GetCSharpResultAt(9, 34, SslProtocolsAnalyzer.HardcodedRule, "Tls13"));
        }

        [Fact]
        public void UseTls12OrdTls11_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls11;
    }
}",
                GetCSharpResultAt(9, 34, SslProtocolsAnalyzer.HardcodedRule, "Tls12"),
                GetCSharpResultAt(9, 55, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void Use192CompoundAssignment_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public SslProtocols SslProtocols { get; set; }

    public void Method()
    {
        this.SslProtocols |= (SslProtocols)192;
    }
}",
                GetCSharpResultAt(11, 30, SslProtocolsAnalyzer.DeprecatedRule, "192"));
        }

        [Fact]
        public void Use384SimpleAssignment_Diagnostic()
        {
            // 384 = SchProtocols.Tls11Server | SchProtocols.Tls10Client
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public SslProtocols SslProtocols { get; set; }

    public void Method()
    {
        this.SslProtocols = (SslProtocols)384;
    }
}",
                GetCSharpResultAt(11, 29, SslProtocolsAnalyzer.DeprecatedRule, "384"));
        }

        [Fact]
        public void Use768SimpleAssignmentOrExpression_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public SslProtocols SslProtocols { get; set; }

    public void Method(SslProtocols input)
    {
        this.SslProtocols = input | (SslProtocols)768;
    }
}",
                GetCSharpResultAt(11, 37, SslProtocolsAnalyzer.DeprecatedRule, "768"));
        }

        [Fact]
        public void Use12288SimpleAssignmentOrExpression_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public SslProtocols SslProtocols { get; set; }

    public void Method(SslProtocols input)
    {
        this.SslProtocols = input | (SslProtocols)12288;
    }
}",
                GetCSharpResultAt(11, 37, SslProtocolsAnalyzer.HardcodedRule, "12288"));
        }

        [Fact]
        public void UseTls12OrTls11Or192_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public SslProtocols SslProtocols { get; set; }

    public void Method()
    {
        this.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | (SslProtocols)192;
    }
}",
                GetCSharpResultAt(11, 29, SslProtocolsAnalyzer.HardcodedRule, "Tls12"),
                GetCSharpResultAt(11, 50, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void UseTls12Or192_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        SslProtocols protocols = SslProtocols.Tls12 | (SslProtocols)192;
    }
}",
                GetCSharpResultAt(9, 34, SslProtocolsAnalyzer.HardcodedRule, "Tls12"),
                GetCSharpResultAt(9, 34, SslProtocolsAnalyzer.DeprecatedRule, "3264"));
        }

        [Fact]
        public void Use768DeconstructionAssignment_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public SslProtocols SslProtocols { get; set; }

    public void Method()
    {
        int i;
        (this.SslProtocols, i) = ((SslProtocols)384, 384);
    }
}");
            // Ideally we'd handle the IDeconstructionAssignment, but this code pattern seems unlikely.
        }

        [Fact]
        public void Use24Plus24SimpleAssignment_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        SslProtocols sslProtocols = (SslProtocols)(24 + 24);
    }
}",
                GetCSharpResultAt(9, 37, SslProtocolsAnalyzer.DeprecatedRule, "48"));
        }

        [Fact]
        public void Use768NotSslProtocols_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void Method()
    {
        int i = 384 | 768;
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SslProtocolsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SslProtocolsAnalyzer();
        }
    }
}
