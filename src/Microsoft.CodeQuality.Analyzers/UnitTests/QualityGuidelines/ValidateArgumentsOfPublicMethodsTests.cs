// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.ParameterValidationAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.CopyAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.NullAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PointsToAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PredicateAnalysis)]
    public partial class ValidateArgumentsOfPublicMethodsTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer() => new ValidateArgumentsOfPublicMethods();
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ValidateArgumentsOfPublicMethods();

        private new DiagnosticResult GetCSharpResultAt(int line, int column, string methodSignature, string parameterName) =>
            GetCSharpResultAt(line, column, ValidateArgumentsOfPublicMethods.Rule, methodSignature, parameterName);

        private new DiagnosticResult GetBasicResultAt(int line, int column, string methodSignature, string parameterName) =>
            GetBasicResultAt(line, column, ValidateArgumentsOfPublicMethods.Rule, methodSignature, parameterName);

        [Fact]
        public void ValueTypeParameter_NoDiagnostic()
        {
            VerifyCSharp(@"
public struct C
{
    public int X;
}

public class Test
{
    public int M1(C c)
    {
        return c.X;
    }
}
");

            VerifyBasic(@"
Public Structure C
    Public X As Integer
End Structure

Public Class Test
    Public Function M1(c As C) As Integer
        Return c.X
    End Function
End Class");
        }

        [Fact]
        public void ReferenceTypeParameter_NoUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    public void M1(string str)
    {
    }
}
");

            VerifyBasic(@"
Public Class Test
    Public Sub M1(str As String)
    End Sub
End Class");
        }

        [Fact]
        public void ReferenceTypeParameter_NoHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    public void M1(string str)
    {
        var x = str;
        M2(str);
    }

    private void M2(string str)
    {
    }
}
");

            VerifyBasic(@"
Public Class Test
    Public Sub M1(str As String)
        Dim x = str
        M2(str)
    End Sub

    Private Sub M2(str As String)
    End Sub
End Class");
        }

        [Fact]
        public void NonExternallyVisibleMethod_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    private int M1(C c)
    {
        return c.X;
    }

    internal int M2(C c)
    {
        return c.X;
    }
}

internal class Test2
{
    public int M1(C c)
    {
        return c.X;
    }

    protected int M2(C c)
    {
        return c.X;
    }

    internal int M3(C c)
    {
        return c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Private Function M1(c As C) As Integer
        Return c.X
    End Function

    Friend Function M2(c As C) As Integer
        Return c.X
    End Function
End Class

Friend Class Test2
    Public Function M1(c As C) As Integer
        Return c.X
    End Function

    Protected Function M2(c As C) As Integer
        Return c.X
    End Function

    Friend Function M3(c As C) As Integer
        Return c.X
    End Function
End Class");
        }

        [Fact]
        public void HazardousUsage_MethodReference_Diagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    public void M1(string str)
    {
        var x = str.ToString();
    }
}
",
            // Test0.cs(6,17): warning CA1062: In externally visible method 'void Test.M1(string str)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(6, 17, "void Test.M1(string str)", "str"));

            VerifyBasic(@"
Public Class Test
    Public Sub M1(str As String)
        Dim x = str.ToString()
    End Sub
End Class
",
            // Test0.vb(4,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(4, 17, "Sub Test.M1(str As String)", "str"));
        }

        [Fact]
        public void HazardousUsage_FieldReference_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        var x = c.X;
    }
}
",
            // Test0.cs(11,17): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(11, 17, "void Test.M1(C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim x = c.X
    End Sub
End Class
",
            // Test0.vb(8,17): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(8, 17, "Sub Test.M1(c As C)", "c"));
        }

        [Fact]
        public void HazardousUsage_PropertyReference_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X { get; }
}

public class Test
{
    public void M1(C c)
    {
        var x = c.X;
    }
}
",
            // Test0.cs(11,17): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(11, 17, "void Test.M1(C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public ReadOnly Property X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim x = c.X
    End Sub
End Class
",
            // Test0.vb(8,17): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(8, 17, "Sub Test.M1(c As C)", "c"));
        }

        [Fact]
        public void HazardousUsage_EventReference_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public delegate void MyDelegate();
    public event MyDelegate X;
}

public class Test
{
    public void M1(C c)
    {
        c.X += MyHandler;
    }

    private void MyHandler()
    {
    }
}
",
            // Test0.cs(12,9): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(12, 9, "void Test.M1(C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public Event X()
End Class

Public Class Test
    Public Sub M1(c As C)
        AddHandler c.X, AddressOf MyHandler
    End Sub

    Private Sub MyHandler()
    End Sub
End Class
",
            // Test0.vb(8,20): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(8, 20, "Sub Test.M1(c As C)", "c"));
        }

        [Fact]
        public void HazardousUsage_ArrayElementReference_Diagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    public void M1(Test[] tArray)
    {
        var x = tArray[0];
    }
}
",
            // Test0.cs(6,17): warning CA1062: In externally visible method 'void Test.M1(Test[] tArray)', validate parameter 'tArray' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(6, 17, "void Test.M1(Test[] tArray)", "tArray"));

            VerifyBasic(@"
Public Class Test
    Public Sub M1(tArray As Test())
        Dim x = tArray(0)
    End Sub
End Class
",
            // Test0.vb(4,17): warning CA1062: In externally visible method 'Sub Test.M1(tArray As Test())', validate parameter 'tArray' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(4, 17, "Sub Test.M1(tArray As Test())", "tArray"));
        }

        [Fact]
        public void MultipleHazardousUsages_OneReportPerParameter_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
    public int Y;    
}

public class Test
{
    public void M1(C c1, C c2)
    {
        var x = c1.X;   // Diagnostic
        var y = c1.Y;
        var x2 = c1.X;

        var x3 = c2.X;   // Diagnostic
        var y2 = c2.Y;
        var x4 = c2.X;
    }
}
",
        // Test0.cs(12,17): warning CA1062: In externally visible method 'void Test.M1(C c1, C c2)', validate parameter 'c1' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
        GetCSharpResultAt(12, 17, "void Test.M1(C c1, C c2)", "c1"),
        // Test0.cs(16,18): warning CA1062: In externally visible method 'void Test.M1(C c1, C c2)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
        GetCSharpResultAt(16, 18, "void Test.M1(C c1, C c2)", "c2"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
    Public Y As Integer
End Class

Public Class Test

    Public Sub M1(c1 As C, c2 As C)
        Dim x = c1.X    ' Diagnostic
        Dim y = c1.Y
        Dim x2 = c1.X

        Dim x3 = c2.X    ' Diagnostic
        Dim y2 = c2.Y
        Dim x4 = c2.X
    End Sub
End Class
",
            // Test0.vb(10,17): warning CA1062: In externally visible method 'Sub Test.M1(c1 As C, c2 As C)', validate parameter 'c1' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(10, 17, "Sub Test.M1(c1 As C, c2 As C)", "c1"),
            // Test0.vb(14,18): warning CA1062: In externally visible method 'Sub Test.M1(c1 As C, c2 As C)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(14, 18, "Sub Test.M1(c1 As C, c2 As C)", "c2"));
        }

        [Fact]
        public void HazardousUsage_OptionalParameter_Diagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    private const string _constStr = """";
    public void M1(string str = _constStr)
    {
        var x = str.ToString();
    }
}
",
            // Test0.cs(7,17): warning CA1062: In externally visible method 'void Test.M1(string str = "")', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(7, 17, @"void Test.M1(string str = """")", "str"));

            VerifyBasic(@"
Public Class Test
    Private Const _constStr As String = """"
    Public Sub M1(Optional str As String = _constStr)
        Dim x = str.ToString()
    End Sub
End Class
",
            // Test0.vb(5,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String = "")', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(5, 17, @"Sub Test.M1(str As String = """")", "str"));
        }

        [Fact]
        public void ConditionalAccessUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        var y = x?.ToString();
        
        var z = c?.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str
        Dim y = x?.ToString()

        Dim z = c?.X
    End Sub
