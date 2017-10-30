// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.CodeQuality.Analyzers.QualityGuidelines.UnitTests
{
    public class MarkMembersAsStaticTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new MarkMembersAsStaticAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MarkMembersAsStaticAnalyzer();
        }

        [Fact]
        public void CSharpSimpleMembers()
        {
            VerifyCSharp(@"
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
                GetCSharpResultAt(7, 16, "Method1"),
                GetCSharpResultAt(12, 17, "Method2"),
                GetCSharpResultAt(14, 17, "Method3"),
                GetCSharpResultAt(19, 16, "Method4"),
                GetCSharpResultAt(24, 16, "Property"),
                GetCSharpResultAt(29, 16, "Property2"),
                GetCSharpResultAt(34, 16, "MyProperty"),
                GetCSharpResultAt(40, 56, "CustomEvent"));
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn-analyzers/issues/1333")]
        public void BasicSimpleMembers()
        {
            VerifyBasic(@"
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

    Public ReadOnly Property Property1 As Integer
        Get
            Return 5
        End Get
    End Property

    Public WriteOnly Property Property2 As Integer
        Set
            s_field = Value
        End Set
    End Property

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
End Class
",
                GetBasicResultAt(8, 21, "Method1"),
                GetBasicResultAt(12, 16, "Method2"),
                GetBasicResultAt(15, 16, "Method3"),
                GetBasicResultAt(19, 21, "Method4"),
                GetBasicResultAt(23, 30, "Property1"),
                GetBasicResultAt(29, 31, "Property2"),
                GetBasicResultAt(35, 21, "MyProperty"),
                GetBasicResultAt(44, 25, "CustomEvent"));
        }

        [Fact]
        public void CSharpSimpleMembers_NoDiagnostic()
        {
            VerifyCSharp(@"
public class MembersTests
{
    public MembersTests() { }

    ~MembersTests() { }

    public int x; 

    public int Method1(string name)
    {
        return x;
    }

    public int Method2()
    {
        MembersTests temp = this;
        return temp.x;
    }

    private int backingField;
    public int Prop1 
    { 
        get { return backingField; }
        set { backingField = value; }
    }

    public int AutoProp { get; set; }
    public int GetterOnlyAutoProp { get; }

    public void SomeEventHandler(object sender, System.EventArgs args) { }
}

public class Generic<T>
{
    public void Method1() { }
    
    public int Property
    {
        get { return 5; }
    }
}
");
        }

        [Fact]
        public void BasicSimpleMembers_NoDiagnostic()
        {
            VerifyBasic(@"
Public Class MembersTests
    Public Sub New()
    End Sub

    Protected Overrides Sub Finalize()
    End Sub

    Public x As Integer

    Public Function Method1(name As String) As Integer
        Return x
    End Function

    Public Function Method2() As Integer
        Dim temp As MembersTests = Me
        Return temp.x
    End Function

    Private backingField As Integer
    Public Property Prop1 As Integer
        Get
            Return backingField
        End Get
        Set 
            backingField = Value
        End Set
    End Property

    Public Property AutoProp As Integer
    Public ReadOnly Property GetterOnlyAutoProp As Integer

    Public Sub SomeEventHandler(sender As Object, args As System.EventArgs)
    End Sub
End Class

Public Class Generic(Of T)
    Public Sub Method1()
    End Sub

    Public ReadOnly Property Property1 As Integer
        Get
            Return 5
        End Get
    End Property
End Class
");
        }

        [Fact]
        public void CSharpOverrides_NoDiagnostic()
        {
            VerifyCSharp(@"
public abstract class SpecialCasesTest1
{
    public abstract void AbstractMethod();
}

public interface ISpecialCasesTest
{
    int Calculate(int arg);
}

public class SpecialCasesTest2 : SpecialCasesTest1, ISpecialCasesTest
{
    public virtual void VirtualMethod() {}
 
    public override void AbstractMethod() { }
 
    public int Calculate(int arg) { return arg/2; }
}");
        }

        [Fact]
        public void BasicOverrides_NoDiagnostic()
        {
            VerifyBasic(@"
Public MustInherit Class SpecialCasesTest1
    Public MustOverride Sub AbstractMethod()
End Class

Public Interface ISpecialCasesTest
    Function Calculate(arg As Integer) As Integer
End Interface

Public Class SpecialCasesTest2
    Inherits SpecialCasesTest1
    Implements ISpecialCasesTest

    Public Overridable Sub VirtualMethod()
    End Sub

    Public Overrides Sub AbstractMethod()
    End Sub
    Public Function Calculate(arg As Integer) As Integer Implements ISpecialCasesTest.Calculate
        Return arg / 2
    End Function
End Class
");
        }

        [Fact]
        public void CSharpNoDiagnostic_Attributes()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices; 

namespace System.Web.Services
{
    public class WebMethodAttribute : Attribute { }
}

public class TestInitializeAttribute : Attribute { }
public class TestMethodAttribute : Attribute { }
public class TestCleanupAttribute : Attribute { }

public class Test
{
    [System.Web.Services.WebMethod]
    public void Method1() { }

    [TestInitialize]
    public void Method2() { }

    [TestMethod]
    public void Method3() { }

    [TestCleanup]
    public void Method4() { }

    [ComVisible(true)]
    public void Method5() { }
}

[ComVisible(true)]
public class ComVisibleClass
{
    public void Method1() { }
}
");
        }

        [Fact]
        public void BasicNoDiagnostic_Attributes()
        {
            VerifyBasic(@"
Imports System
Imports System.Runtime.InteropServices

Namespace System.Web.Services
    Public Class WebMethodAttribute
        Inherits Attribute
    End Class
End Namespace

Public Class TestInitializeAttribute
    Inherits Attribute
End Class
Public Class TestMethodAttribute
    Inherits Attribute
End Class
Public Class TestCleanupAttribute
    Inherits Attribute
End Class

Public Class Test
    <System.Web.Services.WebMethod>
    Public Sub Method1()
    End Sub

    <TestInitialize>
    Public Sub Method2()
    End Sub

    <TestMethod>
    Public Sub Method3()
    End Sub

    <TestCleanup>
    Public Sub Method4()
    End Sub

    <ComVisible(True)>
    Public Sub Method5()
    End Sub
End Class

<ComVisible(True)>
Public Class ComVisibleClass
    Public Sub Method1()
    End Sub
End Class
");
        }

        private DiagnosticResult GetCSharpResultAt(int line, int column, string symbolName)
        {
            return GetCSharpResultAt(line, column, MarkMembersAsStaticAnalyzer.Rule, symbolName);
        }

        private DiagnosticResult GetBasicResultAt(int line, int column, string symbolName)
        {
            return GetBasicResultAt(line, column, MarkMembersAsStaticAnalyzer.Rule, symbolName);
        }
    }
}