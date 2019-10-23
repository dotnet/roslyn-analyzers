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

        #region CSharp diagnostic tests

        [Fact]
        public void BeginInvokeOnAction()
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
        public void BeginInvokeOnFunc()
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
        public void BeginInvokeOnActionWith2Parameters()
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
        public void BeginInvokeOnFuncWith2Parameters()
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
        public void BeginInvokeOnCustomDelegate()
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
        public void BeginInvokeOnEvent()
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
        public void InvokeOnAction()
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
        public void InvokeOnFunc()
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
        public void InvokeOnCustomDelegate()
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
        public void CallCustomDelegate()
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
        public void BeginInvokeOnCustomClass()
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
        public void BeginInvokeOnCustomStruct()
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