End Class");
        }

        [Fact]
        public void ValidatedNonNullAttribute_PossibleNullRefUsage_NoDiagnostic()
        {
            VerifyCSharp(@"
public class ValidatedNotNullAttribute : System.Attribute
{
}

public class Test
{
    public void M1([ValidatedNotNullAttribute]string str)
    {
        var x = str.ToString();
    }
}
");

            VerifyBasic(@"
Public Class ValidatedNotNullAttribute
    Inherits System.Attribute
End Class

Public Class Test
    Public Sub M1(<ValidatedNotNullAttribute>str As String)
        Dim x = str.ToString()
    End Sub
End Class
");
        }

        [Fact]
        public void ValidatedNonNullAttribute_PossibleNullRefUsageOnDifferentParam_Diagnostic()
        {
            VerifyCSharp(@"
public class ValidatedNotNullAttribute : System.Attribute
{
}

public class Test
{
    public void M1([ValidatedNotNullAttribute]string str, string str2)
    {
        var x = str.ToString() + str2.ToString();
    }
}
",
            // Test0.cs(10,34): warning CA1062: In externally visible method 'void Test.M1(string str, string str2)', validate parameter 'str2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(10, 34, "void Test.M1(string str, string str2)", "str2"));

            VerifyBasic(@"
Public Class ValidatedNotNullAttribute
    Inherits System.Attribute
End Class

Public Class Test
    Public Sub M1(<ValidatedNotNullAttribute>str As String, str2 As String)
        Dim x = str.ToString() + str2.ToString()
    End Sub
End Class
",
            // Test0.vb(8,34): warning CA1062: In externally visible method 'Sub Test.M1(str As String, str2 As String)', validate parameter 'str2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(8, 34, "Sub Test.M1(str As String, str2 As String)", "str2"));
        }

        [Fact]
        public void DefiniteSimpleAssignment_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        x = ""newString"";
        var y = x.ToString();

        c = new C();
        var z = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str
        x = ""newString""
        Dim y = x.ToString()

        c = New C()
        Dim z = c.X
    End Sub
End Class");
        }

        [Fact]
        public void AssignedToFieldAndValidated_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    private C _c;
    private Test _t;
    public void M1(C c)
    {
        _c = c;
        _t._c = c;
        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }

        var z = _c.X + _t._c.X + c.X;
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Private _c As C
    Private _t As Test
    Public Sub M1(c As C)
        _c = c
        _t._c = c
        If c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If

        Dim z = _c.X + _t._c.X + c.X
    End Sub
End Class");
        }

        [Fact]
        public void AssignedToFieldAndNotValidated_BeforeHazardousUsages_Diagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    private C _c;
    private Test _t;
    public void M1(C c)
    {
        _t._c = c;
        var z = _t._c.X;
        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }
    }

    public void M2(C c)
    {
        _c = c;
        _t._c = c;
        var z = _c.X + _t._c.X;
        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }
    }
}
",
            // Test0.cs(16,17): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(16, 17, "void Test.M1(C c)", "c"),
            // Test0.cs(27,17): warning CA1062: In externally visible method 'void Test.M2(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(27, 17, "void Test.M2(C c)", "c"));

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Private _c As C
    Private _t As Test
    Public Sub M1(c As C)
        _t._c = c
        Dim z = _t._c.X
        If c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If
    End Sub

    Public Sub M2(c As C)
        _c = c
        _t._c = c
        Dim z = _c.X + _t._c.X
        If c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If
    End Sub
End Class",
            // Test0.vb(13,17): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(13, 17, "Sub Test.M1(c As C)", "c"),
            // Test0.vb(22,17): warning CA1062: In externally visible method 'Sub Test.M2(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(22, 17, "Sub Test.M2(c As C)", "c"));
        }

        [Fact]
        public void MayBeAssigned_BeforeHazardousUsages_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c, bool flag)
    {
        var x = str;
        if (flag)
        {
            x = ""newString"";
            c = new C();
        }

        // Below may or may not cause null refs
        var y = x.ToString();
        var z = c.X;
    }
}
",
            // Test0.cs(19,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c, bool flag)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(19, 17, "void Test.M1(string str, C c, bool flag)", "str"),
            // Test0.cs(20,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c, bool flag)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(20, 17, "void Test.M1(string str, C c, bool flag)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C, flag As Boolean)
        Dim x = str

        If flag Then
            x = ""newString""
            c = New C()
        End If
        
        ' Below may or may not cause null refs
        Dim y = x.ToString()
        Dim z = c.X
    End Sub
