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
    public SslProtocols ExampleMethod()
    {
        // CA5395 violation
        return (SslProtocols) 768;    // TLS 1.1
    }
}",
            GetCSharpResultAt(12, 29, SslProtocolsAnalyzer.DeprecatedRule, "768"));
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
        Dim sslProtocols As SslProtocols = CType(768, SecurityProtocolType)   ' TLS 1.1
    End Sub
End Class
",
            GetBasicResultAt(8, 48, SslProtocolsAnalyzer.DeprecatedRule, "768"));
        }

        [Fact]
        public void DocSample1_CSharp_Solution()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

public class TestClass
{
    public void TestMethod()
    {
        // In .NET Framework 4.7.1 or later, let the operating system decide what TLS protocol version to use.
        // See https://docs.microsoft.com/dotnet/framework/network-programming/tls
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
        SslProtocols protocols = SecurityProtocolType.Tls12;
    }
}",
            GetCSharpResultAt(10, 48, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample3_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Security.Authentication

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5396 violation
        SslProtocols protocols = SslProtocols.Tls12
    End Sub
End Class
",
            GetBasicResultAt(8, 48, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample4_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5396 violation
        SslProtocols protocols = (SslProtocols) 3072;    // TLS 1.2
    }
}",
            GetCSharpResultAt(10, 48, SslProtocolsAnalyzer.HardcodedRule, "3072"));
        }

        [Fact]
        public void DocSample4_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Security.Authentication

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5396 violation
        SslProtocols protocols = CType(3072, SslProtocols)   ' TLS 1.2
    End Sub
End Class
",
            GetBasicResultAt(8, 48, SslProtocolsAnalyzer.HardcodedRule, "3072"));
        }

        [Fact]
        public void TestUseSsl3Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        var a = SslProtocols.Ssl3;
    }
}",
            GetCSharpResultAt(9, 17, SslProtocolsAnalyzer.DeprecatedRule, "Ssl3"));
        }

        [Fact]
        public void TestUseTlsDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        var a = SslProtocols.Tls;
    }
}",
            GetCSharpResultAt(9, 17, SslProtocolsAnalyzer.DeprecatedRule, "Tls"));
        }

        [Fact]
        public void TestUseTls11Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = SslProtocols.Tls11;
    }
}",
            GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void TestUseSystemDefaultNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        var a = SslProtocols.Default;
    }
}",
                GetCSharpResultAt(9, 30, SslProtocolsAnalyzer.DeprecatedRule, "Default"));
        }

        [Fact]
        public void TestUseTls12Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = SslProtocols.Tls12;
    }
}",
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.HardcodedRule, "Tls12"));
        }

        [FactUnlessTls13Unavailable]
        public void TestUseTls13Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = SslProtocols.Tls13;
    }
}",
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.HardcodedRule, "Tls13"));
        }

        [Fact]
        public void TestUseTls12OrdTls11Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls11;
    }
}",
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.HardcodedRule, "Tls12"),
                GetCSharpResultAt(9, 77, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void TestUse192CompoundAssignmentDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol |= (SslProtocols)192;
    }
}",
                GetCSharpResultAt(9, 49, SslProtocolsAnalyzer.DeprecatedRule, "192"));
        }

        [Fact]
        public void TestUse384SimpleAssignmentDiagnostic()
        {
            // 384 = SchProtocols.Tls11Server | SchProtocols.Tls10Client
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = (SslProtocols)384;
    }
}",
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.DeprecatedRule, "384"));
        }

        [Fact]
        public void TestUse768SimpleAssignmentOrExpressionDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod(SslProtocols in)
    {
        SslProtocols protocols = in | (SslProtocols)768;
    }
}",
                GetCSharpResultAt(9, 87, SslProtocolsAnalyzer.DeprecatedRule, "768"));
        }

        [Fact]
        public void TestUse12288SimpleAssignmentOrExpressionDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod(SslProtocols in)
    {
        SslProtocols protocols = in | (SslProtocols)12288;
    }
}",
                GetCSharpResultAt(9, 87, SslProtocolsAnalyzer.HardcodedRule, "12288"));
        }

        [Fact]
        public void TestUseTls12OrTls11Or192Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = SslProtocols.Tls12 | SslProtocols.Tls11 | (SslProtocols)192;
    }
}",
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.HardcodedRule, "Tls12"),
                GetCSharpResultAt(9, 77, SslProtocolsAnalyzer.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void TestUseTls12Or192Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = SslProtocols.Tls12 | (SslProtocols)192;
    }
}",
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.HardcodedRule, "Tls12"),
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.DeprecatedRule, "3264"));
        }

        [Fact]
        public void TestUse768DeconstructionAssignmentNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        int i;
        (ServicePointManager.SecurityProtocol, i) = ((SslProtocols)384, 384);
    }
}");
            // Ideally we'd handle the IDeconstructionAssignment, but this code pattern seems unlikely.
        }

        [Fact]
        public void TestUse24Plus24SimpleAssignmentDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
    {
        SslProtocols protocols = (SslProtocols)(24 + 24);
    }
}",
                GetCSharpResultAt(9, 48, SslProtocolsAnalyzer.DeprecatedRule, "48"));
        }

        [Fact]
        public void TestUse768NotSslProtocolsNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Security.Authentication;

class TestClass
{
    public void TestMethod()
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
