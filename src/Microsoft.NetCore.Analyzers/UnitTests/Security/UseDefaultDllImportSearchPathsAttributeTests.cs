﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.UseDefaultDllImportSearchPathsAttribute,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    // All the test cases use user32.dll as an example,
    // however it is a commonly used system dll and will be influenced by Known Dlls mechanism,
    // which will ignore all the configuration about the search algorithm.
    // Fow now, this rule didn't take Known Dlls into consideration.
    // If it is needed in the future, we can recover this rule.
    public class UseDefaultDllImportSearchPathsAttributeTests
    {
        // It will try to retrieve the MessageBox from user32.dll, which will be searched in a default order.
        [Fact]
        public async Task Test_DllImportAttribute_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(8, 30, UseDefaultDllImportSearchPathsAttribute.UseDefaultDllImportSearchPathsAttributeRule, "MessageBox"));
        }

        [Fact]
        public async Task Test_DllInUpperCase_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(8, 30, UseDefaultDllImportSearchPathsAttribute.UseDefaultDllImportSearchPathsAttributeRule, "MessageBox"));
        }

        [Fact]
        public async Task Test_WithoutDllExtension_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(8, 30, UseDefaultDllImportSearchPathsAttribute.UseDefaultDllImportSearchPathsAttributeRule, "MessageBox"));
        }

        [Fact]
        public async Task Test_DllImportSearchPathAssemblyDirectory_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "AssemblyDirectory"));
        }

        [Fact]
        public async Task Test_UnsafeDllImportSearchPathBits_BitwiseCombination_OneValueIsBad_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.UserDirectories)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}",
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "AssemblyDirectory"));
        }

        [Fact]
        public async Task Test_UnsafeDllImportSearchPathBits_BitwiseCombination_BothIsBad_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "AssemblyDirectory, ApplicationDirectory"));
        }

        [Fact]
        public async Task Test_DllImportSearchPathLegacyBehavior_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "LegacyBehavior"));
        }

        [Fact]
        public async Task Test_DllImportSearchPathUseDllDirectoryForDependencies_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "UseDllDirectoryForDependencies"));
        }

        [Fact]
        public async Task Test_DllImportSearchPathAssemblyDirectory_Assembly_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(10, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "AssemblyDirectory"));
        }

        [Fact]
        public async Task Test_AssemblyDirectory_ApplicationDirectory_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
}",
            GetCSharpResultAt(11, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "ApplicationDirectory"));
        }

        [Fact]
        public async Task Test_ApplicationDirectory_AssemblyDirectory_Diagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
            GetCSharpResultAt(11, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "AssemblyDirectory"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("dotnet_code_quality.CA5393.unsafe_DllImportSearchPath_bits = 2 | 256 | 512")]
        [InlineData("dotnet_code_quality.CA5393.unsafe_DllImportSearchPath_bits = 770")]
        public async Task EditorConfigConfiguration_UnsafeDllImportSearchPathBits_DefaultValue_Diagnostic(string editorConfigText)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}"
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "AssemblyDirectory, ApplicationDirectory"),
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("dotnet_code_quality.CA5393.unsafe_DllImportSearchPath_bits = 2048")]
        public async Task EditorConfigConfiguration_UnsafeDllImportSearchPathBits_NonDefaultValue_Diagnostic(string editorConfigText)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
using System;
using System.Runtime.InteropServices;

class TestClass
{
    [DllImport(""user32.dll"")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

    public void TestMethod()
    {
        MessageBox(new IntPtr(0), ""Hello World!"", ""Hello Dialog"", 0);
    }
}"
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "System32"),
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("dotnet_code_quality.CA5393.unsafe_DllImportSearchPath_bits = 1026")]
        public async Task EditorConfigConfiguration_UnsafeDllImportSearchPathBits_BitwiseCombination_Diagnostic(string editorConfigText)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}"
                    },
                    ExpectedDiagnostics =
                    {
                        GetCSharpResultAt(9, 30, UseDefaultDllImportSearchPathsAttribute.DoNotUseUnsafeDllImportSearchPathRule, "UserDirectories"),
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            }.RunAsync();
        }

        [Fact]
        public async Task Test_NoAttribute_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task Test_DllImportAndDefaultDllImportSearchPathsAttributes_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        [Theory]
        [InlineData("dotnet_code_quality.CA5392.unsafe_DllImportSearchPath_bits = 2 | 1024")]
        [InlineData("dotnet_code_quality.CA5392.unsafe_DllImportSearchPath_bits = DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.UserDirectories")]
        public async Task EditorConfigConfiguration_UnsafeDllImportSearchPathBits_BitwiseCombination_NoDiagnostic(string editorConfigText)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
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
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("dotnet_code_quality.CA5393.unsafe_DllImportSearchPath_bits = 2048")]
        public async Task EditorConfigConfiguration_UnsafeDllImportSearchPathBits_NonDefaultValue_NoDiagnostic(string editorConfigText)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
            @"
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
}"
                    },
                    AdditionalFiles = { (".editorconfig", editorConfigText) }
                },
            }.RunAsync();
        }

        // In this case, [DefaultDllImportSearchPaths] is applied to the assembly.
        // So, this attribute specifies the paths that are used by default to search for any DLL that provides a function for a platform invoke, in any code in the assembly.
        [Fact]
        public async Task Test_DllImportAndAssemblyDefaultDllImportSearchPathsAttributes_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task Test_DefaultDllImportSearchPaths_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task Test_AssemblyDefaultDllImportSearchPaths_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task Test_DllImportAttributeWithAbsolutePath_DefaultDllImportSearchPaths_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task Test_DllImportAttributeWithAbsolutePath_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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
        public async Task Test_UsingNonexistentAbsolutePath_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
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

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule, params string[] arguments)
            => VerifyCS.Diagnostic(rule)
                .WithLocation(line, column)
                .WithArguments(arguments);
    }
}
