// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseDefaultDllImportSearchPathsAttributeTests : DiagnosticAnalyzerTestBase
    {
        [Fact]
        public void Test_NoAttribute_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(7, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DllImportAttribute_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(8, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DllImportAndDefaultDllImportSearchPathsAttributes_ApplyOnDifferentMethods_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);


    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static extern int AnotherMessageBox(IntPtr hWnd, String text, String caption, uint type);

}",
            GetCSharpResultAt(8, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DllImportAndDefaultDllImportSearchPathsAttributes_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DllImportAndGlobalDefaultDllImportSearchPathsAttributes_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

[assembly:DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]

class TestClass
{
    [DllImport(""user32.dll"")]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(10, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DefaultDllImportSearchPaths_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public void Test_GlobalDefaultDllImportSearchPaths_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

[assembly:DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]

class TestClass
{
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
    }
}");
        }

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new UseDefaultDllImportSearchPathsAttribute();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseDefaultDllImportSearchPathsAttribute();
        }
    }
}
