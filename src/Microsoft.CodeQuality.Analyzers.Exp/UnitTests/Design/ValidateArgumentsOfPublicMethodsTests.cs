// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeQuality.Analyzers.Exp.Design;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.Exp.UnitTests.Design
{
    using Verify = CSharpCodeFixVerifier<ValidateArgumentsOfPublicMethods, EmptyCodeFixProvider>;
    using VerifyVB = VisualBasicCodeFixVerifier<ValidateArgumentsOfPublicMethods, EmptyCodeFixProvider>;

    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.ParameterValidationAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.CopyAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.NullAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PointsToAnalysis)]
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.PredicateAnalysis)]
    public partial class ValidateArgumentsOfPublicMethodsTests
    {
        private DiagnosticResult GetCSharpResultAt(int line, int column, string methodSignature, string parameterName) =>
            Verify.Diagnostic(ValidateArgumentsOfPublicMethods.Rule).WithLocation(line, column).WithArguments(methodSignature, parameterName);

        private DiagnosticResult GetBasicResultAt(int line, int column, string methodSignature, string parameterName) =>
            VerifyVB.Diagnostic(ValidateArgumentsOfPublicMethods.Rule).WithLocation(line, column).WithArguments(methodSignature, parameterName);

        [Fact]
        public async Task ValueTypeParameter_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ReferenceTypeParameter_NoUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
public class Test
{
    public void M1(string str)
    {
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Test
    Public Sub M1(str As String)
    End Sub
End Class");
        }

        [Fact]
        public async Task ReferenceTypeParameter_NoHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task NonExternallyVisibleMethod_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsage_MethodReference_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsage_FieldReference_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsage_PropertyReference_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsage_EventReference_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsage_ArrayElementReference_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task MultipleHazardousUsages_OneReportPerParameter_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsage_OptionalParameter_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ConditionalAccessUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ValidatedNonNullAttribute_PossibleNullRefUsage_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ValidatedNonNullAttribute_PossibleNullRefUsageOnDifferentParam_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task DefiniteSimpleAssignment_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task AssignedToFieldAndValidated_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task AssignedToFieldAndNotValidated_BeforeHazardousUsages_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task MayBeAssigned_BeforeHazardousUsages_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ConditionalButDefiniteNonNullAssigned_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ThrowOnNull_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ThrowOnNullForSomeParameter_HazardousUsageForDifferentParameter_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ThrowOnNull_AfterHazardousUsages_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task NullCoalescingThrowExpressionOnNull_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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
        public async Task ThrowOnNull_UncommonNullCheckSyntax_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ContractCheck_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ContractCheck_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ReturnOnNull_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ReturnOnNullForSomeParameter_HazardousUsageForDifferentParameter_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task StringIsNullCheck_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class Test
    Public Sub M1(ByVal str As String)
        If Not String.IsNullOrEmpty(str) Then
            Dim y = str.ToString()
        End If
    End Sub
End Class");
        }

        [Fact]
        public async Task StringIsNullCheck_WithCopyAnalysis_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task SpecialCase_ExceptionGetObjectData_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task NullCheckWithNegationBasedCondition_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsageInInvokedMethod_PrivateMethod_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsageInInvokedMethod_PublicMethod_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(c1 As C, c2 As C)
        M2(c1) ' No diagnostic
        M3(c2) ' Diagnostic
    End Sub

    Public Sub M2(c As C)
    End Sub

    Public Sub M3(c As C)
        Dim x = c.X
    End Sub
End Class
",
            // Test0.vb(16,17): warning CA1062: In externally visible method 'Sub Test.M3(c As C)', validate parameter 'c' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.
            GetBasicResultAt(16, 17, "Sub Test.M3(c As C)", "c"));
        }

        [Fact]
        public async Task HazardousUsageInInvokedMethod_PrivateMethod_MultipleLevelsDown_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsageInInvokedMethod_WithInvocationCycles_Diagnostic()
        {
            // Code with cyclic call graph to verify we don't analyze indefinitely.
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task HazardousUsageInInvokedMethod_InvokedAfterValidation_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ValidatedInInvokedMethod_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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

        [Fact]
        public async Task MaybeValidatedInInvokedMethod_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ValidatedButNoExceptionThrownInInvokedMethod_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ValidatedInInvokedMethod_AfterHazardousUsage_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task WhileLoop_NullCheckInCondition_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task WhileLoop_NullCheckInCondition_HazardousUsageOnExit_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ForLoop_NullCheckInCondition_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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
        public async Task ForLoop_NullCheckInCondition_HazardousUsageOnExit_Diagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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
        public async Task LocalFunctionInvocation_EmptyBody_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

        MyLocalFunction();    // This should not change state of parameters if we analyzed the local function.
        var y = x.ToString();
        var z = c.X;
    }
}
");

            // VB has no local functions.
        }

        [Fact]
        public async Task LocalFunction_HazardousUsagesInBody_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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
            // Below should fire diagnostics if we analyzed the local function invocation.
            var y = x.ToString();
            var z = c.X;
        };

        MyLocalFunction();
    }
}
");

            // VB has no local functions.
        }

        [Fact]
        public async Task LambdaInvocation_EmptyBody_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

        myLambda();    // This should not change state of parameters if we analyzed the lambda.
        var y = x.ToString();
        var z = c.X;
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str

        Dim myLambda As System.Action = Sub()
                                        End Sub

        myLambda()      ' This should not change state of parameters if we analyzed the lambda.
        Dim y = x.ToString()
        Dim z = c.X
    End Sub
End Class");
        }

        [Fact]
        public async Task Lambda_HazardousUsagesInBody_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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
            // Below should fire diagnostics if we analyzed the lambda invocation.
            var y = x.ToString();
            var z = c.X;
        };

        myLambda();
    }
}
");

            await VerifyVB.VerifyAnalyzerAsync(@"
Public Class C
    Public X As Integer
End Class

Public Class Test
    Public Sub M1(str As String, c As C)
        Dim x = str

        Dim myLambda As System.Action = Sub()
                                            ' Below should fire diagnostics if we analyzed the lambda invocation.
                                            Dim y = x.ToString()
                                            Dim z = c.X
                                        End Sub

        myLambda()
    End Sub
End Class");
        }

        [Fact]
        public async Task TryCast_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task DirectCastToObject_BeforeNullCheck_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task StaticObjectReferenceEquals_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task StaticObjectEquals_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ObjectEquals_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task ObjectEqualsOverride_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task IEquatableEquals_ExplicitImplementation_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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

            await VerifyVB.VerifyAnalyzerAsync(@"
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
        public async Task IEquatableEquals_ImplicitImplementation_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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
        public async Task IEquatableEquals_Override_BeforeHazardousUsages_NoDiagnostic()
        {
            await Verify.VerifyAnalyzerAsync(@"
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
    }
}
