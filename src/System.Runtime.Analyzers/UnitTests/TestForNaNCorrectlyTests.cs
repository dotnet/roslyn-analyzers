// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace System.Runtime.Analyzers.UnitTests
{
    public class TestForNaNCorrectlyTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void CSharpDiagnosticForEqualityWithFloatNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f == float.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForEqualityWithFloatNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f = Single.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpDiagnosticForInequalityWithFloatNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f != float.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForInEqualityWithFloatNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f <> Single.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpDiagnosticForGreaterThanFloatNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f > float.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForGreaterThanFloatNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f > Single.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpDiagnosticForGreaterThanOrEqualToFloatNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f >= float.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForGreaterThanOrEqualToFloatNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f >= Single.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpDiagnosticForLessThanFloatNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f < float.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForLessThanFloatNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f < Single.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpDiagnosticForLessThanOrEqualToFloatNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f <= float.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForLessThanOrEqualToFloatNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f <= Single.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpDiagnosticForComparisonWithDoubleNaN()
        {
            var code = @"
public class A
{
    public bool Compare(double d)
    {
        return d == double.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForComparisonWithDoubleNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(d As Double) As Boolean
        Return d < Double.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpDiagnosticForComparisonWithNaNOnLeft()
        {
            var code = @"
public class A
{
    public bool Compare(double d)
    {
        return double.NaN == d;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicDiagnosticForComparisonWithNaNOnLeft()
        {
            var code = @"
Public Class A
    Public Function Compare(d As Double) As Boolean
        Return Double.NaN = d
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        [Fact]
        public void CSharpNoDiagnosticForComparisonWithBadExpression()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f == float.NbN; // Misspelled.
    }
}
";
            VerifyCSharp(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void BasicNoDiagnosticForComparisonWithBadExpression()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f = Single.NbN   ' Misspelled
    End Function
End Class
";
            VerifyBasic(code, TestValidationMode.AllowCompileErrors);
        }

        [Fact]
        public void CSharpNoDiagnosticForComparisonWithFunctionReturningNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f == NaNFunc();
    }

    private float NaNFunc()
    {
        return float.NaN;
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void BasicNoDiagnosticForComparisonWithFunctionReturningNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f = NaNFunc()
    End Function

    Private Function NaNFunc() As Single
        Return Single.NaN
    End Function
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void CSharpNoDiagnosticForEqualityWithNonNaN()
        {
            var code = @"
public class A
{
    public bool Compare(float f)
    {
        return f == 1.0;
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void BasicNoDiagnosticForEqualityWithNonNaN()
        {
            var code = @"
Public Class A
    Public Function Compare(f As Single) As Boolean
        Return f = 1.0
    End Function
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void CSharpNoDiagnosticForNonComparisonOperationWithNaN()
        {
            var code = @"
public class A
{
    public float OperateOn(float f)
    {
        return f + float.NaN;
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void BasicNoDiagnosticForNonComparisonOperationWithNonNaN()
        {
            var code = @"
Public Class A
    Public Function OperateOn(f As Single) As Single
        Return f + Single.NaN
    End Function
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void CSharpOnlyOneDiagnosticForComparisonWithNaNOnBothSides()
        {
            var code = @"
public class A
{
    public bool Compare()
    {
        return float.NaN == float.NaN;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(6, 16));
        }

        [Fact]
        public void BasicOnlyOneDiagnosticForComparisonWithNonNaNOnBothSides()
        {
            var code = @"
Public Class A
    Public Function Compare() As Boolean
        Return Single.NaN = Single.NaN
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(4, 16));
        }

        // At @srivatsn's suggestion, here are a few tests that verify that the operation
        // tree is correct when the comparison occurs in syntactic constructs other than
        // a function return value. Of course we can't be exhaustive about this, and these
        // tests are really more about the correctness of the operation tree -- ensuring
        // that "binary operator expressions" are present in places we expect them to be --
        // than they are about the correctness of our treatment of these expressions once
        // we find them.
        [Fact]
        public void CSharpDiagnosticForComparisonWithNaNInFunctionArgument()
        {
            var code = @"
public class A
{
    float _n = 42.0F;

    public void F()
    {
        G(_n == float.NaN);
    }

    public void G(bool comparison) {}
}
";
            VerifyCSharp(code, GetCSharpResultAt(8, 11));
        }

        [Fact]
        public void BasicDiagnosticForComparisonWithNaNInFunctionArgument()
        {
            var code = @"
Public Class A
    Private _n As Single = 42.0F

    Public Sub F()
        G(_n = Single.NaN)
    End Sub

    Public Sub G(comparison As Boolean)
    End Sub
End Class
";
            VerifyBasic(code, GetBasicResultAt(6, 11));
        }
        [Fact]
        public void CSharpDiagnosticForComparisonWithNaNInTernaryOperator()
        {
            var code = @"
public class A
{
    float _n = 42.0F;

    public int F()
    {
        return _n == float.NaN ? 1 : 0;
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(8, 16));
        }

        [Fact]
        public void BasicDiagnosticForComparisonWithNaNInIfOperator()
        {
            // VB doesn't have the ternary operator, but we add this test for symmetry.
            var code = @"
Public Class A
    Private _n As Single = 42.0F

    Public Function F() As Integer
        Return If(_n = Single.NaN, 1, 0)
    End Function
End Class
";
            VerifyBasic(code, GetBasicResultAt(6, 19));
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new TestForNaNCorrectlyAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new TestForNaNCorrectlyAnalyzer();
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, TestForNaNCorrectlyAnalyzer.Rule);
        }

        private DiagnosticResult GetBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, TestForNaNCorrectlyAnalyzer.Rule);
        }
    }
}