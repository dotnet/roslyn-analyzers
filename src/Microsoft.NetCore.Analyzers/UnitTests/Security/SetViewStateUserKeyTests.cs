// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class SetViewStateUserKeyTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void TestSubclassWithoutOnInitMethodDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected void TestMethod (EventArgs e)
    {
        ViewStateUserKey = ""ViewStateUserKey"";
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));

            VerifyBasic(@"
Imports System
Imports System.Web.UI

class TestClass
    Inherits Page
    protected Sub TestMethod (ByVal e As EventArgs)
        ViewStateUserKey = ""ViewStateUserKey""
    End Sub
End Class",
            GetBasicResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }

        [Fact]
        public void TestOverrideModifierWithoutSettingViewStateUserKeyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected override void OnInit (EventArgs e)
    {
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));

            VerifyBasic(@"
Imports System
Imports System.Web.UI

class TestClass
    Inherits Page
    protected Sub OnInit (ByVal e As EventArgs)
    End Sub
End Class",
            GetBasicResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }

        [Fact]
        public void TestNewModifierWithoutSettingViewStateUserKeyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected new void OnInit (EventArgs e)
    {
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }

        [Fact]
        public void TestNoModifierWithoutSettingViewStateUserKeyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected void OnInit (EventArgs e)
    {
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }

        [Fact]
        public void TestOverloadOnInitWithSettingViewStateUserKeyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected internal void OnInit ()
    {
        ViewStateUserKey = ""ViewStateUserKey"";
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }

        [Fact]
        public void TestStaticMethodWithSettingViewStateUserKeyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected static void OnInit (EventArgs e)
    {
        var testClass = new TestClass();
        testClass.ViewStateUserKey = ""ViewStateUserKey"";
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }

        [Fact]
        public void TestSubclassWithSettingPropertyOfLocalObjectDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected override void OnInit (EventArgs e)
    {
        var testClass = new TestClass();
        testClass.ViewStateUserKey = ""ViewStateUserKey"";
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }

        [Fact]
        public void TestSubclassWithSettingPropertyOfWrongClassDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class MyType
{
    public string ViewStateUserKey { get; set; }
}

class TestClass : Page
{
    private MyType _field;

    protected override void OnInit (EventArgs e)
    {
        _field.ViewStateUserKey = ""ViewStateUserKey"";
    }
}",
            GetCSharpResultAt(10, 7, SetViewStateUserKey.Rule, "TestClass"));
        }
        [Fact]
        public void TestSubclassWithSettingWrongPropertyDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    public int ViewStateUserKey { get; set; }

    protected override void OnInit (EventArgs e)
    {
        ViewStateUserKey = 123;
    }
}",
            GetCSharpResultAt(5, 7, SetViewStateUserKey.Rule, "TestClass"));
        }
        [Fact]
        public void TestSubclassWithSettingViewStateUserKeyNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected override void OnInit (EventArgs e)
    {
        ViewStateUserKey = ""ViewStateUserKey"";
    }
}");
        }

        [Fact]
        public void TestNewModifierNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected new void OnInit (EventArgs e)
    {
        ViewStateUserKey = ""ViewStateUserKey"";
    }
}");
        }

        [Fact]
        public void TestWithoutModifierNoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Web.UI;

class TestClass : Page
{
    protected void OnInit (EventArgs e)
    {
        ViewStateUserKey = ""ViewStateUserKey"";
    }
}");
        }

        [Fact]
        public void TestOrdinaryClassWithSettingViewStateUserKeyNoDiagnostic()
        {
            VerifyCSharp(@"
using System;

class TestClass
{
    public string ViewStateUserKey { get; set; }

    protected void OnInit (EventArgs e)
    {
        ViewStateUserKey = ""ViewStateUserKey"";
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new SetViewStateUserKey();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new SetViewStateUserKey();
        }
    }
}
