// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.CSharp.Analyzers;
using System.Runtime.VisualBasic.Analyzers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class TestForNaNCorrectlyFixerTests : CodeFixTestBase
    {
        [Fact]
        public void CA2242_FixFloatForEqualityWithFloatNaN()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(float f)
    {
        return f == float.NaN;
    }
}
", @"
public class A
{
    public bool Compare(float f)
    {
        return float.IsNaN(f);
    }
}
");

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As Single) As Boolean
        Return s = Single.NaN
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As Single) As Boolean
        Return Single.IsNaN(s)
    End Function
End Class
");
        }

        [Fact]
        public void CA2242_FixFloatForInequalityWithFloatNaN()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(float f)
    {
        return f != float.NaN;
    }
}
", @"
public class A
{
    public bool Compare(float f)
    {
        return !float.IsNaN(f);
    }
}
");

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As Single) As Boolean
        Return s <> Single.NaN
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As Single) As Boolean
        Return Not Single.IsNaN(s)
    End Function
End Class
");
        }

        public void CA2242_FixDoubleForEqualityWithDoubleNaN()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(double d)
    {
        return d == double.NaN;
    }
}
", @"
public class A
{
    public bool Compare(double d)
    {
        return double.IsNaN(d);
    }
}
");

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(d As Double) As Boolean
        Return d = Double.NaN
    End Function
End Class
", @"
Public Class A
    Public Function Compare(d As Double) As Boolean
        Return Double.IsNaN(d)
    End Function
End Class
");
        }

        [Fact]
        public void CA2242_FixDoubleForInequalityWithDoubleNaN()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(double d)
    {
        return d != double.NaN;
    }
}
", @"
public class A
{
    public bool Compare(double d)
    {
        return !double.IsNaN(d);
    }
}
");

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(d As Double) As Boolean
        Return d <> Double.NaN
    End Function
End Class
", @"
Public Class A
    Public Function Compare(d As Double) As Boolean
        Return Not Double.IsNaN(d)
    End Function
End Class
");
        }

        [Fact]
        public void CA2242_FixForComparisonWithNaNOnLeft()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare(double d)
    {
        return double.NaN == d;
    }
}
", @"
public class A
{
    public bool Compare(double d)
    {
        return double.IsNaN(d);
    }
}
");

            VerifyBasicFix(@"
Public Class A
    Public Function Compare(s As Single) As Boolean
        Return Single.NaN = s
    End Function
End Class
", @"
Public Class A
    Public Function Compare(s As Single) As Boolean
        Return Single.IsNaN(s
)
    End Function
End Class
");
        }

        [Fact]
        public void CA2242_FixOnlyOneDiagnosticForComparisonWithNaNOnBothSides()
        {
            VerifyCSharpFix(@"
public class A
{
    public bool Compare()
    {
        return float.NaN == float.NaN;
    }
}
", @"
public class A
{
    public bool Compare()
    {
        return float.IsNaN(float.NaN);
    }
}
");

            VerifyBasicFix(@"
Public Class A
    Public Function Compare() As Boolean
        Return Double.NaN = Double.NaN
    End Function
End Class
", @"
Public Class A
    Public Function Compare() As Boolean
        Return Double.IsNaN(Double.NaN
)
    End Function
End Class
");
        }

        [Fact]
        public void CA2242_FixForComparisonWithNaNInFunctionArgument()
        {
            VerifyCSharpFix(@"
public class A
{
    float _n = 42.0F;

    public void F()
    {
        G(_n == float.NaN);
    }

    public void G(bool comparison) {}
}
", @"
public class A
{
    float _n = 42.0F;

    public void F()
    {
        G(float.IsNaN(_n));
    }

    public void G(bool comparison) {}
}
");

            VerifyBasicFix(@"
Public Class A
    Private _n As Single = 42.0F

    Public Sub F()
        G(_n = Single.NaN)
    End Sub

    Public Sub G(comparison As Boolean)
    End Sub
End Class
", @"
Public Class A
    Private _n As Single = 42.0F

    Public Sub F()
        G(Single.IsNaN(_n))
    End Sub

    Public Sub G(comparison As Boolean)
    End Sub
End Class
");
        }
        
        [Fact]
        public void CA2242_FixForComparisonWithNaNInTernaryOperator()
        {
            VerifyCSharpFix(@"
public class A
{
    float _n = 42.0F;

    public int F()
    {
        return _n == float.NaN ? 1 : 0;
    }
}
", @"
public class A
{
    float _n = 42.0F;

    public int F()
    {
        return float.IsNaN(_n) ? 1 : 0;
    }
}
");

            // VB doesn't have the ternary operator, but we add this test for symmetry.
            VerifyBasicFix(@"
Public Class A
    Private _n As Single = 42.0F

    Public Function F() As Integer
        Return If(_n = Single.NaN, 1, 0)
    End Function
End Class
", @"
Public Class A
    Private _n As Single = 42.0F

    Public Function F() As Integer
        Return If(Single.IsNaN(_n), 1, 0)
    End Function
End Class
");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new TestForNaNCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TestForNaNCorrectlyAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new BasicTestForNaNCorrectlyFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CSharpTestForNaNCorrectlyFixer();
        }
    }
}