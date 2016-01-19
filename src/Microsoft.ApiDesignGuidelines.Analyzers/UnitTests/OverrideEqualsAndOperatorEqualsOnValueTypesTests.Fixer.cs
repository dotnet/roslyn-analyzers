// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Xunit;

namespace Microsoft.ApiDesignGuidelines.Analyzers.UnitTests
{
    public class OverrideEqualsAndOperatorEqualsOnValueTypesFixerTests : CodeFixTestBase
    {
        [Fact]
        public void CSharpCodeFixNoEqualsOverrideOrEqualityOperators()
        {
            VerifyCSharpFix(@"
public struct A
{
}
",

@"
public struct A
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public static bool operator ==(A left, A right)
    {
        throw new System.NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new System.NotImplementedException();
    }
}
",
            // This fix introduces the compiler warnings:
            // Test0.cs(2, 15): warning CS0659: 'S' overrides Object.Equals(object o) but does not override Object.GetHashCode()
            // Test0.cs(2,15): warning CS0661: 'S' defines operator == or operator != but does not override Object.GetHashCode()
            allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CSharpCodeFixNoEqualsOverride()
        {
            VerifyCSharpFix(@"
public struct A
{
    public static bool operator ==(A left, A right)
    {
        throw new System.NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new System.NotImplementedException();
    }
}
",

@"
public struct A
{
    public static bool operator ==(A left, A right)
    {
        throw new System.NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new System.NotImplementedException();
    }

    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}
",
            allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CSharpCodeFixNoEqualityOperator()
        {
            VerifyCSharpFix(@"
public struct A
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new System.NotImplementedException();
    }
}
",

@"
public struct A
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new System.NotImplementedException();
    }

    public static bool operator ==(A left, A right)
    {
        throw new System.NotImplementedException();
    }
}
",
            allowNewCompilerDiagnostics: true);
        }

        [Fact]
        public void CSharpCodeFixNoInequalityOperator()
        {
            VerifyCSharpFix(@"
public struct A
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public static bool operator ==(A left, A right)
    {
        throw new System.NotImplementedException();
    }
}
",

@"
public struct A
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public static bool operator ==(A left, A right)
    {
        throw new System.NotImplementedException();
    }

    public static bool operator !=(A left, A right)
    {
        throw new System.NotImplementedException();
    }
}
",
            allowNewCompilerDiagnostics: true);
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new OverrideEqualsAndOperatorEqualsOnValueTypesAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicOverrideEqualsAndOperatorEqualsOnValueTypesFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpOverrideEqualsAndOperatorEqualsOnValueTypesFixer();
        }
    }
}