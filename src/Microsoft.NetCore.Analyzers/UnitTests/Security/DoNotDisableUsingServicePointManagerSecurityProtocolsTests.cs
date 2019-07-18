// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotDisableUsingServicePointManagerSecurityProtocolsTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void DocSample1_CSharp_Violation()
        {
            VerifyCSharp(@"
using System;

public class ExampleClass
{
    public void ExampleMethod()
    {
        // CA5378 violation
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", true);
    }
}",
            GetCSharpResultAt(9, 9, DoNotSetSwitch.DoNotDisableSpmSecurityProtocolsRule, "SetSwitch"));
        }

        [Fact]
        public void DocSample1_VB_Violation()
        {
            VerifyBasic(@"
Imports System

Public Class ExampleClass
    Public Sub ExampleMethod()
        ' CA5378 violation
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", true)
    End Sub
End Class",
            GetBasicResultAt(7, 9, DoNotSetSwitch.DoNotDisableSpmSecurityProtocolsRule, "SetSwitch"));
        }

        [Fact]
        public void DocSample1_CSharp_Solution()
        {
            VerifyCSharp(@"
using System;

public class ExampleClass
{
    public void ExampleMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", false);
    }
}");
        }

        [Fact]
        public void DocSample1_VB_Solution()
        {
            VerifyBasic(@"
Imports System

Public Class ExampleClass
    Public Sub ExampleMethod()
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", false)
    End Sub
End Class");
        }

        [Fact]
        public void TestBoolDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", true);
    }
}",
            GetCSharpResultAt(8, 9, DoNotSetSwitch.DoNotDisableSpmSecurityProtocolsRule, "SetSwitch"));
        }

        [Fact]
        public void TestEquationDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", 1 + 2 == 3);
    }
}",
            GetCSharpResultAt(8, 9, DoNotSetSwitch.DoNotDisableSpmSecurityProtocolsRule, "SetSwitch"));
        }

        [Fact]
        public void TestConditionalOperatorDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", 1 == 1 ? true : false);
    }
}",
            GetCSharpResultAt(8, 9, DoNotSetSwitch.DoNotDisableSpmSecurityProtocolsRule, "SetSwitch"));
        }

        [Fact]
        public void TestWithConstantSwitchNameDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        const string constSwitchName = ""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"";
        AppContext.SetSwitch(constSwitchName, true);
    }
}",
            GetCSharpResultAt(9, 9, DoNotSetSwitch.DoNotDisableSpmSecurityProtocolsRule, "SetSwitch"));
        }

        [Fact]
        public void TestBoolNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", false);
    }
}");
        }

        [Fact]
        public void TestEquationNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", 1 + 2 != 3);
    }
}");
        }

        [Fact]
        public void TestConditionalOperatorNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", 1 == 1 ? false : true);
    }
}");
        }

        [Fact]
        public void TestSwitchNameNullNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(null, true);
    }
}");
        }

        [Fact]
        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.ValueContentAnalysis)]
        public void TestSwitchNameVariableNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        string switchName = ""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"";
        AppContext.SetSwitch(switchName, true);
    }
}",
                GetCSharpResultAt(9, 9, DoNotSetSwitch.DoNotDisableSpmSecurityProtocolsRule, "SetSwitch"));
        }

        //Ideally, we would generate a diagnostic in this case.
        [Fact]
        public void TestBoolParseNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols"", bool.Parse(""true""));
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotSetSwitch();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotSetSwitch();
        }
    }
}