End Class",
            // Test0.vb(16,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C, flag As Boolean)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(16, 17, "Sub Test.M1(str As String, c As C, flag As Boolean)", "str"),
            // Test0.vb(17,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C, flag As Boolean)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(17, 17, "Sub Test.M1(str As String, c As C, flag As Boolean)", "c"));
        }

        [Fact]
        public void ConditionalButDefiniteNonNullAssigned_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c, bool flag)
    {
        var x = str;
        if (str == null || c == null)
        {
            x = ""newString"";
            c = new C();
        }

        // x and c are both non-null here.
        var y = x.ToString();
        var z = c.X;
    }

    public void M2(string str, C c, bool flag)
    {
        var x = str;
        if (str == null)
        {
            x = ""newString"";
        }

        if (c == null)
        {
            c = new C();
        }

        // x and c are both non-null here.
        var y = x.ToString();
        var z = c.X;
    }

    public void M3(string str, C c, bool flag)
    {
        var x = str ?? ""newString"";
        c = c ?? new C();

        // x and c are both non-null here.
        var y = x.ToString();
        var z = c.X;
    }

    public void M4(string str, C c, bool flag)
    {
        var x = str != null ? str : ""newString"";
        c = c != null ? c : new C();

        // x and c are both non-null here.
        var y = x.ToString();
        var z = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(ByVal str As String, ByVal c As C, ByVal flag As Boolean)
        Dim x = str
        If str Is Nothing OrElse c Is Nothing Then
            x = ""newString""
            c = New C()
        End If

        ' x and c are both non-null here.
        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Public Sub M2(ByVal str As String, ByVal c As C, ByVal flag As Boolean)
        Dim x = str
        If str Is Nothing Then
            x = ""newString""
        End If

        If c Is Nothing Then
            c = New C()
        End If

        ' x and c are both non-null here.
        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Public Sub M3(ByVal str As String, ByVal c As C, ByVal flag As Boolean)
        Dim x = If(str, ""newString"")
        c = If(c, New C())

        ' x and c are both non-null here.
        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Public Sub M4(ByVal str As String, ByVal c As C, ByVal flag As Boolean)
        Dim x = If(str IsNot Nothing, str, ""newString"")
        c = If(c IsNot Nothing, c, New C())

        ' x and c are both non-null here.
        Dim y = x.ToString()
        Dim z = c.X
    End Sub

End Class");
        }

        [Fact]
        public void ThrowOnNull_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        if (str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }

        var y = x.ToString();
        var z = c.X;
    }

    public void M2(string str, C c)
    {
        var x = str;
        if (str == null || c == null)
        {
            throw new ArgumentException();
        }

        var y = x.ToString();
        var z = c.X;
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(ByVal str As String, ByVal c As C)
        Dim x = str
        If str Is Nothing Then
            Throw New ArgumentNullException(NameOf(str))
        End If

        If c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If

        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Public Sub M2(ByVal str As String, ByVal c As C)
        Dim x = str
        If str Is Nothing OrElse c Is Nothing Then
            Throw New ArgumentException()
        End If

        Dim y = x.ToString()
        Dim z = c.X
    End Sub
End Class");
        }

        [Fact]
        public void ThrowOnNullForSomeParameter_HazardousUsageForDifferentParameter_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        if (str == null)
        {
            throw new System.ArgumentNullException(nameof(str));
        }

        var x = str;
        var y = x.ToString();

        var z = c.X;
    }

    public void M2(string str, C c)
    {
        var x = str;
        if (str == null)
        {
            if (c == null)
            {
                throw new System.ArgumentNullException(nameof(c));
            }

            throw new System.ArgumentNullException(nameof(str));
        }

        var y = x.ToString();

        var z = c.X;
    }
}
",
            // Test0.cs(19,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(19, 17, "void Test.M1(string str, C c)", "c"),
            // Test0.cs(37,17): warning CA1062: In externally visible method 'void Test.M2(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(37, 17, "void Test.M2(string str, C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        If str Is Nothing Then
            Throw New System.ArgumentNullException(NameOf(str))
        End If

        Dim x = str
        Dim y = x.ToString()

        Dim z = c.X
    End Sub

    Public Sub M2(str As String, c As C)
        Dim x = str
        If str Is Nothing Then
            If c Is Nothing Then
                Throw New System.ArgumentNullException(NameOf(c))
            End If

            Throw New System.ArgumentNullException(NameOf(str))
        End If

        Dim y = x.ToString()

        Dim z = c.X
    End Sub
End Class
",
            // Test0.vb(15,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(15, 17, "Sub Test.M1(str As String, c As C)", "c"),
            // Test0.vb(30,17): warning CA1062: In externally visible method 'Sub Test.M2(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(30, 17, "Sub Test.M2(str As String, c As C)", "c"));
        }

        [Fact]
        public void ThrowOnNull_AfterHazardousUsages_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        var z = c.X;
        if (c == null)
        {
            throw new System.ArgumentNullException(nameof(c));
        }
    }
}
",
            // Test0.cs(11,17): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(11, 17, "void Test.M1(C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim z = c.X
        If c Is Nothing Then
            Throw New System.ArgumentNullException(NameOf(c))
        End If
    End Sub
