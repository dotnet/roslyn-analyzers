// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Tasks.UnitTests
{
    public class DoNotCallBeginInvokeOnDelegateTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new DoNotCallBeginInvokeOnDelegate();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotCallBeginInvokeOnDelegate();
        }

        #region Basic diagnostic tests

        [Fact]
        public void BasicBeginInvokeOnAction()
        {
            var code = @"
Imports System

Class C
    Public Sub M()
        Dim action = New Action(AddressOf D)
        action.BeginInvoke(AddressOf Callback, Nothing)
    End Sub

    Private Sub D()
        Console.WriteLine(""Test"")
    End Sub

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class
";
            VerifyBasic(code, GetBasicResultAt(7, 9));
        }

        [Fact]
        public void BasicBeginInvokeOnFunc()
        {
            var code = @"
Imports System

Class C
    Private F As Func(Of Object)

    Public Sub M()
        Me.F.BeginInvoke(AddressOf Callback, ""test"")
    End Sub

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class
";
            VerifyBasic(code, GetBasicResultAt(8, 9));
        }

        [Fact]
        public void BasicBeginInvokeOnActionWith2Parameters()
        {
            var code = @"
Imports System

Class C
    Public Sub M()
        Dim action = New Action(Of String, Integer)(AddressOf D)
        action.BeginInvoke(""Value: {0}"", 10, AddressOf Callback, Me)
    End Sub

    Private Sub D(ByVal format As String, ByVal value As Integer)
        Console.WriteLine(format, value)
    End Sub

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class
";
            VerifyBasic(code, GetBasicResultAt(7, 9));
        }

        [Fact]
        public void BasicBeginInvokeOnFuncWith2Parameters()
        {
            var code = @"
Imports System

Class C
    Private AR As IAsyncResult

    Public Sub M()
        Dim func = New Func(Of String, Integer, String)(Function(f, v) String.Format(f, v))
        AR = func.BeginInvoke(""Value: {0}"", 10, AddressOf Callback, Nothing)
    End Sub

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class
";
            VerifyBasic(code, GetBasicResultAt(9, 14));
        }

        [Fact]
        public void BasicBeginInvokeOnCustomDelegate()
        {
            var code = @"
Imports System

Public Delegate Sub D(ByVal sender As Object, ByVal e As EventArgs)

Class C
    Private AR As IAsyncResult

    Public Sub M()
        Dim d = new D(AddressOf H1)
        d = System.Delegate.Combine(d, new D(AddressOf H2))
        AR = d.BeginInvoke(Me, EventArgs.Empty, AddressOf Callback, 10)
    End Sub

    Private Sub H1(ByVal sender As Object, ByVal e As EventArgs)
        Console.WriteLine(e)
    End Sub

    Private Shared Sub H2(ByVal sender As Object, ByVal e As EventArgs)
        Console.WriteLine(e)
    End Sub

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class
";
            VerifyBasic(code, GetBasicResultAt(12, 14));
        }

        #endregion

        #region Basic no diagnostic tests

        [Fact]
        public void BasicInvokeOnAction()
        {
            var code = @"
Imports System

Class C
    Public Sub M()
        Dim action = New Action(AddressOf D)
        action.Invoke()
    End Sub

    Private Sub D()
        Console.WriteLine(""Test"")
    End Sub
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void BasicInvokeOnFunc()
        {
            var code = @"
Imports System

Class C
    Public Sub M()
        Dim func = New Func(Of String, Integer, String)(Function(f, v) String.Format(f, v))
        func.Invoke(""Value: {0}"", 10)
    End Sub
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void BasicInvokeOnCustomDelegate()
        {
            var code = @"
Imports System

Public Delegate Function D(ByVal callback As AsyncCallback, ByVal value As Object) As IAsyncResult

Class C
    Public Sub M()
        Dim d = New D(AddressOf I)
        d.Invoke(AddressOf Callback, 10)
    End Sub

    Private Function I(ByVal callback As AsyncCallback, ByVal value As Object) As IAsyncResult
        Console.WriteLine(value)
        Return Nothing
    End Function

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void BasicCallCustomDelegate()
        {
            var code = @"
Imports System

Public Delegate Function D(ByVal callback As AsyncCallback, ByVal value As Object) As IAsyncResult

Class C
    Private AR As IAsyncResult

    Public Sub M()
        Dim d = New D(AddressOf I)
        AR = d(AddressOf Callback, Nothing)
    End Sub

    Private Function I(ByVal callback As AsyncCallback, ByVal value As Object) As IAsyncResult
        Console.WriteLine(value)
        Return Nothing
    End Function

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void BasicBeginInvokeOnCustomClass()
        {
            var code = @"
Imports System
Imports System.Threading.Tasks

Class C
    Public Sub M()
        Dim t = New T
        t.BeginInvoke(AddressOf Callback, 10)
    End Sub

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class

Class T
    Public Function BeginInvoke(ByVal callback As AsyncCallback, ByVal value As Object) As IAsyncResult
        Return Task.Run(Sub() Console.WriteLine(value)).ContinueWith(Sub(t) callback.Invoke(t))
    End Function
End Class
";
            VerifyBasic(code);
        }

        [Fact]
        public void BasicBeginInvokeOnCustomStruct()
        {
            var code = @"
Imports System
Imports System.Threading.Tasks

Class C
    Public Sub M()
        Dim t = New T
        t.BeginInvoke(AddressOf Callback, 10)
    End Sub

    Private Sub Callback(ByVal ar As IAsyncResult)
        Console.WriteLine(ar.ToString())
    End Sub
End Class

Structure T
    Public Function BeginInvoke(ByVal callback As AsyncCallback, ByVal value As Object) As IAsyncResult
        Return Task.Run(Sub() Console.WriteLine(value)).ContinueWith(Sub(t) callback.Invoke(t))
    End Function
End Structure
";
            VerifyBasic(code);
        }

        #endregion

        #region CSharp diagnostic tests

        [Fact]
        public void CSharpBeginInvokeOnAction()
        {
            var code = @"
using System;

class C
{
    public void M()
    {
        var action = new Action(D);
        action.BeginInvoke(Callback, null);
    }

    private void D()
    {
        Console.WriteLine(""Test"");
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(9, 9));
        }

        [Fact]
        public void CSharpBeginInvokeOnFunc()
        {
            var code = @"
using System;

class C
{
    private Func<object> func;

    public void M()
    {
        this.func.BeginInvoke(Callback, ""test"");
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(10, 9));
        }

        [Fact]
        public void CSharpBeginInvokeOnActionWith2Parameters()
        {
            var code = @"
using System;

class C
{
    public void M()
    {
        var action = new Action<string, int>(D);
        action.BeginInvoke(""Value: {0}"", 10, Callback, this);
    }

    private void D(string format, int value)
    {
        Console.WriteLine(format, value);
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(9, 9));
        }

        [Fact]
        public void CSharpBeginInvokeOnFuncWith2Parameters()
        {
            var code = @"
using System;

class C
{
    private IAsyncResult asyncResult;

    public void M()
    {
        var func = new Func<string, int, string>((f, v) => string.Format(f, v));
        asyncResult = func.BeginInvoke(""Value: {0}"", 10, Callback, null);
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(11, 23));
        }

        [Fact]
        public void CSharpBeginInvokeOnCustomDelegate()
        {
            var code = @"
using System;

public delegate void D(object sender, EventArgs e);

class C
{
    private IAsyncResult asyncResult;

    public void M()
    {
        var d = new D(H1);
        d += new D(H2);
        asyncResult = d.BeginInvoke(this, EventArgs.Empty, Callback, 10);
    }

    private void H1(object sender, EventArgs e)
    {
        Console.WriteLine(e);
    }

    private static void H2(object sender, EventArgs e)
    {
        Console.WriteLine(e);
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(14, 23));
        }

        [Fact]
        public void CSharpBeginInvokeOnEvent()
        {
            var code = @"
using System;

class C
{
    public event EventHandler E;

    public void M()
    {
        E.BeginInvoke(this, EventArgs.Empty, Callback, null);
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code, GetCSharpResultAt(10, 9));
        }

        #endregion

        #region CSharp no diagnostic tests

        [Fact]
        public void CSharpInvokeOnAction()
        {
            var code = @"
using System;

class C
{
    public void M()
    {
        var action = new Action(D);
        action.Invoke();
    }

    private void D()
    {
        Console.WriteLine(""Test"");
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharpInvokeOnFunc()
        {
            var code = @"
using System;

class C
{
    public void M()
    {
        var func = new Func<string, int, string>((f, v) => string.Format(f, v));
        func.Invoke(""Value: {0}"", 10);
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharpInvokeOnCustomDelegate()
        {
            var code = @"
using System;

public delegate IAsyncResult D(AsyncCallback callback, object @object);

class C
{
    public void M()
    {
        var d = new D(I);
        d.Invoke(Callback, 10);
    }

    private IAsyncResult I(AsyncCallback callback, object value)
    {
        Console.WriteLine(value);
        return null;
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharpCallCustomDelegate()
        {
            var code = @"
using System;

public delegate IAsyncResult D(AsyncCallback callback, object @object);

class C
{
    private IAsyncResult asyncResult;

    public void M()
    {
        var d = new D(I);
        asyncResult = d(Callback, null);
    }

    private IAsyncResult I(AsyncCallback callback, object value)
    {
        Console.WriteLine(value);
        return null;
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharpBeginInvokeOnCustomClass()
        {
            var code = @"
using System;
using System.Threading.Tasks;

class C
{
    public void M()
    {
        var t = new T();
        t.BeginInvoke(Callback, 10);
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}

class T
{
    public IAsyncResult BeginInvoke(AsyncCallback callback, object value)
    {
        return Task.Run(() => Console.WriteLine(value))
            .ContinueWith(t => callback.Invoke(t));
    }
}
";
            VerifyCSharp(code);
        }

        [Fact]
        public void CSharpBeginInvokeOnCustomStruct()
        {
            var code = @"
using System;
using System.Threading.Tasks;

class C
{
    public void M()
    {
        var t = default(T);
        t.BeginInvoke(""Value: {0}"", Callback, null);
    }

    private void Callback(IAsyncResult ar)
    {
        Console.WriteLine(ar.ToString());
    }
}

struct T
{
    public IAsyncResult BeginInvoke(string format, AsyncCallback callback, object value)
    {
        return Task.Run(() => Console.WriteLine(format, value))
            .ContinueWith(t => callback(t));
    }
}
";
            VerifyCSharp(code);
        }

        #endregion

        private static DiagnosticResult GetCSharpResultAt(int line, int column)
        {
            return GetCSharpResultAt(line, column, DoNotCallBeginInvokeOnDelegate.RuleId, MicrosoftNetCoreAnalyzersResources.DoNotCallBeginInvokeOnDelegateMessage);
        }

        private static DiagnosticResult GetBasicResultAt(int line, int column)
        {
            return GetBasicResultAt(line, column, DoNotCallBeginInvokeOnDelegate.RuleId, MicrosoftNetCoreAnalyzersResources.DoNotCallBeginInvokeOnDelegateMessage);
        }
    }
}
