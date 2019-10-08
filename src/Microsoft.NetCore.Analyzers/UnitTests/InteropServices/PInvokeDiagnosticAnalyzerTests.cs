// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.InteropServices.UnitTests
{
    public class PInvokeDiagnosticAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        #region Verifiers 

        private static readonly string s_CA1401RuleText = MicrosoftNetCoreAnalyzersResources.PInvokesShouldNotBeVisibleMessage;
        private static readonly string s_CA2101RuleText = MicrosoftNetCoreAnalyzersResources.SpecifyMarshalingForPInvokeStringArgumentsTitle;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new PInvokeDiagnosticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PInvokeDiagnosticAnalyzer();
        }

        private static DiagnosticResult CSharpResult1401(int line, int column, string typeName)
        {
            return GetCSharpResultAt(line, column, PInvokeDiagnosticAnalyzer.RuleCA1401Id, string.Format(s_CA1401RuleText, typeName));
        }

        private static DiagnosticResult BasicResult1401(int line, int column, string typeName)
        {
            return GetBasicResultAt(line, column, PInvokeDiagnosticAnalyzer.RuleCA1401Id, string.Format(s_CA1401RuleText, typeName));
        }

        private static DiagnosticResult CSharpResult2101(int line, int column)
        {
            return GetCSharpResultAt(line, column, PInvokeDiagnosticAnalyzer.RuleCA2101Id, s_CA2101RuleText);
        }

        private static DiagnosticResult BasicResult2101(int line, int column)
        {
            return GetBasicResultAt(line, column, PInvokeDiagnosticAnalyzer.RuleCA2101Id, s_CA2101RuleText);
        }

        #endregion

        #region CA1401 tests 

        [Fact]
        public void CA1401CSharpTest()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

public class C
{
    [DllImport(""user32.dll"")]
    public static extern void Foo1(); // should not be public

    [DllImport(""user32.dll"")]
    protected static extern void Foo2(); // should not be protected

    [DllImport(""user32.dll"")]
    private static extern void Foo3(); // private is OK

    [DllImport(""user32.dll"")]
    static extern void Foo4(); // implicitly private is OK
}
",
                CSharpResult1401(7, 31, "Foo1"),
                CSharpResult1401(10, 34, "Foo2"));
        }

        [Fact]
        public void CA1401CSharpTestWithScope()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;

public class C
{
    [DllImport(""user32.dll"")]
    public static extern void Foo1(); // should not be public

    [|[DllImport(""user32.dll"")]
    protected static extern void Foo2(); // should not be protected
    |]
    [DllImport(""user32.dll"")]
    private static extern void Foo3(); // private is OK

    [DllImport(""user32.dll"")]
    static extern void Foo4(); // implicitly private is OK
}
",
                CSharpResult1401(10, 34, "Foo2"));
        }

        [Fact]
        public void CA1401BasicSubTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    <DllImport(""user32.dll"")>
    Public Shared Sub Foo1() ' should not be public
    End Sub

    <DllImport(""user32.dll"")>
    Protected Shared Sub Foo2() ' should not be protected
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo3() ' private is OK
    End Sub

    <DllImport(""user32.dll"")>
    Shared Sub Foo4() ' implicitly public is not OK
    End Sub
End Class
",
                BasicResult1401(6, 23, "Foo1"),
                BasicResult1401(10, 26, "Foo2"),
                BasicResult1401(18, 16, "Foo4"));
        }

        [Fact]
        public void CA1401BasicSubTestWithScope()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    <DllImport(""user32.dll"")>
    Public Shared Sub Foo1() ' should not be public
    End Sub

    [|<DllImport(""user32.dll"")>
    Protected Shared Sub Foo2() ' should not be protected
    End Sub|]

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo3() ' private is OK
    End Sub

    <DllImport(""user32.dll"")>
    Shared Sub Foo4() ' implicitly public is not OK
    End Sub
End Class
",
                BasicResult1401(10, 26, "Foo2"));
        }

        [Fact]
        public void CA1401BasicFunctionTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    <DllImport(""user32.dll"")>
    Public Shared Function Foo1() As Integer ' should not be public
    End Function

    <DllImport(""user32.dll"")>
    Protected Shared Function Foo2() As Integer ' should not be protected
    End Function

    <DllImport(""user32.dll"")>
    Private Shared Function Foo3() As Integer ' private is OK
    End Function

    <DllImport(""user32.dll"")>
    Shared Function Foo4() As Integer ' implicitly public is not OK
    End Function
