// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseDeprecatedSecurityProtocols,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.DoNotUseDeprecatedSecurityProtocols,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

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

    public class DoNotUseDeprecatedSecurityProtocolsTests
    {
        [Fact]
        public async Task DocSample1_CSharp_Violation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task DocSample1_VB_Violation()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task DocSample2_CSharp_Violation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task DocSample2_VB_Violation()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task DocSample1_CSharp_Solution()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task DocSample1_VB_Solution()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task DocSample3_CSharp_Violation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task DocSample3_VB_Violation()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task DocSample4_CSharp_Violation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task DocSample4_VB_Violation()
        {
            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task TestUseSsl3Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseTlsDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseTls11Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseSystemDefaultNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseTls12Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseTls13Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseTls12OrdTls11Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUse192CompoundAssignmentDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUse384SimpleAssignmentDiagnostic()
        {
            // 384 = SchProtocols.Tls11Server | SchProtocols.Tls10Client
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUse768SimpleAssignmentOrExpressionDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUse12288SimpleAssignmentOrExpressionDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseTls12OrTls11Or192Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUseTls12Or192Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUse768DeconstructionAssignmentNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUse24Plus24SimpleAssignmentDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task TestUse768NotSecurityProtocolTypeNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        [Fact]
        public async Task TestMaskOutUnsafeOnServicePointManagerNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        ServicePointManager.SecurityProtocol &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11);
    }
}");
        }

        [Fact]
        public async Task TestMaskOutUnsafeOnVariableDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Net;

class TestClass
{
    public void TestMethod()
    {
        SecurityProtocolType t = default(SecurityProtocolType);
        t &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11);
    }
}",
                GetCSharpResultAt(10, 14, DoNotUseDeprecatedSecurityProtocols.HardCodedRule, "-1009"),
                GetCSharpResultAt(10, 16, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Ssl3"),
                GetCSharpResultAt(10, 44, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls"),
                GetCSharpResultAt(10, 71, DoNotUseDeprecatedSecurityProtocols.DeprecatedRule, "Tls11"));
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule, params string[] arguments)
           => VerifyCS.Diagnostic(rule)
               .WithLocation(line, column)
               .WithArguments(arguments);

        private DiagnosticResult GetBasicResultAt(int line, int column, DiagnosticDescriptor rule, params string[] arguments)
           => VerifyVB.Diagnostic(rule)
               .WithLocation(line, column)
               .WithArguments(arguments);
    }
}