End Class
",
            // Test0.vb(8,17): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(8, 17, "Sub Test.M1(c As C)", "c"));
        }

        [Fact]
        public void NullCoalescingThrowExpressionOnNull_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str ?? throw new ArgumentNullException(nameof(str));
        var y = x.ToString();

        c = c ?? throw new ArgumentNullException(nameof(c));
        var z = c.X;
    }
}
");
            // Throw expression not supported for VB.
        }

        [Fact]
        public void ThrowOnNull_UncommonNullCheckSyntax_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        if (null == str)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (null == c)
        {
            throw new ArgumentNullException(nameof(c));
        }

        var y = x.ToString();
        var z = c.X;
    }

    public void M2(string str, C c)
    {
        var x = str;
        if ((object)str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (null == (object)c)
        {
            throw new ArgumentNullException(nameof(c));
        }

        var y = x.ToString();
        var z = c.X;
    }

    public void M3(string str, C c)
    {
        var x = str;
        object myNullObject = null;
        if (str == myNullObject || myNullObject == c)
        {
            throw new ArgumentException();
        }

        var y = x.ToString();
        var z = c.X;
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(ByVal str As String, ByVal c As C)
        Dim x = str
        If Nothing Is str Then
            Throw New ArgumentNullException(NameOf(str))
        End If

        If Nothing Is c Then
            Throw New ArgumentNullException(NameOf(c))
        End If

        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Public Sub M2(ByVal str As String, ByVal c As C)
        Dim x = str
        If DirectCast(str, System.Object) Is Nothing Then
            Throw New ArgumentNullException(NameOf(str))
        End If

        If Nothing Is CType(c, System.Object) Then
            Throw New ArgumentNullException(NameOf(c))
        End If

        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Public Sub M3(ByVal str As String, ByVal c As C)
        Dim x = str
        Dim myNullObject As System.Object = Nothing
        If str Is myNullObject OrElse myNullObject Is c Then
            Throw New ArgumentException()
        End If

        Dim y = x.ToString()
        Dim z = c.X
    End Sub
End Class");
        }

        [Fact]
        public void ContractCheck_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        System.Diagnostics.Contracts.Contract.Requires(x != null);
        System.Diagnostics.Contracts.Contract.Requires(c != null);
        var y = str.ToString();
        var z = c.X;
    }

    public void M2(string str, C c)
    {
        var x = str;
        System.Diagnostics.Contracts.Contract.Requires(x != null && c != null);
        var y = str.ToString();
        var z = c.X;
    }

    public void M3(C c1, C c2)
    {
        System.Diagnostics.Contracts.Contract.Requires(c1 != null && c1 == c2);
        var z = c1.X + c2.X;
    }

    void M4_Assume(C c)
    {
        System.Diagnostics.Contracts.Contract.Assume(c != null);
        var z = c.X;
    }

    void M5_Assert(C c)
    {
        System.Diagnostics.Contracts.Contract.Assert(c != null);
        var z = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C

    Public X As Integer
End Class

Public Class Test

    Public Sub M1(str As String, c As C)
        Dim x = str
        System.Diagnostics.Contracts.Contract.Requires(x IsNot Nothing)
        System.Diagnostics.Contracts.Contract.Requires(c IsNot Nothing)
        Dim y = str.ToString()
        Dim z = c.X
    End Sub

    Public Sub M2(str As String, c As C)
        Dim x = str
        System.Diagnostics.Contracts.Contract.Requires(x IsNot Nothing AndAlso c IsNot Nothing)
        Dim y = str.ToString()
        Dim z = c.X
    End Sub

    Public Sub M3(c1 As C, c2 As C)
        System.Diagnostics.Contracts.Contract.Requires(c1 IsNot Nothing AndAlso c1 Is c2)
        Dim z = c1.X + c2.X
    End Sub

    Private Sub M4_Assume(c As C)
        System.Diagnostics.Contracts.Contract.Assume(c IsNot Nothing)
        Dim z = c.X
    End Sub

    Private Sub M5_Assert(c As C)
        System.Diagnostics.Contracts.Contract.Assert(c IsNot Nothing)
        Dim z = c.X
    End Sub
End Class
");
        }

        [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PredicateAnalysis)]
        [Fact]
        public void ContractCheck_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    private C _c;
    public void M1(string str, C c)
    {
        var x = str;
        System.Diagnostics.Contracts.Contract.Requires(x == null);
        System.Diagnostics.Contracts.Contract.Requires(c == _c);
        var y = str.ToString();
        var z = c.X;
    }

    public void M2(string str, C c)
    {
        var x = str;
        System.Diagnostics.Contracts.Contract.Requires(x != null || c != null);
        var y = str.ToString();
        var z = c.X;
    }

    public void M3(C c1, C c2)
    {
        System.Diagnostics.Contracts.Contract.Requires(c1 == null && c1 == c2);
        var z = c2.X;
    }

    public void M4(C c1, C c2)
    {
        System.Diagnostics.Contracts.Contract.Requires(c1 == null && c1 == c2 && c2 != null);   // Infeasible condition
        var z = c2.X;
    }
}
",
            // Test0.cs(15,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(15, 17, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(16,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(16, 17, "void Test.M1(string str, C c)", "c"),
            // Test0.cs(23,17): warning CA1062: In externally visible method 'void Test.M2(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(23, 17, "void Test.M2(string str, C c)", "str"),
            // Test0.cs(24,17): warning CA1062: In externally visible method 'void Test.M2(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(24, 17, "void Test.M2(string str, C c)", "c"),
            // Test0.cs(30,17): warning CA1062: In externally visible method 'void Test.M3(C c1, C c2)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(30, 17, "void Test.M3(C c1, C c2)", "c2"));

            VerifyBasic(@"
Public Class C

    Public X As Integer
End Class

Public Class Test

    Private _c As C

    Public Sub M1(str As String, c As C)
        Dim x = str
        System.Diagnostics.Contracts.Contract.Requires(x Is Nothing)
        System.Diagnostics.Contracts.Contract.Requires(c Is _c)
        Dim y = str.ToString()
        Dim z = c.X
    End Sub

    Public Sub M2(str As String, c As C)
        Dim x = str
        System.Diagnostics.Contracts.Contract.Requires(x IsNot Nothing OrElse c IsNot Nothing)
        Dim y = str.ToString()
        Dim z = c.X
    End Sub

    Public Sub M3(c1 As C, c2 As C)
        System.Diagnostics.Contracts.Contract.Requires(c1 Is Nothing AndAlso c1 Is c2)
        Dim z = c2.X
    End Sub

    Public Sub M4(c1 As C, c2 As C)
        System.Diagnostics.Contracts.Contract.Requires(c1 Is Nothing AndAlso c1 Is c2 AndAlso c2 IsNot Nothing) ' Infeasible condition
        Dim z = c2.X
    End Sub
End Class",
            // Test0.vb(15,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(15, 17, "Sub Test.M1(str As String, c As C)", "str"),
            // Test0.vb(16,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(16, 17, "Sub Test.M1(str As String, c As C)", "c"),
            // Test0.vb(22,17): warning CA1062: In externally visible method 'Sub Test.M2(str As String, c As C)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(22, 17, "Sub Test.M2(str As String, c As C)", "str"),
            // Test0.vb(23,17): warning CA1062: In externally visible method 'Sub Test.M2(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(23, 17, "Sub Test.M2(str As String, c As C)", "c"),
            // Test0.vb(28,17): warning CA1062: In externally visible method 'Sub Test.M3(c1 As C, c2 As C)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(28, 17, "Sub Test.M3(c1 As C, c2 As C)", "c2"));
        }

        [Fact]
        public void ReturnOnNull_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        if (str == null)
        {
            return;
        }

        if (c == null)
        {
            return;
        }

        var x = str;
        var y = x.ToString();

        var z = c.X;
    }

    public void M2(string str, C c)
    {
        if (str == null || c == null)
        {
            return;
        }

        var x = str;
        var y = x.ToString();

        var z = c.X;
    }

    public void M3(string str, C c)
    {
        if (str == null || c == null)
        {
            return;
        }
        else
        {
            var x = str;
            var y = x.ToString();

            var z = c.X;
        }
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        If str Is Nothing Then
            Return
        End If

        If c Is Nothing Then
            Return
        End If

        Dim x = str
        Dim y = x.ToString()

        Dim z = c.X
    End Sub

    Public Sub M2(str As String, c As C)
        If str Is Nothing OrElse c Is Nothing Then
            Return
        End If

        Dim x = str
        Dim y = x.ToString()

        Dim z = c.X
    End Sub

    Public Sub M3(str As String, c As C)
        If str Is Nothing OrElse c Is Nothing Then
            Return
        Else
            Dim x = str
            Dim y = x.ToString()

            Dim z = c.X
        End If
    End Sub
End Class
");
        }

        [Fact]
        public void ReturnOnNullForSomeParameter_HazardousUsageForDifferentParameter_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        if (str == null)
        {
            return;
        }

        var x = str;
        var y = x.ToString();

        var z = c.X;
    }

    public void M2(string str, C c)
    {
        var x = str;
        if (str == null)
        {
            if (c == null)
            {
                return;
            }

            return;
        }

        var y = x.ToString();

        var z = c.X;
    }
}
",
            // Test0.cs(19,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(19, 17, "void Test.M1(string str, C c)", "c"),
            // Test0.cs(37,17): warning CA1062: In externally visible method 'void Test.M2(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(37, 17, "void Test.M2(string str, C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        If str Is Nothing Then
            Return
        End If

        Dim x = str
        Dim y = x.ToString()

        Dim z = c.X
    End Sub

    Public Sub M2(str As String, c As C)
        Dim x = str
        If str Is Nothing Then
            If c Is Nothing Then
                Return
            End If

            Return
        End If

        Dim y = x.ToString()

        Dim z = c.X
    End Sub
End Class
",
            // Test0.vb(15,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(15, 17, "Sub Test.M1(str As String, c As C)", "c"),
            // Test0.vb(30,17): warning CA1062: In externally visible method 'Sub Test.M2(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(30, 17, "Sub Test.M2(str As String, c As C)", "c"));
        }

        [Fact]
        public void StringIsNullCheck_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    public void M1(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            var y = str.ToString();
        }
    }
}
");

            VerifyBasic(@"
Public Class Test
    Public Sub M1(ByVal str As String)
        If Not String.IsNullOrEmpty(str) Then
            Dim y = str.ToString()
        End If
    End Sub
End Class");
        }

        [Fact]
        public void StringIsNullCheck_WithCopyAnalysis_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class Test
{
    public void M1(string str)
    {
        var x = str;
        if (!string.IsNullOrEmpty(str))
        {
            var y = x.ToString();
        }
    }
}
");

            VerifyBasic(@"
Public Class Test
    Public Sub M1(ByVal str As String)
        Dim x = str
        If Not String.IsNullOrEmpty(str) Then
            Dim y = x.ToString()
        End If
    End Sub
End Class");
        }

        [Fact]
        public void SpecialCase_ExceptionGetObjectData_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.Serialization;