End Class
",
                BasicResult1401(6, 28, "Foo1"),
                BasicResult1401(10, 31, "Foo2"),
                BasicResult1401(18, 21, "Foo4"));
        }

        [Fact]
        public void CA1401BasicDeclareSubTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    Public Declare Sub Foo1 Lib ""user32.dll"" Alias ""Foo1"" () ' should not be public

    Protected Declare Sub Foo2 Lib ""user32.dll"" Alias ""Foo2"" () ' should not be protected

    Private Declare Sub Foo3 Lib ""user32.dll"" Alias ""Foo3"" () ' private is OK

    Declare Sub Foo4 Lib ""user32.dll"" Alias ""Foo4"" () ' implicitly public is not OK
End Class
",
                BasicResult1401(5, 24, "Foo1"),
                BasicResult1401(7, 27, "Foo2"),
                BasicResult1401(11, 17, "Foo4"));
        }

        [Fact]
        public void CA1401BasicDeclareFunctionTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices

Public Class C
    Public Declare Function Foo1 Lib ""user32.dll"" Alias ""Foo1"" () As Integer ' should not be public

    Protected Declare Function Foo2 Lib ""user32.dll"" Alias ""Foo2"" () As Integer ' should not be protected

    Private Declare Function Foo3 Lib ""user32.dll"" Alias ""Foo3"" () As Integer ' private is OK

    Declare Function Foo4 Lib ""user32.dll"" Alias ""Foo4"" () As Integer ' implicitly public is not OK
End Class
",
                BasicResult1401(5, 29, "Foo1"),
                BasicResult1401(7, 32, "Foo2"),
                BasicResult1401(11, 22, "Foo4"));
        }

        [WorkItem(792, "https://github.com/dotnet/roslyn-analyzers/issues/792")]
        [Fact]
        public void CA1401CSharpNonPublic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

public sealed class TimerFontContainer
{
    private static class NativeMethods
    {
        [DllImport(""gdi32.dll"")]
        public static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);
    }
}
");
        }

        [WorkItem(792, "https://github.com/dotnet/roslyn-analyzers/issues/792")]
        [Fact]
        public void CA1401BasicNonPublic()
        {
            VerifyBasic(@"
Public NotInheritable Class TimerFontContainer
    Private Class NativeMethods
        Public Declare Function AddFontMemResourceEx Lib ""gdi32.dll"" (pbFont As Integer, cbFont As Integer, pdv As Integer) As Integer
    End Class
End Class
");
        }

        #endregion

        #region CA2101 tests 

        [Fact]
        public void CA2101SimpleCSharpTest()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"")]
    private static extern void Foo1(string s); // one string parameter

    [DllImport(""user32.dll"")]
    private static extern void Foo2(string s, string t); // two string parameters, should be only 1 diagnostic

    [DllImport(""user32.dll"")]
    private static extern void Foo3(StringBuilder s); // one StringBuilder parameter

    [DllImport(""user32.dll"")]
    private static extern void Foo4(StringBuilder s, StringBuilder t); // two StringBuilder parameters, should be only 1 diagnostic
}
",
                CSharpResult2101(7, 6),
                CSharpResult2101(10, 6),
                CSharpResult2101(13, 6),
                CSharpResult2101(16, 6));
        }

        [Fact]
        public void CA2101SimpleCSharpTestWithScope()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"")]
    private static extern void Foo1(string s); // one string parameter

    [|[DllImport(""user32.dll"")]
    private static extern void Foo2(string s, string t); // two string parameters, should be only 1 diagnostic|]

    [DllImport(""user32.dll"")]
    private static extern void Foo3(StringBuilder s); // one StringBuilder parameter

    [DllImport(""user32.dll"")]
    private static extern void Foo4(StringBuilder s, StringBuilder t); // two StringBuilder parameters, should be only 1 diagnostic
}
",
                CSharpResult2101(10, 6));
        }

        [Fact]
        public void CA2101SimpleBasicTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"")>
    Private Shared Sub Foo1(s As String) ' one string parameter
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo2(s As String, t As String) ' two string parameters, should be only 1 diagnostic
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo3(s As StringBuilder) ' one StringBuilder parameter
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo4(s As StringBuilder, t As StringBuilder) ' two StringBuilder parameters, should be only 1 diagnostic
    End Sub
End Class
",
                BasicResult2101(6, 6),
                BasicResult2101(10, 6),
                BasicResult2101(14, 6),
                BasicResult2101(18, 6));
        }

        [Fact]
        public void CA2101SimpleBasicTestWithScope()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"")>
    Private Shared Sub Foo1(s As String) ' one string parameter
    End Sub

    [|<DllImport(""user32.dll"")>
    Private Shared Sub Foo2(s As String, t As String) ' two string parameters, should be only 1 diagnostic
    End Sub|]

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo3(s As StringBuilder) ' one StringBuilder parameter
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo4(s As StringBuilder, t As StringBuilder) ' two StringBuilder parameters, should be only 1 diagnostic
    End Sub
