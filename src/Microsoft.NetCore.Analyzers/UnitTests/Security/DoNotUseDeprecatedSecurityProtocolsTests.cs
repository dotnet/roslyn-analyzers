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
