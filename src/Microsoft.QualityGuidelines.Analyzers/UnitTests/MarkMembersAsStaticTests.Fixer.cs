// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.QualityGuidelines.Analyzers.UnitTests
{
    public class MarkMembersAsStaticFixerTests : CodeFixTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MarkMembersAsStaticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MarkMembersAsStaticAnalyzer();
        }

        protected override CodeFixProvider GetBasicCodeFixProvider()
        {
            return new MarkMembersAsStaticFixer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MarkMembersAsStaticFixer();
        }

        [Fact]
        public void TestCSharp_SimpleMembers_MadeStatic()
        {
            VerifyCSharpFix(@"
public class MembersTests
{
    internal static int s_field;
    public const int Zero = 0;

    public int Method1(string name)
    {
        return name.Length;
    }

    public void Method2() { }

    public void Method3()
    {
        s_field = 4;
    }

    public int Method4()
    {
        return Zero;
    }

    public int Property
    {
        get { return 5; }
    }

    public int Property2
    {
        set { s_field = value; }
    }

    public int MyProperty
    {
        get { return 10; }
        set { System.Console.WriteLine(value); }
    }

    public event System.EventHandler<System.EventArgs> CustomEvent { add {} remove {} }
}",
@"
public class MembersTests
{
    internal static int s_field;
    public const int Zero = 0;

    public static int Method1(string name)
    {
        return name.Length;
    }

    public static void Method2() { }

    public static void Method3()
    {
        s_field = 4;
    }

    public static int Method4()
    {
        return Zero;
    }

    public static int Property
    {
        get { return 5; }
    }

    public static int Property2
    {
        set { s_field = value; }
    }

    public static int MyProperty
    {
        get { return 10; }
        set { System.Console.WriteLine(value); }
    }

    public static event System.EventHandler<System.EventArgs> CustomEvent { add {} remove {} }
}");
        }

        [Fact]
        public void TestBasic_SimpleMembers_MadeShared()
        {
            VerifyBasicFix(@"
Imports System
Public Class MembersTests
    Shared s_field As Integer
    Public Const Zero As Integer = 0

    Public Function Method1(name As String) As Integer
        Return name.Length
    End Function

    Public Sub Method2()
    End Sub

    Public Sub Method3()
        s_field = 4
    End Sub

    Public Function Method4() As Integer
        Return Zero
    End Function

    Public Property MyProperty As Integer
        Get
            Return 10
        End Get
        Set
            System.Console.WriteLine(Value)
        End Set
    End Property

    Public Custom Event CustomEvent As EventHandler(Of EventArgs)
        AddHandler(value As EventHandler(Of EventArgs))
        End AddHandler
        RemoveHandler(value As EventHandler(Of EventArgs))
        End RemoveHandler
        RaiseEvent(sender As Object, e As EventArgs)
        End RaiseEvent
    End Event
End Class",
@"
Imports System
Public Class MembersTests
    Shared s_field As Integer
    Public Const Zero As Integer = 0

    Public Shared Function Method1(name As String) As Integer
        Return name.Length
    End Function

    Public Shared Sub Method2()
    End Sub

    Public Shared Sub Method3()
        s_field = 4
    End Sub

    Public Shared Function Method4() As Integer
        Return Zero
    End Function

    Public Shared Property MyProperty As Integer
        Get
            Return 10
        End Get
        Set
            System.Console.WriteLine(Value)
        End Set
    End Property

    Public Shared Custom Event CustomEvent As EventHandler(Of EventArgs)
        AddHandler(value As EventHandler(Of EventArgs))
        End AddHandler
        RemoveHandler(value As EventHandler(Of EventArgs))
        End RemoveHandler
        RaiseEvent(sender As Object, e As EventArgs)
        End RaiseEvent
    End Event
End Class");
        }
    }
}