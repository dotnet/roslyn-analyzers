// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    // All the test cases use user32.dll as an example,
    // however it is a commonly used system dll and will be influenced by Known Dlls mechanism,
    // which will ignore all the configuration about the search algorithm.
    // Fow now, this rule didn't take Known Dlls into consideration.
    // If it is needed in the future, we can recover this rule.
    public class UseDefaultDllImportSearchPathsAttributeTests : DiagnosticAnalyzerTestBase
    {
        // It will try to retrieve the MessageBox from user32.dll, which will be searched in a default order.
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
        public void Test_DllInUpperCase_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.DLL"")]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(8, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_WithoutDllExtension_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32"")]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(8, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

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

        [Fact]
        public void Test_DllImportSearchPathAssemblyDirectory_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DllImportSearchPathLegacyBehavior_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.LegacyBehavior)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DllImportSearchPathUseDllDirectoryForDependencies_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UseDllDirectoryForDependencies)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Fact]
        public void Test_DllImportSearchPathAssemblyDirectory_Assembly_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

[assembly:DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]

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
        public void Test_AssemblyDirectory_ApplicationDirectory_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

[assembly:DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}");
        }

        [Fact]
        public void Test_ApplicationDirectory_AssemblyDirectory_Diagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

[assembly:DefaultDllImportSearchPaths(DllImportSearchPath.ApplicationDirectory)]

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(11, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("dotnet_code_quality.CA5392.unsafe_DllImportSearchPath_bits = 2 | 256")]
        [InlineData("dotnet_code_quality.CA5392.unsafe_DllImportSearchPath_bits = 258")]
        public void EditorConfigConfiguration_UnsafeDllImportSearchPathBits_Diagnostic(string editorConfigText)
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.ApplicationDirectory)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetEditorConfigAdditionalFile(editorConfigText),
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.Rule, "MessageBox"));
        }

        [Theory]
        [InlineData("dotnet_code_quality.CA5392.unsafe_DllImportSearchPath_bits = 2048")]
        public void EditorConfigConfiguration_UnsafeDllImportSearchPathBits_NoDiagnostic(string editorConfigText)
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.ApplicationDirectory)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetEditorConfigAdditionalFile(editorConfigText),
            Array.Empty<DiagnosticResult>());
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

        // [DllImport] is set with an absolute path, which will let the [DefaultDllImportSearchPaths] be ignored.
        [Fact]
        public void Test_DllImportAttributeWithAbsolutePath_DefaultDllImportSearchPaths_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""C:\\Windows\\System32\\user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}");
        }

        // [DllImport] is set with an absolute path.
        [Fact]
        public void Test_DllImportAttributeWithAbsolutePath_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""C:\\Windows\\System32\\user32.dll"")]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}");
        }

        [Fact]
        public void Test_UsingNonexistentAbsolutePath_NoDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""C:\\Nonexistent\\user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
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