public class MyException : Exception
{
    public MyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
    }
}

public class Test
{
    public void M1(MyException ex, SerializationInfo info, StreamingContext context)
    {
        if (ex != null)
        {
            ex.GetObjectData(info, context);
            var name = info.AssemblyName;
        }
    }

    public void M2(SerializationInfo info, StreamingContext context)
    {
        var ex = new MyException(info, context);
        var name = info.AssemblyName;
    }
}
");

            VerifyBasic(@"
Imports System
Imports System.Runtime.Serialization

Public Class MyException
    Inherits Exception
    Public Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
    End Sub

    Public Overrides Sub GetObjectData(info As SerializationInfo, context As StreamingContext)
    End Sub
End Class

Public Class Test
    Public Sub M1(ex As MyException, info As SerializationInfo, context As StreamingContext)
        If ex IsNot Nothing Then
            ex.GetObjectData(info, context)
            Dim name = info.AssemblyName
        End If
    End Sub

    Public Sub M2(info As SerializationInfo, context As StreamingContext)
        Dim ex = New MyException(info, context)
        Dim name = info.AssemblyName
    End Sub
End Class
");
        }

        [Fact]
        public void NullCheckWithNegationBasedCondition_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        if (!(str == null || !(null != c)))
        {
            var y = x.ToString();
            var z = c.X;
        }        
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(ByVal str As String, ByVal c As C)
        Dim x = str
        If Not (str Is Nothing OrElse Not (Nothing IsNot c)) Then
            Dim y = x.ToString()
            Dim z = c.X
        End If
    End Sub
End Class");
        }

        [Fact]
        public void HazardousUsageInInvokedMethod_PrivateMethod_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c1, C c2)
    {
        M2(c1); // No diagnostic
        M3(c2); // Diagnostic
    }

    private static void M2(C c)
    {
    }

    private static void M3(C c)
    {
        var x = c.X;
    }
}
",
            // Test0.cs(12,12): warning CA1062: In externally visible method 'void Test.M1(C c1, C c2)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(12, 12, "void Test.M1(C c1, C c2)", "c2"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c1 As C, c2 As C)
        M2(c1) ' No diagnostic
        M3(c2) ' Diagnostic
    End Sub

    Private Shared Sub M2(c As C)
    End Sub

    Private Shared Sub M3(c As C)
        Dim x = c.X
    End Sub
End Class
",
            // Test0.vb(9,12): warning CA1062: In externally visible method 'Sub Test.M1(c1 As C, c2 As C)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(9, 12, "Sub Test.M1(c1 As C, c2 As C)", "c2"));
        }

        [Fact, WorkItem(1707, "https://github.com/dotnet/roslyn-analyzers/issues/1707")]
        public void HazardousUsageInInvokedMethod_PrivateMethod_Generic_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c1, C c2)
    {
        M2(c1); // No diagnostic
        M3(c2); // Diagnostic
    }

    private static void M2<T>(T c) where T: C
    {
    }

    private static void M3<T>(T c) where T: C
    {
        var x = c.X;
    }
}
",
            // Test0.cs(12,12): warning CA1062: In externally visible method 'void Test.M1(C c1, C c2)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(12, 12, "void Test.M1(C c1, C c2)", "c2"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c1 As C, c2 As C)
        M2(c1) ' No diagnostic
        M3(c2) ' Diagnostic
    End Sub

    Private Shared Sub M2(Of T As C)(c As T)
    End Sub

    Private Shared Sub M3(Of T As C)(c As T)
        Dim x = c.X
    End Sub
End Class
",
            // Test0.vb(9,12): warning CA1062: In externally visible method 'Sub Test.M1(c1 As C, c2 As C)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(9, 12, "Sub Test.M1(c1 As C, c2 As C)", "c2"));
        }

        [Fact]
        public void HazardousUsageInInvokedMethod_PublicMethod_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c1, C c2)
    {
        M2(c1); // No diagnostic
        M3(c2); // No diagnostic here, diagnostic in M3
    }

    public void M2(C c)
    {
    }

    public void M3(C c)
    {
        var x = c.X;    // Diagnostic
    }
}
",
            // Test0.cs(21,17): warning CA1062: In externally visible method 'void Test.M3(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(21, 17, "void Test.M3(C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c1 As C, c2 As C)
        M2(c1) ' No diagnostic
        M3(c2) ' No diagnostic here, diagnostic in M3
    End Sub

    Public Sub M2(c As C)
    End Sub

    Public Sub M3(c As C)
        Dim x = c.X     ' Diagnostic
    End Sub
End Class
",
            // Test0.vb(16,17): warning CA1062: In externally visible method 'Sub Test.M3(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(16, 17, "Sub Test.M3(c As C)", "c"));
        }

        [Fact, WorkItem(1707, "https://github.com/dotnet/roslyn-analyzers/issues/1707")]
        public void HazardousUsageInInvokedMethod_PublicMethod_Generic_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c1, C c2)
    {
        M2(c1); // No diagnostic
        M3(c2); // No diagnostic here, diagnostic in M3
    }

    public void M2<T>(T c) where T: C
    {
    }

    public void M3<T>(T c) where T: C
    {
        var x = c.X;    // Diagnostic
    }
}
",
            // Test0.cs(21,17): warning CA1062: In externally visible method 'void Test.M3<T>(T c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(21, 17, "void Test.M3<T>(T c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c1 As C, c2 As C)
        M2(c1) ' No diagnostic
        M3(c2) ' No diagnostic here, diagnostic in M3
    End Sub

    Public Sub M2(Of T As C)(c As T)
    End Sub

    Public Sub M3(Of T As C)(c As T)
        Dim x = c.X
    End Sub
End Class
",
            // Test0.vb(16,17): warning CA1062: In externally visible method 'Sub Test.M3(Of T)(c As T)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(16, 17, "Sub Test.M3(Of T)(c As T)", "c"));
        }

        [Fact]
        public void HazardousUsageInInvokedMethod_PrivateMethod_MultipleLevelsDown_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        M2(c); // No diagnostic, currently we do not analyze invocations in invoked method.
    }

    private static void M2(C c)
    {
        M3(c);
    }

    private static void M3(C c)
    {
        var x = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        M2(c) ' No diagnostic, currently we do not analyze invocations in invoked method.
    End Sub

    Private Shared Sub M2(c As C)
        M3(c)
    End Sub

    Private Shared Sub M3(c As C)
        Dim x = c.X
    End Sub
End Class
");
        }

        [Fact]
        public void HazardousUsageInInvokedMethod_WithInvocationCycles_Diagnostic()
        {
            // Code with cyclic call graph to verify we don't analyze indefinitely.
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c1, C c2)
    {
        M2(c1); // No diagnostic
        M3(c2); // Diagnostic
    }

    private static void M2(C c)
    {
        M3(c);
    }

    private static void M3(C c)
    {
        M2(c);
        var x = c.X;
    }
}
",
            // Test0.cs(12,12): warning CA1062: In externally visible method 'void Test.M1(C c1, C c2)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(12, 12, "void Test.M1(C c1, C c2)", "c2"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c1 As C, c2 As C)
        M2(c1) ' No diagnostic
        M3(c2) ' Diagnostic
    End Sub

    Private Shared Sub M2(c As C)
    End Sub

    Private Shared Sub M3(c As C)
        Dim x = c.X
    End Sub
