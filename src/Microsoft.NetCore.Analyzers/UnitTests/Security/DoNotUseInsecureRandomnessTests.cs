// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class DoNotUseInsecureRandomnessTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void Test_UsingMethodNext_OfRandom_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod(Random random)
    {
        var sensitiveVariable = random.Next();
    }
}",
            GetCSharpResultAt(8, 33, DoNotUseInsecureRandomness.Rule, "Random"));

            VerifyBasic(@"
Imports System

class TestClass
    public Sub TestMethod(random As Random)
        Dim sensitiveVariable As Integer
        sensitiveVariable = random.Next()
    End Sub
End Class",
            GetBasicResultAt(7, 29, DoNotUseInsecureRandomness.Rule, "Random"));
        }

        [Fact]
        public void Test_UsingMethodNextDouble_OfRandom_Diagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod(Random random)
    {
        var sensitiveVariable = random.NextDouble();
    }
}",
            GetCSharpResultAt(8, 33, DoNotUseInsecureRandomness.Rule, "Random"));

            VerifyBasic(@"
Imports System

class TestClass
    public Sub TestMethod(random As Random)
        Dim sensitiveVariable As Integer
        sensitiveVariable = random.NextDouble()
    End Sub
End Class",
            GetBasicResultAt(7, 29, DoNotUseInsecureRandomness.Rule, "Random"));
        }

        [Fact]
        public void Test_UsingMethodGetHashCode_OfObject_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod(Random random)
    {
        var hashCode = random.GetHashCode();
    }
}");

            VerifyBasic(@"
Imports System

class TestClass
    public Sub TestMethod(random As Random)
        Dim hashCode As Integer
        hashCode = random.GetHashCode()
    End Sub
End Class");
        }

        [Fact]
        public void Test_UsingConstructor_OfRandom_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public void TestMethod()
    {
        var random = new Random();
    }
}");

            VerifyBasic(@"
Imports System

class TestClass
    public Sub TestMethod
        Dim random As New Random
    End Sub
End Class");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureRandomness();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseInsecureRandomness();
        }
    }
}
