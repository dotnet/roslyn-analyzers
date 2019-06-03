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
    public class DoNotDisableSchUseStrongCryptoTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestBoolDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        AppContext.SetSwitch(""Switch.System.Net.DontEnableSchUseStrongCrypto"", true);
    }
}",
            GetCSharpResultAt(8, 9, DoNotSetSwitch.DoNotDisableSchUseStrongCryptoRule, "SetSwitch"));
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
        AppContext.SetSwitch(""Switch.System.Net.DontEnableSchUseStrongCrypto"", 1 + 2 == 3);
    }
}",
            GetCSharpResultAt(8, 9, DoNotSetSwitch.DoNotDisableSchUseStrongCryptoRule, "SetSwitch"));
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
        AppContext.SetSwitch(""Switch.System.Net.DontEnableSchUseStrongCrypto"", 1 == 1 ? true : false);
    }
}",
            GetCSharpResultAt(8, 9, DoNotSetSwitch.DoNotDisableSchUseStrongCryptoRule, "SetSwitch"));
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
        const string constSwitchName = ""Switch.System.Net.DontEnableSchUseStrongCrypto"";
        AppContext.SetSwitch(constSwitchName, true);
    }
}",
            GetCSharpResultAt(9, 9, DoNotSetSwitch.DoNotDisableSchUseStrongCryptoRule, "SetSwitch"));
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
        AppContext.SetSwitch(""Switch.System.Net.DontEnableSchUseStrongCrypto"", false);
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
        AppContext.SetSwitch(""Switch.System.Net.DontEnableSchUseStrongCrypto"", 1 + 2 != 3);
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
        AppContext.SetSwitch(""Switch.System.Net.DontEnableSchUseStrongCrypto"", 1 == 1 ? false : true);
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

        //Ideally, we would generate a diagnostic in this case.
        [Fact]
        public void TestSwitchNameVariableNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        string switchName = ""Switch.System.Net.DontEnableSchUseStrongCrypto"";
        AppContext.SetSwitch(switchName, true);
    }
}");
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
        AppContext.SetSwitch(""Switch.System.Net.DontEnableSchUseStrongCrypto"", bool.Parse(""true""));
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