End Class
",
            // Test0.vb(9,12): warning CA1062: In externally visible method 'Sub Test.M1(c1 As C, c2 As C)', validate parameter 'c2' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(9, 12, "Sub Test.M1(c1 As C, c2 As C)", "c2"));
        }

        [Fact]
        public void HazardousUsageInInvokedMethod_InvokedAfterValidation_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        if (c != null)
        {
            M2(c); // No diagnostic
        }
    }

    private static void M2(C c)
    {
        var x = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        If c IsNot Nothing Then
            M2(c) ' No diagnostic
        End If
    End Sub

    Private Shared Sub M2(c As C)
        Dim x = c.X
    End Sub
End Class
");
        }

        [Fact]
        public void ValidatedInInvokedMethod_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        M2(c); // Validation method
        var x = c.X;    // No diagnostic here.
    }

    private static void M2(C c)
    {
        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        M2(c) ' Validation method
        Dim x = c.X
    End Sub

    Private Shared Sub M2(c As C)
        If c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If
    End Sub
End Class");
        }

        [Fact, WorkItem(1707, "https://github.com/dotnet/roslyn-analyzers/issues/1707")]
        public void ValidatedInInvokedMethod_Generic_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        M2(c); // Validation method
        var x = c.X;    // No diagnostic here.
    }

    private static void M2<T>(T c) where T: class
    {
        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        M2(c) ' Validation method
        Dim x = c.X    ' No diagnostic here.
    End Sub

    Private Shared Sub M2(Of T As Class)(c As T)
        If c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If
    End Sub
End Class");
        }

        [Fact]
        public void MaybeValidatedInInvokedMethod_Diagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    private bool _flag;

    public void M1(C c)
    {
        M2(c); // Validation method - validates 'c' on some paths.
        var x = c.X;    // Diagnostic.
    }

    private void M2(C c)
    {
        if (_flag && c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }
    }
}
",
            // Test0.cs(16,17): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(16, 17, "void Test.M1(C c)", "c"));

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Private _flag As Boolean

    Public Sub M1(c As C)
        M2(c) ' Validation method - validates 'c' on some paths.
        Dim x = c.X     ' Diagnostic
    End Sub

    Private Sub M2(c As C)
        If _flag AndAlso c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If
    End Sub
End Class",
            // Test0.vb(13,17): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(13, 17, "Sub Test.M1(c As C)", "c"));
        }

        [Fact]
        public void ValidatedButNoExceptionThrownInInvokedMethod_Diagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        M2(c);
        var x = c.X;    // Diagnostic.
    }

    public void M2(C c)
    {
        if (c == null)
        {
            return;
        }

        var x = c.X;    // No Diagnostic.
    }
}
",
            // Test0.cs(14,17): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(14, 17, "void Test.M1(C c)", "c"));

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        M2(c)
        Dim x = c.X     ' Diagnostic
    End Sub

    Public Sub M2(c As C)
        If c Is Nothing Then
            Return
        End If

        Dim x = c.X     ' No Diagnostic
    End Sub
End Class",
            // Test0.vb(11,17): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(11, 17, "Sub Test.M1(c As C)", "c"));
        }

        [Fact]
        public void ValidatedInInvokedMethod_AfterHazardousUsage_Diagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        var x = c.X;    // Diagnostic.
        M2(c); // Validation method
    }

    private static void M2(C c)
    {
        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }
    }
}
",
            // Test0.cs(13,17): warning CA1062: In externally visible method 'void Test.M1(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(13, 17, "void Test.M1(C c)", "c"));

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        Dim x = c.X     ' Diagnostic
        M2(c) ' Validation method
    End Sub

    Private Shared Sub M2(c As C)
        If c Is Nothing Then
            Throw New ArgumentNullException(NameOf(c))
        End If
    End Sub
End Class",
            // Test0.vb(10,17): warning CA1062: In externally visible method 'Sub Test.M1(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(10, 17, "Sub Test.M1(c As C)", "c"));
        }

        [Fact]
        public void WhileLoop_NullCheckInCondition_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        while (str != null && c != null)
        {
            var y = x.ToString();
            var z = c.X;
        }
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Sub M1(str As String, c As C)
        Dim x = str
        While str IsNot Nothing AndAlso c IsNot Nothing
            Dim y = x.ToString()
            Dim z = c.X
        End While
    End Sub
End Class");
        }

        [Fact]
        public void WhileLoop_NullCheckInCondition_HazardousUsageOnExit_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;
        while (str != null && c != null)
        {
            var y = x.ToString();
            var z = c.X;
        }

        x = str.ToString();
        var z2 = c.X;
    }
}
",
            // Test0.cs(18,13): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(18, 13, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(19,18): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(19, 18, "void Test.M1(string str, C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str
        While str IsNot Nothing AndAlso c IsNot Nothing
            Dim y = x.ToString()
            Dim z = c.X
        End While

        x = str.ToString()
        Dim z2 = c.X
    End Sub
End Class",
            // Test0.vb(14,13): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(14, 13, "Sub Test.M1(str As String, c As C)", "str"),
            // Test0.vb(15,18): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(15, 18, "Sub Test.M1(str As String, c As C)", "c"));
        }

        [Fact]
        public void ForLoop_NullCheckInCondition_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        for (var x = str; str != null && c != null;)
        {
            var y = x.ToString();
            var z = c.X;
        }
    }
}
");
        }

        [Fact]
        public void ForLoop_NullCheckInCondition_HazardousUsageOnExit_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        for (var x = str; str != null;)
        {
            var y = x.ToString();
            var z = c.X;    // Diagnostic
        }

        var x2 = str.ToString();    // Diagnostic
        var z2 = c.X;
    }
}
",
            // Test0.cs(14,21): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(14, 21, "void Test.M1(string str, C c)", "c"),
            // Test0.cs(17,18): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(17, 18, "void Test.M1(string str, C c)", "str"));
        }

        [Fact]
        public void LocalFunctionInvocation_EmptyBody_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;

        void MyLocalFunction()
        {
        };

        MyLocalFunction();    // This should not change state of parameters.
        var y = x.ToString();
        var z = c.X;
    }
}
",
            // Test0.cs(18,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(18, 17, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(19,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(19, 17, "void Test.M1(string str, C c)", "c"));

            // VB has no local functions.
        }

        [Fact]
        public void LocalFunction_HazardousUsagesInBody_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;

        void MyLocalFunction()
        {
            // Below should fire diagnostics.
            var y = x.ToString();
            var z = c.X;
        };

        MyLocalFunction();
        MyLocalFunction(); // Do not fire duplicate diagnostics
    }
}
",
            // Test0.cs(16,21): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(16, 21, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(17,21): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(17, 21, "void Test.M1(string str, C c)", "c"));

            // VB has no local functions.
        }

        [Fact]
        public void LambdaInvocation_EmptyBody_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;

        System.Action myLambda = () =>
        {
        };

        myLambda();    // This should not change state of parameters.
        var y = x.ToString();
        var z = c.X;
    }
}
",
            // Test0.cs(18,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(18, 17, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(19,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(19, 17, "void Test.M1(string str, C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str

        Dim myLambda As System.Action = Sub()
                                        End Sub

        myLambda()      ' This should not change state of parameters.
        Dim y = x.ToString()
        Dim z = c.X
    End Sub
