// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        #region CSharp Tests

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
        var func = new Func<string, int, string>(D);
        asyncResult = func.BeginInvoke(""Value: {0}"", 10, Callback, null);
    }

    private string D(string format, int value)
    {
        return string.Format(format, value);
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

        private object D(string format, int value)
        {
            return string.Format(format, value);
        }

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
