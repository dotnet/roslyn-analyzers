// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class UseDefaultDllImportSearchPathsAttributeTests : DiagnosticAnalyzerTestBase
    {
        // It will try to retrieve the MessageBox from user32.dll, which will be searched in a default order:
        //  1. The directory from which the application is loaded.
        //  2. The current directory.
        //  3. The system directory, usually C:\\Windows\\System32\\ (The GetSystemDirectory function is called to obtain this directory.).
        //  4. The 16-bit system directory – There is no dedicated function to retrieve the path of this directory, but it is searched as well.
        //  5. The Windows directory. The GetWindowsDirectory function is called to obtain this directory.
        //  6. The directories that are listed in the PATH environment variable
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

        // [DllImport] is set with an absolute path, which will let the [DefaultDllImportSearchPaths] be ignored.
        // So user32.dll will also be searched in the default order.
        [Fact]
        public void Test_DllImportAttributeWithAbsolutePath_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""C:\\\\Windows\\System32\\user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        // It will have a compiler warning and recommend to use [DllImport]. So, there's no need to flag a diagnostic for this case.
        [Fact]
        public void Test_NoAttribute_NoDiagnostic()
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
}");
        }

        // user32.dll will be searched in UserDirectories, which is specified by DllImportSearchPath and is good.
        [Fact]
        public void Test_DllImportAndDefaultDllImportSearchPathsAttributes_NoDiagnostic()
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
}");
        }

        // In this case, [DefaultDllImportSearchPaths] is applied to the assembly.
        // So, this attribute specifies the paths that are used by default to search for any DLL that provides a function for a platform invoke, in any code in the assembly.
        [Fact]
        public void Test_DllImportAndAssemblyDefaultDllImportSearchPathsAttributes_NoDiagnostic()
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
}");
        }

        // It will have a compiler warning and recommend to use [DllImport] also.
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

        // It will have a compiler warning and recommend to use [DllImport] also.
        [Fact]
        public void Test_AssemblyDefaultDllImportSearchPaths_NoDiagnostic()
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