End Class",
            // Test0.vb(14,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(14, 17, "Sub Test.M1(str As String, c As C)", "str"),
            // Test0.vb(15,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(15, 17, "Sub Test.M1(str As String, c As C)", "c"));
        }

        [Fact]
        public void Lambda_HazardousUsagesInBody_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;

        System.Action myLambda = () =>
        {
            // Below should fire diagnostics.
            var y = x.ToString();
            var z = c.X;
        };

        myLambda();
        myLambda(); // Do not fire duplicate diagnostics
    }
}
",
            // Test0.cs(16,21): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(16, 21, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(17,21): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(17, 21, "void Test.M1(string str, C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str

        Dim myLambda As System.Action = Sub()
                                            ' Below should fire diagnostics.
                                            Dim y = x.ToString()
                                            Dim z = c.X
                                        End Sub

        myLambda()
        myLambda() ' Do not fire duplicate diagnostics
    End Sub
End Class",
            // Test0.vb(12,53): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(12, 53, "Sub Test.M1(str As String, c As C)", "str"),
            // Test0.vb(13,53): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(13, 53, "Sub Test.M1(str As String, c As C)", "c"));
        }

        [Fact]
        public void DelegateInvocation_ValidatedArguments_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;

        System.Action<string, C> myDelegate = M2;
        myDelegate(x, c);

        var y = x.ToString();
        var z = c.X;
    }

    private void M2(string x, C c)
    {
        if (x == null)
        {
            throw new ArgumentNullException(nameof(x));
        }

        if (c == null)
        {
            throw new ArgumentNullException(nameof(c));
        }
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str

        Dim myDelegate As System.Action(Of String, C) = AddressOf M2
        myDelegate(x, c)

        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Private Sub M2(x As String, c As C)
        If x Is Nothing Then
            Throw New System.ArgumentNullException(NameOf(x))
        End If

        If c Is Nothing Then
            Throw New System.ArgumentNullException(NameOf(c))
        End If
    End Sub
End Class");
        }

        [Fact]
        public void DelegateInvocation_EmptyBody_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;

        System.Action<string, C> myDelegate = M2;
        myDelegate(x, c);

        var y = x.ToString();
        var z = c.X;
    }

    private void M2(string x, C c)
    {
    }
}
",
            // Test0.cs(16,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(16, 17, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(17,17): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(17, 17, "void Test.M1(string str, C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str

        Dim myDelegate As System.Action(Of String, C) = AddressOf M2
        myDelegate(x, c)

        Dim y = x.ToString()
        Dim z = c.X
    End Sub

    Private Sub M2(x As String, c As C)
    End Sub
End Class",
            // Test0.vb(13,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(13, 17, "Sub Test.M1(str As String, c As C)", "str"),
            // Test0.vb(14,17): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(14, 17, "Sub Test.M1(str As String, c As C)", "c"));
        }

        [Fact]
        public void DelegateInvocation_HazardousUsagesInBody_Diagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(string str, C c)
    {
        var x = str;

        System.Action<string, C> myDelegate = M2;
        myDelegate(x, c);
    }

    private void M2(string x, C c)
    {
        var y = x.ToString();
        var z = c.X;
    }
}
",
            // Test0.cs(14,20): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(14, 20, "void Test.M1(string str, C c)", "str"),
            // Test0.cs(14,23): warning CA1062: In externally visible method 'void Test.M1(string str, C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(14, 23, "void Test.M1(string str, C c)", "c"));

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str

        Dim myDelegate As System.Action(Of String, C) = AddressOf M2
        myDelegate(x, c)
    End Sub

    Private Sub M2(x As String, c As C)
        Dim y = x.ToString()
        Dim z = c.X
    End Sub
End Class",
            // Test0.vb(11,20): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'str' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(11, 20, "Sub Test.M1(str As String, c As C)", "str"),
            // Test0.vb(11,23): warning CA1062: In externally visible method 'Sub Test.M1(str As String, c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(11, 23, "Sub Test.M1(str As String, c As C)", "c"));
        }

        [Fact]
        public void TryCast_NoDiagnostic()
        {
            VerifyCSharp(@"
public class A
{
}

public class B : A
{
}

public class Test
{
    public void M1(A a)
    {
        if (a is B)
        {
        }

        if (a is B b)
        {
        }

        var c = a as B;
    }
}
");

            VerifyBasic(@"
Public Class A
End Class

Public Class B
    Inherits A
End Class
Public Class Test
    Public Sub M1(a As A)
        If TypeOf(a) Is B Then
        End If

        Dim b = TryCast(a, b)
    End Sub
End Class");
        }

        [Fact]
        public void DirectCastToObject_BeforeNullCheck_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        if ((object)c == null)
        {
            return;
        }

        var x = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        If DirectCast(c, Object) Is Nothing Then
            Return
        End If

        Dim x = c.X
    End Sub
End Class");
        }

        [Fact]
        public void StaticObjectReferenceEquals_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        if (ReferenceEquals(c, null))
        {
            return;
        }

        var x = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        If ReferenceEquals(c, Nothing) Then
            Return
        End If

        Dim x = c.X
    End Sub
End Class");
        }

        [Fact]
        public void StaticObjectEquals_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c)
    {
        if (object.Equals(c, null))
        {
            return;
        }

        var x = c.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C)
        If Object.Equals(c, Nothing) Then
            Return
        End If

        Dim x = c.X
    End Sub
End Class");
        }

        [Fact]
        public void ObjectEquals_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;
}

public class Test
{
    public void M1(C c, C c2)
    {
        if (c == null || !c.Equals(c2))
        {
            return;
        }

        var x = c2.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c As C, c2 As C)
        If c Is Nothing OrElse Not c.Equals(c2) Then
            Return
        End If

        Dim x = c2.X
    End Sub
End Class");
        }

        [Fact]
        public void ObjectEqualsOverride_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
public class C
{
    public int X;

    public override bool Equals(object other) => true;
}

public class Test
{
    public void M1(C c, C c2)
    {
        if (c == null || !c.Equals(c2))
        {
            return;
        }

        var x = c2.X;
    }
}
");

            VerifyBasic(@"
Public Class C
    Public X As Integer

    Public Overrides Function Equals(other As Object) As Boolean
        Return True
    End Function
End Class

Public Class Test
    Public Sub M1(c As C, c2 As C)
        If c Is Nothing OrElse Not c.Equals(c2) Then
            Return
        End If

        Dim x = c2.X
    End Sub
End Class");
        }

        [Fact]
        public void IEquatableEquals_ExplicitImplementation_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C : IEquatable<C>
{
    public int X;

    bool IEquatable<C>.Equals(C other) => true;
}

