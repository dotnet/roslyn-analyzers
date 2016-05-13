// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OverrideGetHashCodeOnOverridingEqualsFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicOverrideGetHashCodeOnOverridingEqualsAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            // Diagnostic is from the compiler.
            return null;
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicOverrideGetHashCodeOnOverridingEqualsFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpOverrideGetHashCodeOnOverridingEqualsFixer();
        }

        [Fact]
        public void CS0659()
        {
            VerifyCSharpFix(@"
class C
{
    public override bool Equals(object obj) => true;
}
",
                @"
class C
{
    public override bool Equals(object obj) => true;

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}
",
                codeFixIndex: null,
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CS0659_Simplified()
        {
            VerifyCSharpFix(@"
using System;

class C
{
    public override bool Equals(object obj) => true;
}
",
                @"
using System;

class C
{
    public override bool Equals(object obj) => true;

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
",
                codeFixIndex: null,
                allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void Basic_CA2218()
        {
            VerifyBasicFix(@"
Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function
End Class
",
@"
Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function

    Public Overrides Function GetHashCode() As Integer
        Throw New System.NotImplementedException()
    End Function
End Class
");
        }

        [Fact]
        public void Basic_CA2218_Simplified()
        {
            VerifyBasicFix(@"
Imports System

Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function
End Class
",
@"
Imports System

Class C
    Public Overrides Function Equals(o As Object) As Boolean
        Return True
    End Function

    Public Overrides Function GetHashCode() As Integer
        Throw New NotImplementedException()
    End Function
End Class
");
        }
    }
}