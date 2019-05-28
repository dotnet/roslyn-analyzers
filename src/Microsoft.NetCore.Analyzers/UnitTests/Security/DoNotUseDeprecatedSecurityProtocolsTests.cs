// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseDeprecatedSecurityProtocolsTests : DiagnosticAnalyzerTestBase
    {
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
            GetCSharpResultAt(9, 17, DoNotUseDeprecatedSecurityProtocols.Rule, "Ssl3"));
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
            GetCSharpResultAt(9, 17, DoNotUseDeprecatedSecurityProtocols.Rule, "Tls"));
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
            GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.Rule, "Tls11"));
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
        public void TestUseTls12NoDiagnostic()
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
}");
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
                GetCSharpResultAt(9, 77, DoNotUseDeprecatedSecurityProtocols.Rule, "Tls11"));
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
                GetCSharpResultAt(9, 49, DoNotUseDeprecatedSecurityProtocols.Rule, "192"));
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
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.Rule, "384"));
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
                GetCSharpResultAt(9, 87, DoNotUseDeprecatedSecurityProtocols.Rule, "768"));
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
                GetCSharpResultAt(9, 48, DoNotUseDeprecatedSecurityProtocols.Rule, "48"));
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