End Class
",
                BasicResult2101(10, 6));
        }

        [Fact]
        public void CA2101SimpleDeclareBasicTest()
        {
            VerifyBasic(@"
Imports System.Text

Class C
    Private Declare Sub Foo1 Lib ""user32.dll"" (s As String) ' one string parameter

    Private Declare Sub Foo2 Lib ""user32.dll"" (s As String, t As String) ' two string parameters, should be only 1 diagnostic

    Private Declare Function Foo3 Lib ""user32.dll"" (s As StringBuilder) As Integer ' one StringBuilder parameter

    Private Declare Function Foo4 Lib ""user32.dll"" (s As StringBuilder, t As StringBuilder) As Integer ' two StringBuilder parameters, should be only 1 diagnostic
End Class
",
                BasicResult2101(5, 25),
                BasicResult2101(7, 25),
                BasicResult2101(9, 30),
                BasicResult2101(11, 30));
        }

        [Fact]
        public void CA2101ParameterMarshaledCSharpTest()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"")]
    private static extern void Foo1([MarshalAs(UnmanagedType.LPWStr)] string s); // marshaling specified on parameter

    [DllImport(""user32.dll"")]
    private static extern void Foo2([MarshalAs(UnmanagedType.LPWStr)] StringBuilder s);

    [DllImport(""user32.dll"")]
    private static extern void Foo3([MarshalAs(UnmanagedType.LPWStr)] string s, [MarshalAs(UnmanagedType.LPWStr)] string t);

    [DllImport(""user32.dll"")]
    private static extern void Foo4([MarshalAs(UnmanagedType.LPWStr)] StringBuilder s, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder t);

    [DllImport(""user32.dll"")]
    private static extern void Foo5([MarshalAs(UnmanagedType.LPWStr)] string s, string t); // un-marshaled second parameter

    [DllImport(""user32.dll"")]
    private static extern void Foo6([MarshalAs(UnmanagedType.LPWStr)] StringBuilder s, StringBuilder t);

    [DllImport(""user32.dll"")]
    private static extern void Foo7([MarshalAs(UnmanagedType.LPStr)] string s); // marshaled, but as the wrong type

    [DllImport(""user32.dll"")]
    private static extern void Foo8([MarshalAs(UnmanagedType.LPStr)] StringBuilder s);

    [DllImport(""user32.dll"")]
    private static extern void Foo9([MarshalAs(UnmanagedType.LPStr)] string s, [MarshalAs(UnmanagedType.LPStr)] string t); // two parameters marshaled as the wrong type

    [DllImport(""user32.dll"")]
    private static extern void Foo10([MarshalAs(UnmanagedType.LPStr)] StringBuilder s, [MarshalAs(UnmanagedType.LPStr)] StringBuilder t);

    [DllImport(""user32.dll"")]
    private static extern void Foo11([MarshalAs((short)0)] string s);
}
",
                CSharpResult2101(19, 6),
                CSharpResult2101(22, 6),
                CSharpResult2101(26, 38),
                CSharpResult2101(29, 38),
                CSharpResult2101(32, 38),
                CSharpResult2101(32, 81),
                CSharpResult2101(35, 39),
                CSharpResult2101(35, 89),
                CSharpResult2101(38, 39));
        }

        [Fact]
        public void CA2101ParameterMarshaledBasicTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"")>
    Private Shared Sub Foo1(<MarshalAs(UnmanagedType.LPWStr)> s As String) ' marshaling specified on parameter
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo2(<MarshalAs(UnmanagedType.LPWStr)> s As StringBuilder)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo3(<MarshalAs(UnmanagedType.LPWStr)> s As String, <MarshalAs(UnmanagedType.LPWStr)> t As String)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo4(<MarshalAs(UnmanagedType.LPWStr)> s As StringBuilder, <MarshalAs(UnmanagedType.LPWStr)> t As StringBuilder)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo5(<MarshalAs(UnmanagedType.LPWStr)> s As String, t As String) ' un-marshaled second parameter
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo6(<MarshalAs(UnmanagedType.LPWStr)> s As StringBuilder, t As StringBuilder)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo7(<MarshalAs(UnmanagedType.LPStr)> s As String) ' marshaled, but as the wrong type
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo8(<MarshalAs(UnmanagedType.LPStr)> s As StringBuilder)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo9(<MarshalAs(UnmanagedType.LPStr)> s As String, <MarshalAs(UnmanagedType.LPStr)> t As String) ' two parameters marshaled as the wrong type
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo10(<MarshalAs(UnmanagedType.LPStr)> s As StringBuilder, <MarshalAs(UnmanagedType.LPStr)> t As StringBuilder)
    End Sub

    <DllImport(""user32.dll"")>
    Private Shared Sub Foo11(<MarshalAs(CShort(0))> s As String)
    End Sub
