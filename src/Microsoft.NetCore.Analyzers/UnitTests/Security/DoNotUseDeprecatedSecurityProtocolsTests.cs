// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class FactUnlessTls13UnavailableAttribute : FactAttribute
    {
        public override string Skip
        {
            get
            {
                if (!typeof(SecurityProtocolType).GetEnumNames().Any(s => s == "Tls13"))
                {
                    return "SecurityProtocolType.Tls13 is unavailable";
                }
                else
                {
                    return base.Skip;
                }
            }

            set
            {
                base.Skip = value;
            }
        }
    }

    public class DoNotUseDeprecatedSecurityProtocolsTests : DiagnosticAnalyzerTestBase
    {
        public DoNotUseDeprecatedSecurityProtocolsTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void DocSample1_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Net;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5364 violation for using Tls11
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
    }
}",
            GetCSharpResultAt(10, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls11"),
            GetCSharpResultAt(10, 77, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample1_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Net

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5364 violation for using Tls11
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 Or SecurityProtocolType.Tls12
    End Sub
End Class
",
            GetBasicResultAt(8, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls11"),
            GetBasicResultAt(8, 78, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample2_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Net;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5364 violation
        ServicePointManager.SecurityProtocol = (SecurityProtocolType) 768;    // TLS 1.1
    }
}",
            GetCSharpResultAt(10, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "768"));
        }

        [Fact]
        public void DocSample2_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Net

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5364 violation
        ServicePointManager.SecurityProtocol = CType(768, SecurityProtocolType)   ' TLS 1.1
    End Sub
End Class
",
            GetBasicResultAt(8, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "768"));
        }

        [Fact]
        public void DocSample1_CSharp_Solution()
        {
            VerifyCSharp(@"
using System;
using System.Net;

public class TestClass
{
    public void TestMethod()
    {
        // Let the operating system decide what TLS protocol version to use.
        // See https://docs.microsoft.com/dotnet/framework/network-programming/tls
    }
}");
        }

        [Fact]
        public void DocSample1_VB_Solution()
        {
            VerifyBasic(@"
Imports System
Imports System.Net

Public Class TestClass
    Public Sub ExampleMethod()
        ' Let the operating system decide what TLS protocol version to use.
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
using System.Net;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5386 violation
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }
}",
            GetCSharpResultAt(10, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample3_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Net

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5386 violation
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
    End Sub
End Class
",
            GetBasicResultAt(8, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"));
        }

        [Fact]
        public void DocSample4_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;
using System.Net;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5386 violation
        ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;    // TLS 1.2
    }
}",
            GetCSharpResultAt(10, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "3072"));
        }

        [Fact]
        public void DocSample4_VB_Violation()
        {
            VerifyBasic(@"
Imports System
Imports System.Net

Public Class TestClass
    Public Sub ExampleMethod()
        ' CA5386 violation
        ServicePointManager.SecurityProtocol = CType(3072, SecurityProtocolType)   ' TLS 1.2
    End Sub
End Class
",
            GetBasicResultAt(8, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "3072"));
        }

        [Fact]
        public void TestUseSsl3Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        var a = SecurityProtocolType.Ssl3;
    }
}",
            GetCSharpResultAt(9, 17, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Ssl3"));
        }

        [Fact]
        public void TestUseTlsDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        var a = SecurityProtocolType.Tls;
    }
}",
            GetCSharpResultAt(9, 17, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls"));
        }

        [Fact]
        public void TestUseTls11Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
    }
}",
            GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void TestUseSystemDefaultNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        var a = SecurityProtocolType.SystemDefault;
    }
}");
        }

        [Fact]
        public void TestUseTls12Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }
}",
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"));
        }

        [FactUnlessTls13Unavailable]
        public void TestUseTls13Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
    }
}",
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls13"));
        }

        [Fact]
        public void TestUseTls12OrdTls11Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
    }
}",
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"),
                GetCSharpResultAt(9, 77, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void TestUse192CompoundAssignmentDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol |= (SecurityProtocolType)192;
    }
}",
                GetCSharpResultAt(9, 49, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "192"));
        }

        [Fact]
        public void TestUse384SimpleAssignmentDiagnostic()
        {
            // 384 = SchProtocols.Tls11Server | SchProtocols.Tls10Client
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)384;
    }
}",
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "384"));
        }

        [Fact]
        public void TestUse768SimpleAssignmentOrExpressionDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | (SecurityProtocolType)768;
    }
}",
                GetCSharpResultAt(9, 87, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "768"));
        }

        [Fact]
        public void TestUse12288SimpleAssignmentOrExpressionDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | (SecurityProtocolType)12288;
    }
}",
                GetCSharpResultAt(9, 87, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "12288"));
        }

        [Fact]
        public void TestUseTls12OrTls11Or192Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | (SecurityProtocolType)192;
    }
}",
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"),
                GetCSharpResultAt(9, 77, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls11"));
        }

        [Fact]
        public void TestUseTls12Or192Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)192;
    }
}",
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "Tls12"),
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "3264"));
        }

        [Fact]
        public void TestUse768DeconstructionAssignmentNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        int i;
        (ServicePointManager.SecurityProtocol, i) = ((SecurityProtocolType)384, 384);
    }
}");
            // Ideally we'd handle the IDeconstructionAssignment, but this code pattern seems unlikely.
        }

        [Fact]
        public void TestUse24Plus24SimpleAssignmentDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)(24 + 24);
    }
}",
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "48"));
        }

        [Fact]
        public void TestUse768NotSecurityProtocolTypeNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Net;

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
            return new DoNotUseDeprecatedSecurityProtocols();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseDeprecatedSecurityProtocols();
        }
    }
}