public class Test
{
    public void M1(C c, C c2)
    {
        if (c == null || !c.Equals(c2))
        {
            return;
        }

        var x = c2.X;
    }
}
");

            VerifyBasic(@"
Imports System

Public Class C
    Implements IEquatable(Of C)

    Public X As Integer
    Public Function Equals(other As C) As Boolean Implements IEquatable(Of C).Equals
        Return True
    End Function
End Class

Public Class Test
    Public Sub M1(c As C, c2 As C)
        If c Is Nothing OrElse Not c.Equals(c2) Then
            Return
        End If

        Dim x = c2.X
    End Sub
End Class");
        }

        [Fact]
        public void IEquatableEquals_ImplicitImplementation_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public class C : IEquatable<C>
{
    public int X;

    public bool Equals(C other) => true;
}

public class Test
{
    public void M1(C c, C c2)
    {
        if (c == null || !c.Equals(c2))
        {
            return;
        }

        var x = c2.X;
    }
}
");
        }

        [Fact]
        public void IEquatableEquals_Override_BeforeHazardousUsages_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;

public abstract class MyEquatable<T> : IEquatable<T>
{
    public abstract bool Equals(T other);
}

public class C : MyEquatable<C>
{
    public int X;
    public override bool Equals(C other) => true;
}

public class Test
{
    public void M1(C c, C c2)
    {
        if (c == null || !c.Equals(c2))
        {
            return;
        }

        var x = c2.X;
    }
}
");
        }

        [Fact, WorkItem(1852, "https://github.com/dotnet/roslyn-analyzers/issues/1852")]
        public void Issue1852()
        {
            VerifyCSharp(@"
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Messaging;

namespace Blah
{
    public class Program
    {
        delegate object Des(Stream s);

        public object Deserialize(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return DoDeserialization(formatter.Deserialize, new MemoryStream(bytes));
        }

        private object DoDeserialization(Des des, Stream stream)
        {
            return des(stream);
        }
    }
}");
        }

        [Fact, WorkItem(1856, "https://github.com/dotnet/roslyn-analyzers/issues/1856")]
        public void PointsToDataFlowOperationVisitor_VisitInstanceReference_Assert()
        {
            VerifyCSharp(@"
using System;
using System.Xml.Linq;
 namespace Blah
{
    public class ContentContext
    {
        public XElement Data { get; set; }
         public XElement Element(string elementName)
        {
            var element = Data.Element(elementName);
            if (element == null)
            {
                element = new XElement(elementName);
                Data.Add(element);
            }
            return element;
        }
    }
     public interface IDef
    {
        string Name { get; }
    }
     public interface IContent
    {
        T As<T>();
        IDef Definition { get; }
    }
     public class Container
    {
        private XElement _element;
         private void SetElement(XElement value)
        {
            _element = value;
        }
         public XElement Element
        {
            get
            {
                return _element ?? (_element = new XElement(""Data""));
            }
        }
         public string Data
        {
            get
            {
                return _element == null ? null : Element.ToString(SaveOptions.DisableFormatting);
            }
            set
            {
                SetElement(string.IsNullOrEmpty(value) ? null : XElement.Parse(value, LoadOptions.PreserveWhitespace));
            }
        }
    }
     public class ContainerPart
    {
        public Container Container;
        public Container VersionContainer;
    }
     public abstract class Idk<TContent> where TContent : IContent, new()
    {
        public static void ExportInfo(TContent part, ContentContext context)
        {
            var containerPart = part.As<ContainerPart>();
             if (containerPart == null)
            {
                return;
            }
             Action<XElement, bool> exportInfo = (element, versioned) => {
                if (element == null)
                {
                    return;
                }
                 var elementName = GetContainerXmlElementName(part, versioned);
                foreach (var attribute in element.Attributes())
                {
                    context.Element(elementName).SetAttributeValue(attribute.Name, attribute.Value);
                }
            };
             exportInfo(containerPart.VersionContainer.Element.Element(part.Definition.Name), true);
            exportInfo(containerPart.Container.Element.Element(part.Definition.Name), false);
        }
         private static string GetContainerXmlElementName(TContent part, bool versioned)
        {
            return part.Definition.Name + ""-"" + (versioned ? ""VersionInfoset"" : ""Infoset"");
        }
    }
}",
            // Test0.cs(77,21): warning CA1062: In externally visible method 'void Idk<TContent>.ExportInfo(TContent part, ContentContext context)', validate parameter 'context' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(77, 21, "void Idk<TContent>.ExportInfo(TContent part, ContentContext context)", "context"));
        }

        [Fact, WorkItem(1856, "https://github.com/dotnet/roslyn-analyzers/issues/1856")]
        public void InvocationThroughAnUninitializedLocalInstance()
        {
            VerifyCSharp(@"
public class C
{
    private int _field;
    public void M(C c)
    {
        C c2;
        c2.M2(c);
    }

    private void M2(C c)
    {
        var x = c._field;
    }
}
", validationMode: TestValidationMode.AllowCompileErrors, expected:
            // Test0.cs(8,15): warning CA1062: In externally visible method 'void C.M(C c)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(8, 15, "void C.M(C c)", "c"));
        }

        [Fact, WorkItem(1870, "https://github.com/dotnet/roslyn-analyzers/issues/1870")]
        public void Issue1870()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Reflection;
 namespace ANamespace {
    public interface IInterface {
    }
     public class PropConvert {
        public static IInterface ToSettings(object o) {
            if (IsATypeOfSomeSort(o.GetType())) {
                dynamic b = new PropBag();
                 foreach (var p in o.GetType().GetProperties()) {
                    b[p.Name] = p.GetValue(o, null);
                }
                 return b;
            }
             return null;
        }
         private static bool IsATypeOfSomeSort(Type type) {
            return type.IsGenericType
                && Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);
        }
    }
     public class PropBag : DynamicObject, IInterface {
        internal readonly Dictionary<string, IInterface> _properties = new Dictionary<string, IInterface>();
         public static dynamic New() {
            return new PropBag();
        }
         public void SetMember(string name, object value) {
            if (value == null && _properties.ContainsKey(name)) {
                _properties.Remove(name);
            }
            else {
                _properties[name] = PropConvert.ToSettings(value);
            }
        }
    }
}",
            // Test0.cs(14,36): warning CA1062: In externally visible method 'IInterface PropConvert.ToSettings(object o)', validate parameter 'o' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetCSharpResultAt(14, 36, "IInterface PropConvert.ToSettings(object o)", "o"));
        }

        [Fact, WorkItem(1870, "https://github.com/dotnet/roslyn-analyzers/issues/1870")]
        public void Issue1870_02()
        {
            VerifyCSharp(@"
using System;
using System.Collections.Generic;

namespace ANamespace
{
    public static class SomeExtensions
    {
        public static Dictionary<string, string> Merge(this Dictionary<string, string> dictionary, Dictionary<string, string> dictionaryToMerge) {
            if (dictionaryToMerge == null)
                return dictionary;

            var newDictionary = new Dictionary<string, string>(dictionary);

            foreach (var valueDictionary in dictionaryToMerge)
                newDictionary[valueDictionary.Key] = valueDictionary.Value;

            return newDictionary;
        }
    }
}");
        }
    }
}