End Class
",
                BasicResult2101(22, 6),
                BasicResult2101(26, 6),
                BasicResult2101(31, 30),
                BasicResult2101(35, 30),
                BasicResult2101(39, 30),
                BasicResult2101(39, 76),
                BasicResult2101(43, 31),
                BasicResult2101(43, 84),
                BasicResult2101(47, 31));
        }

        [Fact]
        public void CA2101CharSetCSharpTest()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"", CharSet = CharSet.Auto)]
    private static extern void Foo1(string s); // wrong marshaling

    [DllImport(""user32.dll"", CharSet = CharSet.Auto)]
    private static extern void Foo2(StringBuilder s);

    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern void Foo3(string s); // correct marshaling

    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern void Foo4(StringBuilder s);

    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern void Foo5([MarshalAs(UnmanagedType.LPStr)] string s); // correct marshaling on method, not on parameter

    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern void Foo6([MarshalAs(UnmanagedType.LPStr)] StringBuilder s);
}
",
                CSharpResult2101(7, 6),
                CSharpResult2101(10, 6),
                CSharpResult2101(20, 38),
                CSharpResult2101(23, 38));
        }

        [Fact]
        public void CA2101CharSetBasicTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"", CharSet := CharSet.Auto)>
    Private Shared Sub Foo1(s As String) ' wrong marshaling
    End Sub

    <DllImport(""user32.dll"", CharSet := CharSet.Auto)>
    Private Shared Sub Foo2(s As StringBuilder)
    End Sub

    <DllImport(""user32.dll"", CharSet := CharSet.Unicode)>
    Private Shared Sub Foo3(s As String) ' correct marshaling
    End Sub

    <DllImport(""user32.dll"", CharSet := CharSet.Unicode)>
    Private Shared Sub Foo4(s As StringBuilder)
    End Sub

    <DllImport(""user32.dll"", CharSet := CharSet.Unicode)>
    Private Shared Sub Foo5(<MarshalAs(UnmanagedType.LPStr)> s As String) ' correct marshaling on method, not on parameter
    End Sub

    <DllImport(""user32.dll"", CharSet := CharSet.Unicode)>
    Private Shared Sub Foo6(<MarshalAs(UnmanagedType.LPStr)> s As StringBuilder)
    End Sub
End Class
",
                BasicResult2101(6, 6),
                BasicResult2101(10, 6),
                BasicResult2101(23, 30),
                BasicResult2101(27, 30));
        }

        [Fact]
        public void CA2101ReturnTypeCSharpTest()
        {
            VerifyCSharp(@"
using System.Runtime.InteropServices;
using System.Text;

class C
{
    [DllImport(""user32.dll"")]
    private static extern string Foo1(); // wrong marshaling on return type

    [DllImport(""user32.dll"")]
    private static extern StringBuilder Foo2();

    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern string Foo3(); // correct marshaling on return type

    [DllImport(""user32.dll"", CharSet = CharSet.Unicode)]
    private static extern StringBuilder Foo4();
}
",
                CSharpResult2101(7, 6),
                CSharpResult2101(10, 6));
        }

        [Fact]
        public void CA2101ReturnTypeBasicTest()
        {
            VerifyBasic(@"
Imports System.Runtime.InteropServices
Imports System.Text

Class C
    <DllImport(""user32.dll"")>
    Private Shared Function Foo1() As String ' wrong marshaling on return type
    End Function

    <DllImport(""user32.dll"")>
    Private Shared Function Foo2() As StringBuilder
    End Function

    <DllImport(""user32.dll"", CharSet := CharSet.Unicode)>
    Private Shared Function Foo3() As String ' correct marshaling on return type
    End Function

    <DllImport(""user32.dll"", CharSet := CharSet.Unicode)>
    Private Shared Function Foo4() As StringBuilder
    End Function

    Private Declare Function Foo5 Lib ""user32.dll"" () As String
End Class
",
                BasicResult2101(6, 6),
                BasicResult2101(10, 6),
                BasicResult2101(22, 30));
        }

        #endregion
    }
}
